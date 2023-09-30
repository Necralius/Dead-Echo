using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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

    private List<Resolution> resolutions    = new List<Resolution>();
    private int currentResolutionIndex      = 0;
    private Resolution currentResolution;
    [SerializeField] private TMP_Dropdown resolutionDrp            = null;

    [SerializeField] private TMP_Dropdown vSyncDrp                 = null;
    [SerializeField] private TMP_Dropdown presetQualityDrp         = null;
    [SerializeField] private TMP_Dropdown textureQualityDrp        = null;
    [SerializeField] private TMP_Dropdown antialisingQualityDrp    = null;
    [SerializeField] private TMP_Dropdown anisotropicQualityDrp    = null;
    [SerializeField] private TMP_Dropdown shadowQualityDrp         = null;
    [SerializeField] private TMP_Dropdown shadowResolutionDrp      = null;

    //private List<>


    GameStateManager _gameStateManager;

    private void Start()
    {
        _gameStateManager = GameStateManager.Instance;
        resolutions = Screen.resolutions.ToList();
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

    #region - Resolution Setting -
    public void UpdateResoluitonDrpd()
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

        QualitySettings.vSyncCount = vSyncDrp.value;
        #endregion

        #region - Save -
        if (_gameStateManager.currentApplicationData == null) 
            return;

        _gameStateManager.currentApplicationData._currentResolution = currentResolution;
        _gameStateManager.currentApplicationData._resolutionIndex   = currentResolutionIndex;
        _gameStateManager.currentApplicationData._isFullscreen      = fullscreenActive.isOn;

        _gameStateManager.currentApplicationData._vSyncActive       = vSyncActive.isOn;
        _gameStateManager.currentApplicationData._vSyncCount        = vSyncDrp.value;

        _gameStateManager.SaveApplicationData();
        #endregion
    }
    public void ResetGraphicsSettings()
    {
        _gameStateManager.currentApplicationData.ResetGraphicsSettings();
        _gameStateManager.SaveApplicationData();

        LoadGraphicsSettings();
    }
    public void LoadGraphicsSettings()
    {
        UpdateResoluitonDrpd();

        if (_gameStateManager.currentApplicationData == null)
            return;

        resolutionDrp.value = _gameStateManager.currentApplicationData._resolutionIndex;

        vSyncActive.isOn    = _gameStateManager.currentApplicationData._vSyncActive;
        vSyncDrp.value      = _gameStateManager.currentApplicationData._vSyncCount;
    }
    #endregion



    public void StartGame()
    {
        //Start a new game from the starting point
    }
    public void LoadData() //TODO -> LoadData Type gonna be used as argument
    {
        //Load an Game data on the scene
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitToMenu()
    {
        LoadScene(0);
    }
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}