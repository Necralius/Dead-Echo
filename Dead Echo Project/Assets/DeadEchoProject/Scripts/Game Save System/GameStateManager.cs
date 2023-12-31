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
    #region - Singleton Pattern -
    public static GameStateManager Instance;
    #endregion

    public Sprite noSaveImage;

    #region - Save Names -
    [SerializeField] private string permanentSaveName   = "ApplicationData";
    [SerializeField] private string saveName            = "GameSave";
    #endregion

    //Data Writers
    private FileDataHandler dynamicDataHandler;// -> This file handler, only is active for the game save data, he is very mutable.
    private FileDataHandler staticDataHandler; // -> This file handler, only is active for the application data file, this variable is imutable.

    //An list of all savable objects in the game scene.
    private List<IDataPersistence> dataPersistenceObjects = new List<IDataPersistence>(); 

    public GameSaveData currentGameData;

    public ApplicationData currentApplicationData; // -> This file stores all the application data.

    MenuSystem menuSystem;

    public int saveIndexer = 0;

    // ------------------------------------------ Methods ------------------------------------------ //

    #region - Built In Methods -
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        DontDestroyOnLoad(gameObject);       

        staticDataHandler = new FileDataHandler(Application.persistentDataPath, permanentSaveName);
        
        currentApplicationData = staticDataHandler.LoadApplicationData();

        if (currentApplicationData == null)
        {
            staticDataHandler.EncapsulateApplicationData(new ApplicationData());
            currentApplicationData = staticDataHandler.LoadApplicationData();
        }
    }

    private void Start()
    {
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
            SaveGameData();

        if (Input.GetKeyDown(KeyCode.V)) 
            LoadGame(saveIndexer);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += LoadSaveOnSceneLoad;
        SceneManager.sceneLoaded += SaveGameOnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LoadSaveOnSceneLoad;
        SceneManager.sceneLoaded -= SaveGameOnSceneLoad;
    }
    #endregion

    #region - Data Load -
    private void SaveGameOnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        SaveGameData();
    }
    private void LoadSaveOnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (saveIndexer <= -1) return;
        if (menuSystem == null) return;

        LoadGame(saveIndexer);
    }
    public void LoadGame(int saveIndex)
    {
        if (dynamicDataHandler == null) return;
        SaveDirectoryData fullPath;

        if (currentApplicationData.GetSavePathByIndex(saveIndex, out fullPath))

        saveIndexer = saveIndex;

        currentGameData = dynamicDataHandler.LoadGameState(fullPath.savePath);

        if (currentGameData == null)
        {
            Debug.Log("No data found.");
            return;
        }

        if (dataPersistenceObjects.Count <= 0 || dataPersistenceObjects == null) return;

        StartCoroutine(LoadWithTime(0.5f));
    }
    #endregion

    IEnumerator LoadWithTime(float time)
    {
        yield return new WaitForSeconds(time / 2);

        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
            dataPersistence.Load(currentGameData);

        yield return new WaitForSeconds(time / 2);
    }

    #region - Data Save -
    public void SaveGameData()
    {
        if (currentGameData == null) 
            return;

        if (dataPersistenceObjects.Equals(null) || dataPersistenceObjects.Count == 0)
        {
            //Debug.Log("Don't exist persistence objects!"); -> Debug Line
            return;
        }
            
        foreach (IDataPersistence dataPersistence in dataPersistenceObjects) 
            dataPersistence.Save(currentGameData);

        dynamicDataHandler.EncapsulateData(currentGameData);
    }

    public void NewGameSave(int saveIndex)
    {
        dynamicDataHandler = new FileDataHandler(Application.persistentDataPath, saveName + saveIndex.ToString());
        currentGameData = new GameSaveData(saveName + saveIndex.ToString());
        currentGameData.saveIndex = saveIndex;

        currentGameData.saveDirectoryData = dynamicDataHandler.EncapsulateData(currentGameData);
        currentApplicationData.StartNewSave(saveIndex, currentGameData.saveDirectoryData);

        saveIndexer = saveIndex;
        staticDataHandler.EncapsulateApplicationData(currentApplicationData);

        LoadScreen.Instance.LoadScene("Scene_Level1", saveIndex);
    }

    public List<GameSaveData> GetAllGameSaves()
    {
        List<GameSaveData>  datas = new List<GameSaveData>();

        List<string>        paths = new List<string>();

        if (currentApplicationData == null)             return null;
        if (!currentApplicationData.ExistSaves())       return null;

        for (int i = 0; i < 3; i++)
        {
            SaveDirectoryData path;
            if (currentApplicationData.GetSavePathByIndex(i, out path))
            {
                if (path.Equals(null) || 
                    path.Equals(string.Empty) || 
                    path.savePath == "" || 
                    path.savePath.Equals(string.Empty)) continue;
                else paths.Add(path.savePath);
            }
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
        else return null;
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