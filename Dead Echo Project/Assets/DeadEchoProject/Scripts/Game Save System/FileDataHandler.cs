using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class FileDataHandler
{
    private string dataDirPath      = "";
    private string dataFileName     = "";

    public GameSaveData data;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }
    public FileDataHandler()
    {
        dataDirPath     = "";
        dataFileName    = "";
    }

    public SaveDirectoryData EncapsulateData(GameSaveData data)
    {
        if (data == null) return null;
        SaveDirectoryData directoryData = new SaveDirectoryData();

        try
        {
            data.UpdateSave(DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string fullPath = string.Empty;
            fullPath = Path.Combine(dataDirPath, dataFileName);

            if (!Directory.Exists(fullPath)) 
                Directory.CreateDirectory(fullPath);
            directoryData.saveFolderPath = fullPath;

            //data.lastScreenshotPath = ScreenshotTaker.Instance.SaveScreenshot(fullPath, dataFileName);

            directoryData.screenshotPath = ScreenshotTaker.Instance.SaveScreenshot(fullPath, dataFileName);

            fullPath = Path.Combine(fullPath, dataFileName + ".NBSV");

            directoryData.savePath = fullPath;

            //data.savePath = fullPath;

            string dataToStore = JsonUtility.ToJson(data, true);

            using(FileStream stream = new FileStream(fullPath, FileMode.Create)) 
            {
                using (StreamWriter writer = new StreamWriter(stream)) writer.Write(dataToStore);
            }
            return directoryData;
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
            throw;
        }
    }

    public GameSaveData LoadGameState(string fullPath)
    {
        GameSaveData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using(FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream)) dataToLoad = reader.ReadToEnd();
                }

                loadedData = JsonUtility.FromJson<GameSaveData>(dataToLoad);
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }

        return loadedData;
    }

    #region - Application Data Handler -
    public ApplicationData LoadApplicationData()
    {

        string fullPath = Path.Combine(dataDirPath, dataFileName);
        ApplicationData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream)) dataToLoad = reader.ReadToEnd();
                }

                loadedData = JsonUtility.FromJson<ApplicationData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        return loadedData;
    }

    public void EncapsulateApplicationData(ApplicationData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream)) writer.Write(dataToStore);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    #endregion
}