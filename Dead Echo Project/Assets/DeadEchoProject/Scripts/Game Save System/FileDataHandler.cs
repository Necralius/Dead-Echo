using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public GameSaveData data;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public void EncapsulateData(GameSaveData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            using(FileStream stream = new FileStream(fullPath, FileMode.Create)) 
            {
                using (StreamWriter writer = new StreamWriter(stream)) writer.Write(dataToStore);
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    public GameSaveData LoadGameState()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
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
                Debug.LogError(e.ToString());
            }
        }

        return loadedData;
    }

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
}
