using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class FigmaLoader : MonoBehaviour
{
    public string figmaJson;

    [SerializeField]FigmaResponse Response;
    [SerializeField] Canvas canvas;
    void Start()
    {
        Create();
    }
    [ContextMenu("Create")]
    void Create()
    {
        Response =
           FigmaJsonConverter.FromJson(figmaJson);

        if (Response == null) return;

        Traverse(Response.document);
    }

    [ContextMenu("Clear")]
    void Clear()
    {
        if(canvas.transform.childCount>0)
            for(int i=0;i<canvas.transform.childCount;i++)
                DestroyImmediate(canvas.transform.GetChild(i).gameObject);
    }
    void Traverse(FigmaDocument doc)
    {
        if (doc.children == null) return;

        foreach (var node in doc.children)
            TraverseNode(node,canvas.GetComponent<RectTransform>());
    }

    void TraverseNode(FigmaNode node, RectTransform parent)
    {
        if (node == null )
            return;

        // Only RECTANGLES
        if ((node.type == "FRAME" || node.type=="GROUP" || node.type=="RECTANGLE") && node.absoluteBoundingBox != null)
        {
            // Create GameObject
            GameObject go = new GameObject(node.name, typeof(RectTransform), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            go.transform.SetParent(parent, false);

            float width = node.absoluteBoundingBox.width;
            float height = node.absoluteBoundingBox.height;

            // TOP-LEFT anchor & pivot
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            // Figma → Unity (invert Y)
            float posX = node.absoluteBoundingBox.x;
            float posY = -node.absoluteBoundingBox.y;

            rt.anchoredPosition = new Vector2(posX, posY);
            rt.sizeDelta = new Vector2(width, height);

            // Optional: Image setup
            Image img = go.GetComponent<Image>();
            img.AddComponent<UiData>().nodeData = node;
            if (node.type == "FRAME")
                img.color = Color.red;
            if(node.type == "GROUP")
                img.color = Color.green;
            if(node.type=="RECTANGLE")
                img.color= Color.blue;

            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);
            // img.sprite = yourSprite;
            // img.preserveAspect = true;
        }
        
        if (node.children == null)
            return;

        foreach (var child in node.children)
            TraverseNode(child, parent);
    }
}

public static class FigmaJsonConverter
{
    /// <summary>
    /// Converts raw JSON string to FigmaResponse object
    /// </summary>
    public static FigmaResponse FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Figma JSON string is null or empty");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<FigmaResponse>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse Figma JSON\n" + e);
            return null;
        }
    }
}
