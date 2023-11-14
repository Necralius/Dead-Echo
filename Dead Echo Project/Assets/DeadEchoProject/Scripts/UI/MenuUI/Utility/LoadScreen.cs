using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{
    #region - Singleton Pattern -
    public static LoadScreen Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;      
    }
    #endregion

    [SerializeField] private Slider loadingSlider;
    [SerializeField] private float loadingProgress;
    [SerializeField] private GameObject loadingScreen;

    public void UpdateState(float loadingProgress)
    {
        float progress = Mathf.Clamp01(loadingProgress / 0.9f);
        loadingSlider.value = (progress * 100);
    }

    public void LoadScene(string sceneName, int saveIndexer)
    {
        StartCoroutine(LoadSceneProcess(sceneName, saveIndexer));
    }

    private IEnumerator LoadSceneProcess(string sceneName, int saveIndexer)
    {
        loadingScreen.gameObject.SetActive(true);

        AsyncOperation opr = SceneManager.LoadSceneAsync(sceneName);

        while (!opr.isDone)
        {
            loadingProgress = opr.progress;
            UpdateState(loadingProgress);
            yield return null;
        }

        loadingScreen.gameObject.SetActive(false);

        if (saveIndexer != -1) 
            GameStateManager.Instance.LoadGame(saveIndexer);
    }
}