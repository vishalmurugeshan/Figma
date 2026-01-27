using System.Collections.Generic;
using UnityEngine;
public class CanvasDataRuntimeAsset : MonoBehaviour
{

    [Header("Image Nodes Data")]
    public List<ImageNodeData> imageNodes = new List<ImageNodeData>();

    [Header("A comma-separated string of image IDs for easy reference.can be used while using figma image api")]
    public string imageIdsString = "";

    [Header("Downloaded Images Paths")]
    public List<FigmaImageDownloadedPath> downloadedImagePaths = new List<FigmaImageDownloadedPath>();

    public void OnReset()
    {
        imageNodes.Clear();
        imageIdsString = "";
        downloadedImagePaths.Clear();
    }

}