using System.IO;
using UnityEditor.PackageManager.Requests;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class FigmaImageHandler : MonoBehaviour
{
    UnityWebRequest request;

    [SerializeField] FigmaImageUrlResponse response;
    
    private bool isDownloading = false; 
    /// <summary>
    /// the json string received from Figma Image API
    /// </summary>
    /// <param name="amazonS3json"></param>
    public void DownloadImage(string amazonS3json)
    {
        if(isDownloading)
        {
            Debug.LogWarning("Image download already in progress.");
            return;
        }
        List<FigmaImageDownloadedPath> figmaImageDownloadedPath = new List<FigmaImageDownloadedPath>();
        response = JsonConvert.DeserializeObject<FigmaImageUrlResponse>(amazonS3json);
        StartCoroutine(DownloadHelper(figmaImageDownloadedPath));
    }
    public void AbortDownload()
    {
        if (isDownloading && request != null)
        {
            request.Abort();
            isDownloading = false;
            StopAllCoroutines();
            Debug.Log("Image download aborted.");
        }
        else
        {
            Debug.LogWarning("No download in progress to abort.");
        }
    }
    IEnumerator DownloadHelper(List<FigmaImageDownloadedPath> allfigmaImageDownloadedPath)
    {
        isDownloading = true;

        string folderPath = "Assets/REGAL_ASSET";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        int i = 1;
        List<string> importedPaths = new List<string>();

        foreach (var urlData in response.images)
        {
            Debug.Log($"Image ID: {urlData.Key}, URL: {urlData.Value}");

            string fileName = "IMG" + (i++) + ".png";
            string fullPath = Path.Combine(folderPath, fileName);

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(urlData.Value))
            {
                request.timeout = 20;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"Download failed\n" +
                        $"Result: {request.result}\n" +
                        $"Error: {request.error}\n" +
                        $"Response Code: {request.responseCode}"
                    );
                    continue;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                byte[] pngData = tex.EncodeToPNG();
                File.WriteAllBytes(fullPath, pngData);
                Object.DestroyImmediate(tex);

                allfigmaImageDownloadedPath.Add(new FigmaImageDownloadedPath
                {
                    figmaImageId = urlData.Key,
                    path = fullPath
                });

                importedPaths.Add(fullPath);
                Debug.Log("Image saved to: " + fullPath);
            }
            Debug.Log((i-1).ToString()+"/"+response.images.Count+" "+"completed");
            yield return null;
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
        foreach (string path in importedPaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
#endif

        FindAnyObjectByType<CanvasDataRuntimeAsset>().downloadedImagePaths =
            new List<FigmaImageDownloadedPath>(allfigmaImageDownloadedPath);

        Debug.Log("All images downloaded successfully.");
        isDownloading = false;
    }

    public void OnReset()
    {
        response = null;
    }
}

[System.Serializable]
public class FigmaImageUrlResponse
{
    [JsonProperty("err")]
    public object err;

    [JsonProperty("images")]
    public Dictionary<string, string> images;

    public List<UrlData> url_data=new List<UrlData>();
}
[System.Serializable]
public class UrlData
{
    public string imgId;
    public string url;

    public UrlData(string id, string url)
    {
        this.imgId = id;
        this.url = url;
    }
}

[System.Serializable]
public class FigmaImageDownloadedPath
{
    public string figmaImageId;
    public string path;
}