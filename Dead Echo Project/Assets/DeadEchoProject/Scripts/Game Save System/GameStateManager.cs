using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public Sprite noSaveImage;

    [SerializeField] private string permanentSaveName;
    [SerializeField] private string saveName;

    private FileDataHandler dynamicDataHandler;// -> This file handler, only is active for the game save data, he is very mutable.
    private FileDataHandler staticDataHandler; // -> This file handler, only is active for the application data file, this variable is imutable.

    private List<IDataPersistence> dataPersistenceObjects = new List<IDataPersistence>();

    private List<GameSaveData> gameDatas = new List<GameSaveData>();   

    public GameSaveData currentGameData;

    public ApplicationData currentApplicationData; // -> This file stores all the application data.

    MenuSystem menuSystem;

    #region - Built In Methods -
    private void Awake()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;

        DontDestroyOnLoad(gameObject);       
    }

    private void Start()
    {
        staticDataHandler = new FileDataHandler(Application.persistentDataPath, permanentSaveName);
        
        currentApplicationData = staticDataHandler.LoadApplicationData();

        if (currentApplicationData == null)
        {
            staticDataHandler.EncapsulateApplicationData(new ApplicationData());
            currentApplicationData = staticDataHandler.LoadApplicationData();
        }
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
            SaveGameData();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SaveGameData;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SaveGameData;
    }
    #endregion

    #region - Data Load -
    public void LoadGame(int saveIndex)
    {
        currentGameData = dynamicDataHandler.LoadGameState(currentApplicationData.GetSavePathByIndex(saveIndex).savePath);

        if (currentGameData == null)
        {
            Debug.Log("No data found.");
            return;
        }
        Debug.Log("Game Loaded");

        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
            dataPersistence.Load(currentGameData);
    }
    #endregion

    #region - Data Save -
    public void SaveGameData(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (currentGameData == null) 
            return;
        if (dataPersistenceObjects.Equals(null) || dataPersistenceObjects.Count == 0) 
            return;
        
        foreach(IDataPersistence dataPersistence in dataPersistenceObjects) 
            dataPersistence.Save(currentGameData);

        dynamicDataHandler.EncapsulateData(currentGameData);
    }
    public void SaveGameData()
    {
        if (currentGameData == null) 
            return;

        foreach(IDataPersistence dataPersistence in dataPersistenceObjects) dataPersistence.Save(currentGameData);

        dynamicDataHandler.EncapsulateData(currentGameData);
    }

    public void NewGameSave(int saveIndex)
    {
        dynamicDataHandler = new FileDataHandler(Application.persistentDataPath, saveName + saveIndex.ToString());
        currentGameData = new GameSaveData(saveName + saveIndex.ToString());

        currentGameData.saveDirectoryData = dynamicDataHandler.EncapsulateData(currentGameData);
        currentApplicationData.StartNewSave(saveIndex, currentGameData.saveDirectoryData);

        staticDataHandler.EncapsulateApplicationData(currentApplicationData);

        menuSystem.StartSceneLoading("Scene_Level1");
    }

    public List<GameSaveData> GetAllGameSaves()
    {
        List<GameSaveData>  datas = new List<GameSaveData>();

        List<string>        paths = new List<string>();

        if (currentApplicationData == null)         return null;
        if (!currentApplicationData.ExistSaves())    return null;

        for (int i = 0; i <= 3; i++)
        {
            string path = currentApplicationData.GetSavePathByIndex(i).savePath;

            if (path == null || path.Equals(string.Empty)) continue;
            else paths.Add(path);
        }

        if (paths.Count > 0)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (dynamicDataHandler == null) dynamicDataHandler = new FileDataHandler();
                GameSaveData save = dynamicDataHandler.LoadGameState(paths[i]);

                if (save != null) datas.Add(save);
            }
        }
        return datas;
    }

    public void SaveApplicationData()
    {
        currentApplicationData.UpdateSave();
        staticDataHandler.EncapsulateApplicationData(currentApplicationData);
    }
    #endregion

    public void RegisterDataHandler(IDataPersistence dataPersistence) => dataPersistenceObjects.Add(dataPersistence);

}