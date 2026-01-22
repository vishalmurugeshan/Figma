using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

public class FigmaImageDownloaderEditor : EditorWindow
{
    private string imageUrl;
    private string fileName = "DownloadedImage.png";

    private UnityWebRequest request;

    [MenuItem("Tools/Figma/Download Image")]
    static void Open()
    {
        GetWindow<FigmaImageDownloaderEditor>("Figma Image Downloader");
    }

    void OnGUI()
    {
        GUILayout.Label("Download Image from URL", EditorStyles.boldLabel);

        imageUrl = EditorGUILayout.TextField("Image URL", imageUrl);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        GUI.enabled = request == null;

        if (GUILayout.Button("Download & Save to Assets"))
        {
            StartDownload();
        }

        GUI.enabled = true;
    }

    void StartDownload()
    {
        request = UnityWebRequestTexture.GetTexture(imageUrl);
        request.SendWebRequest();

        EditorApplication.update += EditorUpdate;
    }

    void EditorUpdate()
    {
        if (request == null)
            return;

        // ⏳ wait until fully finished
        if (!request.isDone)
            return;

        EditorApplication.update -= EditorUpdate;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"Download failed\n" +
                $"Result: {request.result}\n" +
                $"Error: {request.error}\n" +
                $"Response Code: {request.responseCode}"
            );

            request.Dispose();
            request = null;
            return;
        }

        // ✅ SUCCESS
        Texture2D tex = DownloadHandlerTexture.GetContent(request);
        byte[] pngData = tex.EncodeToPNG();

        string folderPath = "Assets/DownloadedImages";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, pngData);

        AssetDatabase.Refresh();

        TextureImporter importer =
            AssetImporter.GetAtPath(fullPath) as TextureImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();

        Debug.Log("Image saved to: " + fullPath);

        request.Dispose();
        request = null;
    }
}
