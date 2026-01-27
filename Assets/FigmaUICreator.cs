using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;

public class FigmaUICreator : MonoBehaviour
{
    [Header("Figma File API response json string")]
    [SerializeField]string figmaFileJson;

    [Header("Figma Image API response json string")]
    [SerializeField]string figmaImageJson;
    Canvas canvas;
    bool isFirst = true;

    CanvasDataRuntimeAsset canvasDataRuntimeAsset;
    FigmaImageHandler figmaImageDownloader;

    [SerializeField] FigmaResponse Response;

    
    #region Figma UI Creation


    public void CreateCanvasSkeleton()
    {
        CheckDepedency();
        canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        Create();
        canvasDataRuntimeAsset.imageIdsString =string.Empty;
        foreach (var imgNode in canvasDataRuntimeAsset.imageNodes)
        {
            canvasDataRuntimeAsset.imageIdsString += imgNode.uiData.nodeData.id + ",";
        }
    }
    public void DownloadImage()
    {
        figmaImageDownloader.DownloadImage(figmaImageJson);
    }
    public void AbortImageDownload()
    {
        figmaImageDownloader.AbortDownload();
    }
    public void AssignImages()
    {
        foreach (var imgNode in canvasDataRuntimeAsset.imageNodes)
        {
            foreach (var downloadedPath in canvasDataRuntimeAsset.downloadedImagePaths)
            {
                if (imgNode.uiData.nodeData.id == downloadedPath.figmaImageId)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(downloadedPath.path);
                    imgNode.SetImage(sprite);
                }
            }
        }
    }
    public void ResetAll()
    {
        if(canvas != null)
            DestroyImmediate(canvas.gameObject);
        canvasDataRuntimeAsset?.OnReset();
        figmaImageDownloader?.OnReset();
    }
    void CheckDepedency()
    {
        GameObject _temp;
        figmaImageDownloader = FindAnyObjectByType<FigmaImageHandler>();
        canvasDataRuntimeAsset = FindAnyObjectByType<CanvasDataRuntimeAsset>();
        if (figmaImageDownloader == null)
        {
            _temp = new GameObject("FigmaImageDownloader", typeof(FigmaImageHandler));
            figmaImageDownloader = _temp.GetComponent<FigmaImageHandler>();
        }
        if (canvasDataRuntimeAsset == null)
        {
            _temp = new GameObject("CanvasDataRuntimeAsset", typeof(CanvasDataRuntimeAsset));
            canvasDataRuntimeAsset = _temp.GetComponent<CanvasDataRuntimeAsset>();
        }
    }
    void Create()
    {
        isFirst = true;
        Response = FigmaJsonConverter.FromJson(figmaFileJson);
        if (Response == null) return;
        Traverse(Response.document);
    }

    void Traverse(FigmaDocument doc)
    {
        if (doc.children == null) return;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        foreach (var node in doc.children)
            TraverseNode(node, canvasRT, null);
    }

    void TraverseNode(FigmaNode node, RectTransform parentRT, FigmaNode parentNode)
    {
        if (node == null || node.absoluteBoundingBox == null)
            return;

        RectTransform rt = parentRT;

        // Only create UI for these nodes
        if (node.name == "PANEL" || node.name == "IMG" || isFirst)
        {
            isFirst = false;
            GameObject go = null;
            if (node.name == "PANEL")
            {
                go = new GameObject(node.name, typeof(RectTransform));
            }
            else
            {
                go = new GameObject(node.name, typeof(RectTransform), typeof(Image));
            }
            go.transform.SetParent(parentRT, false);

            rt = go.GetComponent<RectTransform>();

            // Anchor & pivot (Top-Left like Figma)
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            // Size
            rt.sizeDelta = new Vector2(
                node.absoluteBoundingBox.width,
                node.absoluteBoundingBox.height
            );

            // ✅ Correct local position
            rt.anchoredPosition = GetLocalPosition(node, parentNode);

            // Image setup

            if (node.name == "IMG")
            {
                Image img = go.GetComponent<Image>();
                img.AddComponent<UiData>().nodeData = node;

                //if (node.name == "PANEL") img.color = Color.red;
                if (node.name == "IMG") img.color = Color.green;

                img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);

                ImageNodeData ind = new ImageNodeData();
                ind.uiData = img.GetComponent<UiData>();
                ind.image = img;
                canvasDataRuntimeAsset.imageNodes.Add(ind);
            }
        }

        // Traverse children
        if (node.children == null) return;

        foreach (var child in node.children)
            TraverseNode(child, rt, node);
    }

    /// <summary>
    /// Converts Figma absolute position to Unity local position
    /// </summary>
    Vector2 GetLocalPosition(FigmaNode node, FigmaNode parentNode)
    {
        if (parentNode == null || parentNode.absoluteBoundingBox == null)
        {
            return new Vector2(
                node.absoluteBoundingBox.x,
                -node.absoluteBoundingBox.y
            );
        }

        float localX = node.absoluteBoundingBox.x - parentNode.absoluteBoundingBox.x;
        float localY = -(node.absoluteBoundingBox.y - parentNode.absoluteBoundingBox.y);

        return new Vector2(localX, localY);
    }
    #endregion

    void CreateImage()
    {

    }
}

[CustomEditor(typeof(FigmaUICreator))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FigmaUICreator script = (FigmaUICreator)target;

        if (GUILayout.Button("Create Canvas skeleton"))
        {
            script.CreateCanvasSkeleton();
        }
        if (GUILayout.Button("Download Images"))
        {
            script.DownloadImage();
        }
        if(GUILayout.Button("Abort Image Download"))
        {
            script.AbortImageDownload();
        }
        if(GUILayout.Button("Assign Images"))
        {
            script.AssignImages();
        }
        if (GUILayout.Button("Reset All"))
        {
            script.ResetAll();
        }

    }
}