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

    public GameData data;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public void SaveGunData(GameData data)
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
    public GameData LoadGameState()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using(FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream)) dataToLoad = reader.ReadToEnd();
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        return loadedData;
    }
}
