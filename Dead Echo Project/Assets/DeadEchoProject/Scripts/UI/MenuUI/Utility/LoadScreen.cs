using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{
    #region - Singleton Pattern -
    public static LoadScreen Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;
    }
    #endregion

    [SerializeField] private Slider loadingSlider;

    public void UpdateState(float loadingProgress)
    {
        loadingSlider.value = loadingProgress / 100;
    }
}