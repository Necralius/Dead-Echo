using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour
{

    //Volume Data
    [Header("Audio System")]
    [SerializeField] private SliderFloatField _generalVolume    = null;
    [SerializeField] private SliderFloatField _effectsVolume    = null;
    [SerializeField] private SliderFloatField _musicVolume      = null;
    [SerializeField] private SliderFloatField _zombiesVolume    = null;

    [Header("Graphics System")]
    [SerializeField] Toggle vSyncActive                         = null;
    [SerializeField] Toggle fullscreenActive                    = null;

    private List<Resolution> resolutions                            = new List<Resolution>();
    private int currentResolutionIndex                              = 0;
    private Resolution currentResolution;
    [SerializeField] private TMP_Dropdown resolutionDrp             = null;

    [SerializeField] private TMP_Dropdown   vSyncDrp                 = null;
    [SerializeField] private TMP_Dropdown   presetQualityDrp         = null;
    [SerializeField] private TMP_Dropdown   textureQualityDrp        = null;
    [SerializeField] private TMP_Dropdown   antialisingQualityDrp    = null;
    [SerializeField] private TMP_Dropdown   anisotropicQualityDrp    = null;
    [SerializeField] private TMP_Dropdown   shadowQualityDrp         = null;
    [SerializeField] private TMP_Dropdown   shadowResolutionDrp      = null;

    [Header("Gameplay Settings")]
    [SerializeField] private Toggle         tgl_InvertedX            = null;
    [SerializeField] private Toggle         tgl_InvertedY            = null;

    [SerializeField] private TMP_Dropdown   aimTypeDrp               = null;
    [SerializeField] private TMP_Dropdown   crouchTypeDrp            = null;

    [SerializeField] private SliderFloatField sensitivityX          = null;
    [SerializeField] private SliderFloatField sensitivityY          = null;

    GameStateManager _gameStateManager;

    [SerializeField] private LoadScreen loadingScreen;
    [SerializeField] private float loadingTime = 0f;

    private void Start()
    {
        _gameStateManager   = GameStateManager.Instance;
        resolutions         = Screen.resolutions.ToList();

        LoadSettings();
    }

    #region - Volume System -
    public void LoadVolumeSettings()
    {
        if (_gameStateManager.currentApplicationData == null) 
            return;

        _generalVolume.OverrideValue(_gameStateManager.currentApplicationData._generalVolume);
        _effectsVolume.OverrideValue(_gameStateManager.currentApplicationData._effectsVolume);
        _musicVolume.OverrideValue(_gameStateManager.currentApplicationData._musicVolume);
        _zombiesVolume.OverrideValue(_gameStateManager.currentApplicationData._zombiesVolume);

    }
    public void SetAndSaveVolumeSettings()
    {
        //TODO -> Set Volume settings

        if (_gameStateManager.currentApplicationData == null) 
            return;

        _gameStateManager.currentApplicationData._generalVolume    = _generalVolume.GetValue();
        _gameStateManager.currentApplicationData._effectsVolume    = _effectsVolume.GetValue();
        _gameStateManager.currentApplicationData._musicVolume      = _musicVolume.GetValue();
        _gameStateManager.currentApplicationData._zombiesVolume    = _zombiesVolume.GetValue();

        _gameStateManager.SaveApplicationData();
    }
    public void ResetVolumeSettings()
    {
        _gameStateManager.currentApplicationData.ResetVolumeSettings();
        _gameStateManager.SaveApplicationData();
        LoadVolumeSettings();
    }
    #endregion

    #region - Graphics Settings -

    #region - Resolution Settings -
    private void UpdateResolutionDrpd()
    {
        resolutionDrp.ClearOptions();

        List<string> options = new();

        for (int i = 0; i < resolutions.Count; i++)
        {
            string newOption = resolutions[i].width + "x" + resolutions[i].height;
          
            options.Add(newOption);
        }

        resolutionDrp.AddOptions(options);
        resolutionDrp.value = currentResolutionIndex;
        resolutionDrp.RefreshShownValue();

        resolutionDrp.onValueChanged.AddListener(delegate { VerifyResolution(); });
    }
    private void VerifyResolution()
    {
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
                currentResolution = resolutions[i];
            }
        }
    }
    #endregion

    public void SetAndSaveGraphicsSettings()
    {
        #region - Set -
        Screen.SetResolution(currentResolution.width, currentResolution.height, fullscreenActive.isOn);
        QualitySettings.SetQualityLevel(presetQualityDrp.value);

        QualitySettings.shadows                 = (ShadowQuality)shadowQualityDrp.value;
        QualitySettings.shadowResolution        = (ShadowResolution)shadowResolutionDrp.value;
        QualitySettings.anisotropicFiltering    = (AnisotropicFiltering)anisotropicQualityDrp.value;
        QualitySettings.antiAliasing            = antialisingQualityDrp.value;

        QualitySettings.vSyncCount              = vSyncDrp.value;
        #endregion

        #region - Save -
        if (_gameStateManager.currentApplicationData == null) 
            return;

        _gameStateManager.currentApplicationData._currentResolution     = currentResolution;
        _gameStateManager.currentApplicationData._resolutionIndex       = currentResolutionIndex;
        _gameStateManager.currentApplicationData._isFullscreen          = fullscreenActive.isOn;

        _gameStateManager.currentApplicationData._vSyncActive           = vSyncActive.isOn;
        _gameStateManager.currentApplicationData._vSyncCount            = vSyncDrp.value;

        _gameStateManager.currentApplicationData.shadowQuality          = shadowQualityDrp.value;
        _gameStateManager.currentApplicationData.shadowResolution       = shadowResolutionDrp.value;

        _gameStateManager.currentApplicationData.antialiasing           = antialisingQualityDrp.value;
        _gameStateManager.currentApplicationData.anisotropicFiltering   = anisotropicQualityDrp.value;

        _gameStateManager.currentApplicationData.qualityLevelIndex      = presetQualityDrp.value;

        _gameStateManager.SaveApplicationData();
        LoadSettings();
        #endregion
    }
    public void ResetGraphicsSettings()
    {
        _gameStateManager.currentApplicationData.ResetGraphicsSettings();
        _gameStateManager.SaveApplicationData();

        UpdateGraphicsUI();
    }
    public void UpdateGraphicsUI()
    {
        UpdateResolutionDrpd();

        if (_gameStateManager.currentApplicationData == null)
            return;

        resolutionDrp.value = _gameStateManager.currentApplicationData._resolutionIndex;

        vSyncActive.isOn    = _gameStateManager.currentApplicationData._vSyncActive;
        vSyncDrp.value      = _gameStateManager.currentApplicationData._vSyncCount;

        presetQualityDrp.value      = _gameStateManager.currentApplicationData.qualityLevelIndex;

        shadowQualityDrp.value      = _gameStateManager.currentApplicationData.shadowQuality;
        shadowResolutionDrp.value   = _gameStateManager.currentApplicationData.shadowResolution;

        antialisingQualityDrp.value = _gameStateManager.currentApplicationData.antialiasing;
        anisotropicQualityDrp.value = _gameStateManager.currentApplicationData.anisotropicFiltering;      
    }
    #endregion

    #region - Load General Settings -

    // ----------------------------------------------------------------------
    // Name: LoadSettings
    // Desc: This method load all settings founded on the local serialized
    //       data archieve.
    // ----------------------------------------------------------------------
    public void LoadSettings()
    {
        if (_gameStateManager != null)
        {

            // Settings Load and Set
            Screen.SetResolution(_gameStateManager.currentApplicationData._currentResolution.width,
                _gameStateManager.currentApplicationData._currentResolution.height, _gameStateManager.currentApplicationData._isFullscreen);

            QualitySettings.SetQualityLevel(_gameStateManager.currentApplicationData.qualityLevelIndex);

            QualitySettings.shadows                 = (ShadowQuality)_gameStateManager.currentApplicationData.shadowQuality;
            QualitySettings.shadowResolution        = (ShadowResolution)_gameStateManager.currentApplicationData.shadowResolution;
            QualitySettings.anisotropicFiltering    = (AnisotropicFiltering)_gameStateManager.currentApplicationData.anisotropicFiltering;
            QualitySettings.antiAliasing            = _gameStateManager.currentApplicationData.antialiasing;

            QualitySettings.vSyncCount              = _gameStateManager.currentApplicationData._vSyncCount;
        }
    }
    #endregion

    #region - Gameplay Settings -
    // -----------------------------------------------------------------------
    // Name: SetAndSaveGameplaySettings
    // Desc: This method set the current gameplay settings and save it in an
    //       serialized file.
    // ----------------------------------------------------------------------
    public void SetAndSaveGameplaySettings()
    {
        if (_gameStateManager == null) return;

        _gameStateManager.currentApplicationData.invertX        = tgl_InvertedX.isOn;
        _gameStateManager.currentApplicationData.invertY        = tgl_InvertedY.isOn;

        _gameStateManager.currentApplicationData.xSensitivity   = sensitivityX.GetValue();
        _gameStateManager.currentApplicationData.ySensitivity   = sensitivityY.GetValue();

        _gameStateManager.currentApplicationData.aimType        = aimTypeDrp.value;
        _gameStateManager.currentApplicationData.crouchType     = crouchTypeDrp.value;

        _gameStateManager.SaveApplicationData();
    }

    // ----------------------------------------------------------------------
    // Name: UpdateGameplaySettings
    // Desc: This method updates the gameplay settings tab UI, using the data
    //       saved on the serialized file.
    // ----------------------------------------------------------------------
    public void UpdateGameplaySettings()
    {
        tgl_InvertedX.isOn = _gameStateManager.currentApplicationData.invertX;
        tgl_InvertedY.isOn = _gameStateManager.currentApplicationData.invertY;

        sensitivityX.OverrideValue(_gameStateManager.currentApplicationData.xSensitivity);
        sensitivityY.OverrideValue(_gameStateManager.currentApplicationData.ySensitivity);

        aimTypeDrp.value        = _gameStateManager.currentApplicationData.aimType;
        crouchTypeDrp.value     = _gameStateManager.currentApplicationData.crouchType;
    }

    //
    //
    //
    //
    public void ResetGameplaySettings()
    {
        _gameStateManager.currentApplicationData.ResetGameplaySettings();
        _gameStateManager.SaveApplicationData();

        UpdateGameplaySettings();
    }
    #endregion

    #region - Game Data System -
    public void SaveGameData() => GameStateManager.Instance.SaveGameData();

    public void DeleteGameSave(int index)
    {
        SaveConteiner.Instance.LoadGameSaves();
        SaveConteiner.Instance.LoadNewGameSaves();
        GameStateManager.Instance.currentApplicationData.DeleteSaveByIndex(index);
    }

    public void LoadGameData(int saveIndex)
    {

    }
    #endregion

    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitToMenu()
    {
        StartSceneLoading("Scene_MainMenu");
    }
    public void StartSceneLoading(string sceneName)
    {
        loadingScreen.gameObject.SetActive(true);
        loadingTime = 0f;

        StartCoroutine(LoadScene(sceneName));
    }
    private IEnumerator LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        string activeScene = SceneManager.GetActiveScene().name;

        while (activeScene != sceneName)
        {
            loadingTime += Time.deltaTime;
            loadingScreen.UpdateState(loadingTime);
            activeScene = SceneManager.GetActiveScene().name;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        loadingScreen.gameObject.SetActive(false);

        yield return null;
    }
}