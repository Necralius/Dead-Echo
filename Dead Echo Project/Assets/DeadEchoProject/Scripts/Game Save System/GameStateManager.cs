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

    [SerializeField] private string fileName;

    private GameData gameData;
    private FileDataHandler dataHandler;

    [SerializeField] private List<IDataPersistence> dataPersistenceObjects;

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);

        //dataPersistenceObjects = FindAllDataPersistencObjects();
    }

    private void Awake()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] public Image loadImageFill         = null;
    [SerializeField] public GameObject loadingScreen    = null;

    public void LoadScene(int sceneID)
    {
        StartCoroutine(LoadSceneAsync(sceneID));
    }
    IEnumerator LoadSceneAsync(int sceneID)
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            if (loadImageFill != null) loadImageFill.fillAmount = progressValue;

            yield return new WaitForSeconds(0.001f);
        }
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.LoadGameState();

        if (this.gameData == null)
        {
            Debug.Log("No data found.");
            return;
        }

        foreach (IDataPersistence dataPersistence in dataPersistenceObjects) dataPersistence.Load(gameData);
    }
    private List<IDataPersistence> FindAllDataPersistencObjects()
    {
        IEnumerable<IDataPersistence> dataPersistences = (IEnumerable<IDataPersistence>)Resources.FindObjectsOfTypeAll(typeof(IDataPersistence));
        return dataPersistences.ToList();
    }
}