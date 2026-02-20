using UnityEngine;
using System;

#region ROOT

[Serializable]
public class FigmaResponse
{
    public FigmaDocument document;
}
public static class FigmaJsonConverter
{
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

[System.Serializable]
public class ImageNodeData
{
    public UiData uiData;
    public UnityEngine.UI.Image image;

    public void SetImage(Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
        }
    }
}

#endregion

#region DOCUMENT / CANVAS

[Serializable]
public class FigmaDocument
{
    public string id;
    public string name;
    public string type; // DOCUMENT / CANVAS
    public string scrollBehavior;
    public FigmaNode[] children;
}

#endregion

#region NODE (RECTANGLE ONLY)

[Serializable]
public class FigmaNode
{
    public string id;
    public string name;
    public string type; // RECTANGLE, FRAME, GROUP
    public bool visible;


    public BoundingBox absoluteBoundingBox;
    public Constraints constraints;
    public FigmaFill[] fills;
    public ExportSetting[] exportSettings;
    public Style style;

    public FigmaNode[] children;
}

#endregion

#region GEOMETRY

[Serializable]
public class BoundingBox
{
    public float x;
    public float y;
    public float width;
    public float height;
}

#endregion

#region FILL (SOLID / IMAGE)

[Serializable]
public class FigmaFill
{
    public string type; // SOLID / IMAGE
    public FigmaColor color;

    // IMAGE only
    public string imageRef;
    public string scaleMode;
}

[Serializable]
public class FigmaColor
{
    public float r;
    public float g;
    public float b;
    public float a;
}

[Serializable]
public class Style
{
    public string fontFamily;
    public int fontSize;
}

#endregion

#region EXPORT

[Serializable]
public class ExportSetting
{
    public string format; // PNG
}

[Serializable]
public class Constraints
{
    public string vertical;
    public string horizontal;
}

#endregion
