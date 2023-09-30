using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [SerializeField] private string permanentSaveName;

    private FileDataHandler dynamicDataHandler;

    private List<IDataPersistence> dataPersistenceObjects = new List<IDataPersistence>();

    private List<GameSaveData> gameDatas = new List<GameSaveData>();

    private List<string> gameSavesPath = new List<string>();

    public GameSaveData currentGameData;

    public ApplicationData currentApplicationData;
    private FileDataHandler staticDataHandler;

    public void RegisterDataHandler(IDataPersistence dataPersistence) => dataPersistenceObjects.Add(dataPersistence);

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
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
            SaveGameData();
    }

    public void LoadGame()
    {
        currentGameData = dynamicDataHandler.LoadGameState();

        if (currentGameData == null)
        {
            Debug.Log("No data found.");
            return;
        }

        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
            dataPersistence.Load(currentGameData);
    }

    public void SaveGameData()
    {
        if (currentGameData == null) 
            return;

        foreach(IDataPersistence dataPersistence in dataPersistenceObjects) dataPersistence.Save(currentGameData);

        dynamicDataHandler.EncapsulateData(currentGameData);
    }

    public void SaveApplicationData()
    {
        currentApplicationData.UpdateSave();
        staticDataHandler.EncapsulateApplicationData(currentApplicationData);
    }
}