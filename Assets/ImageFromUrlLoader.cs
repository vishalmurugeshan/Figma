using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class ImageFromUrlLoader : MonoBehaviour
{
    [Header("Paste the image URL here")]
    [TextArea]
    public string imageUrl;

    [Header("Target (one is enough)")]
    public Image uiImage;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        if (!string.IsNullOrEmpty(imageUrl))
        {
            StartCoroutine(LoadImage(imageUrl));
        }
        else
        {
            Debug.LogWarning("Image URL is empty");
        }
    }

    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image download failed: " + request.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            if (uiImage != null)
                uiImage.sprite = sprite;

            if (spriteRenderer != null)
                spriteRenderer.sprite = sprite;
        }
    }
}
