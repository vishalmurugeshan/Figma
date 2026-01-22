using UnityEngine;
using System;

#region ROOT

[Serializable]
public class FigmaResponse
{
    public FigmaDocument document;
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
