using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

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
}