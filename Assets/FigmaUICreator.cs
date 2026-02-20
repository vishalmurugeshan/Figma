using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using TMPro;

public static class NodesVariables
{
    public const string panel = "PANEL"; 
    public const string image = "IMG";
    public const string imageWithBtn = "IMG_BTN";
    public const string txt = "TXT";

    public const string txtType = "TEXT";
}
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

    [SerializeField]TMP_FontAsset fontAsset;
    #region Figma UI Creation


    public void CreateCanvasSkeleton()
    {
        ResetAll();
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
        {
            TraverseNode(node, canvasRT, null);
        }
    }


    void TraverseNode(FigmaNode node, RectTransform parentRT, FigmaNode parentNode)
    {
        if (node == null || node.absoluteBoundingBox == null)
            return;

        RectTransform currentParent = parentRT;

        Debug.Log(" Creationg node " + node.name + " " + node.type);

        if ( IsValidNode(node) || isFirst)
        {
            isFirst = false;
            GameObject go = (node.type.Equals(NodesVariables.txtType))?CreateUIObject(NodesVariables.txt):CreateUIObject(node.name);
            if (go != null)
            {
                go.transform.SetParent(parentRT, false);

                RectTransform rt = go.GetComponent<RectTransform>();
                SetupRectTransform(rt, node, parentNode);

                if (node.type.Equals(NodesVariables.txtType))
                {
                    Debug.Log("Created text");
                    SetupText(node.name.ToUpper(),go,node.style.fontSize);
                }
                else
                {
                    if (node.name == NodesVariables.image || node.name == NodesVariables.imageWithBtn)
                        SetupImage(go, node);
                }
                currentParent = rt;
            }
        }

        if (node.children == null) return;

        foreach (var child in node.children)
            TraverseNode(child, currentParent, node);
    }
    
    bool IsValidNode(FigmaNode node)
    {
        return (node.type.Equals(NodesVariables.txtType) || node.name.Equals(NodesVariables.panel)
            || node.name.Equals(NodesVariables.image) || node.name.Equals(NodesVariables.imageWithBtn));
    }
    GameObject CreateUIObject(string nodeName)
    {
        switch (nodeName)
        {
            case NodesVariables.panel:
                return new GameObject(nodeName, typeof(RectTransform));

            case NodesVariables.image:
                return new GameObject(nodeName, typeof(RectTransform), typeof(Image));

            case NodesVariables.imageWithBtn:
                return new GameObject(nodeName, typeof(RectTransform), typeof(Image), typeof(Button));
            case NodesVariables.txt:
                return new GameObject(nodeName, typeof(RectTransform), typeof(TextMeshProUGUI));
            default:
                return new GameObject(nodeName, typeof(RectTransform));
        }
    }

    void SetupRectTransform(RectTransform rt, FigmaNode node, FigmaNode parentNode)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);

        rt.sizeDelta = new Vector2(
            node.absoluteBoundingBox.width,
            node.absoluteBoundingBox.height
        );

        rt.anchoredPosition = GetLocalPosition(node, parentNode);
    }
    void SetupText(string text,GameObject go,float fontSize=18)
    {
        TMP_Text txt=go.GetComponent<TMP_Text>();
        
        if (txt != null)
        {
            txt.text = text;
            if (fontAsset != null)
            {
                txt.font = fontAsset;
            }
            txt.fontSize = fontSize;
            txt.rectTransform.sizeDelta=new Vector2(txt.text.Length*fontSize, fontSize);
        }
    }
    void SetupImage(GameObject go, FigmaNode node)
    {
        Image img = go.GetComponent<Image>();
        img.AddComponent<UiData>().nodeData = node;

        img.color = new Color(0, 1, 0, 0.5f); // semi-transparent green

        ImageNodeData ind = new ImageNodeData
        {
            uiData = img.GetComponent<UiData>(),
            image = img
        };

        canvasDataRuntimeAsset.imageNodes.Add(ind);
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