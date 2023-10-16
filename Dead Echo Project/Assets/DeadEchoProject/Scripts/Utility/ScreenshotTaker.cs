using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    #region - Singleton Pattern -
    public static ScreenshotTaker Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;
    }
    #endregion

    public string SaveScreenshot(string fullPath, string saveName)
    {
        string newPath = Path.Combine(fullPath, $"{saveName}_lastPhoto.png");
        
        if (File.Exists(newPath))
            File.Delete(newPath);

        ScreenCapture.CaptureScreenshot(newPath);

        return newPath;
    }
}