using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class MenuSystem : MonoBehaviour
{
    //Volume Data
    [Header("Audio System")]
    [SerializeField] private List<SliderFloatField> _volumesSliders = new List<SliderFloatField>();
    [SerializeField] private Transform _volumeSlidersContent = null;
    [SerializeField] private GameObject SliderFloatFieldPrefab = null;

    #region - Graphics Data -
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
    #endregion

    #region - Gameplay Data -
    [Header("Gameplay Settings")]
    [SerializeField] private Toggle         tgl_InvertedX            = null;
    [SerializeField] private Toggle         tgl_InvertedY            = null;

    [SerializeField] private TMP_Dropdown   aimTypeDrp               = null;
    [SerializeField] private TMP_Dropdown   crouchTypeDrp            = null;

    [SerializeField] private SliderFloatField sensitivityX          = null;
    [SerializeField] private SliderFloatField sensitivityY          = null;
    #endregion

    GameStateManager _gameStateManager;

    private void Awake()
    {
        _gameStateManager = GameStateManager.Instance;
    }

    private void Start()
    {
        resolutions         = Screen.resolutions.ToList();

        currentResolution.width     = 1920;
        currentResolution.height    = 1080;
        Screen.SetResolution(currentResolution.width, currentResolution.height, fullscreenActive.isOn);

        LoadSettings();
    }

    #region - Volume System -
    // ----------------------------------------------------------------------
    // Name: LoadVolumeSettings (Method)
    // Desc: This method
    // ----------------------------------------------------------------------
    public void LoadVolumeSettings()
    {
        if (_gameStateManager == null || _gameStateManager.currentApplicationData == null) return;

        if (_gameStateManager.currentApplicationData._volumes.Count <= 0)
        {
            _gameStateManager.currentApplicationData._volumes.Clear();
            foreach (Transform child in _volumeSlidersContent) Destroy(child.gameObject);

            foreach (var track in AudioManager.Instance._tracks)
            {
                SliderFloatField obj = Instantiate(SliderFloatFieldPrefab, _volumeSlidersContent).GetComponentInChildren<SliderFloatField>();
                AudioTrackVolume trackVolumeInfo = new AudioTrackVolume(track.Key, DecibelsToVolume(AudioManager.Instance.GetTrackVolume(obj._fieldName)));

                obj.SetUp(trackVolumeInfo);
                _volumesSliders.Add(obj);

                _gameStateManager.currentApplicationData._volumes.Add(trackVolumeInfo);
                _gameStateManager.SaveApplicationData();
            }
        }
        else if (_volumesSliders.Count <= 0)
        {
            foreach (Transform child in _volumeSlidersContent) Destroy(child.gameObject);

            for (int i = 0; i < _gameStateManager.currentApplicationData._volumes.Count; i++)
            {
                SliderFloatField obj = Instantiate(SliderFloatFieldPrefab, _volumeSlidersContent).GetComponentInChildren<SliderFloatField>();
                obj.SetUp(_gameStateManager.currentApplicationData._volumes[i]);
                _volumesSliders.Add(obj);
            }
        }

        SetVolumeData();
    }

    // ----------------------------------------------------------------------
    // Name: SetVolumeData (Method)
    // Desc: This method sets the volume on the mixer, gettting the saved
    //       volume data from the current application data.
    // ----------------------------------------------------------------------
    public void SetVolumeData()
    {
        for (int i = 0; i < _volumesSliders.Count; i++) 
            AudioManager.Instance.SetTrackVolume(_volumesSliders[i]._fieldName, ConvertToDecibels(_volumesSliders[i].GetValue()));
    }

    public void LoadVolumeDataFromFile()
    {
        if (_gameStateManager == null || _gameStateManager.currentApplicationData == null) return;

        ApplicationData currentApplData = _gameStateManager.currentApplicationData;

        for (int i = 0; i < currentApplData._volumes.Count; i++) 
            AudioManager.Instance.SetTrackVolume(currentApplData._volumes[i].Name, ConvertToDecibels(currentApplData._volumes[i].Volume));
    }

    // ----------------------------------------------------------------------
    // Name: SaveVolume (Method)
    // Desc: this method saves the current volumes in the volumes sliders,
    //       encapsulating them in the the current application data.
    // ----------------------------------------------------------------------
    public void SaveVolume()
    {
        for (int i = 0; i < _volumesSliders.Count; i++) 
            _gameStateManager.currentApplicationData._volumes[i].Volume = _volumesSliders[i].GetValue();
       
        _gameStateManager.SaveApplicationData();

        SetVolumeData();
    }

    // ----------------------------------------------------------------------
    // Name: ConvertToDecibels (Method)
    // Desc: This method convert an range of 1.0f to 0.1f to decibels from an
    //       range that starts from -80 DB to +10 DB.
    // ----------------------------------------------------------------------
    public float ConvertToDecibels(float Volume)
    {
        if (Volume < 0 || Volume > 1) return 0;

        int decibels = Mathf.FloorToInt((Volume * 100) - 100);
        //Debug.Log($" Converting to decibels: Volume: {Volume}, Decibels: {decibels}"); -> Debug Line
        return decibels;
    }

    // ----------------------------------------------------------------------
    // Name: DecibelsToVolume (Method)
    // Desc: This method converts decibels to an range of 1.0 to 0.1f.
    // ----------------------------------------------------------------------
    public float DecibelsToVolume(float Decibels)
    {
        float volume = (Decibels + 100) / 100;
        //Debug.Log($" DTV -> Volume: {volume} , Decibels: {Decibels}"); -> Debug Line
        return volume;
    }

    // ----------------------------------------------------------------------
    // Name: ResetVolumeSettings (Methods)
    // Desc: This method resets the volume settings on the game application
    //       data, and on the UI.
    // ----------------------------------------------------------------------
    public void ResetVolumeSettings()
    {
        _volumesSliders.Clear();
        foreach (Transform child in _volumeSlidersContent) Destroy(child.gameObject);

        _gameStateManager.currentApplicationData.ResetVolumeSettings();

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

        QualitySettings.shadows                     = (ShadowQuality)shadowQualityDrp.value;
        QualitySettings.shadowResolution            = (ShadowResolution)shadowResolutionDrp.value;
        QualitySettings.anisotropicFiltering        = (AnisotropicFiltering)anisotropicQualityDrp.value;
        QualitySettings.antiAliasing                = antialisingQualityDrp.value;
        QualitySettings.globalTextureMipmapLimit    = textureQualityDrp.value;

        if (vSyncActive) QualitySettings.vSyncCount = vSyncDrp.value;
        else QualitySettings.vSyncCount = 0;
        #endregion

        #region - Save -
        if (_gameStateManager.currentApplicationData == null) 
            return;

        _gameStateManager.currentApplicationData.currentResolution      = currentResolution;
        _gameStateManager.currentApplicationData.resolutionIndex        = currentResolutionIndex;
        _gameStateManager.currentApplicationData.isFullscreen           = fullscreenActive.isOn;

        _gameStateManager.currentApplicationData.vSyncActive            = vSyncActive.isOn;
        _gameStateManager.currentApplicationData.vSyncCount             = vSyncDrp.value;

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

        resolutionDrp.value         = _gameStateManager.currentApplicationData.resolutionIndex;

        vSyncActive.isOn            = _gameStateManager.currentApplicationData.vSyncActive;
        vSyncDrp.value              = _gameStateManager.currentApplicationData.vSyncCount;

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
            Screen.SetResolution(_gameStateManager.currentApplicationData.currentResolution.width,
                _gameStateManager.currentApplicationData.currentResolution.height, _gameStateManager.currentApplicationData.isFullscreen);

            QualitySettings.SetQualityLevel(_gameStateManager.currentApplicationData.qualityLevelIndex);

            QualitySettings.shadows                 = (ShadowQuality)_gameStateManager.currentApplicationData.shadowQuality;
            QualitySettings.shadowResolution        = (ShadowResolution)_gameStateManager.currentApplicationData.shadowResolution;
            QualitySettings.anisotropicFiltering    = (AnisotropicFiltering)_gameStateManager.currentApplicationData.anisotropicFiltering;
            QualitySettings.antiAliasing            = _gameStateManager.currentApplicationData.antialiasing;

            QualitySettings.vSyncCount              = _gameStateManager.currentApplicationData.vSyncCount;

            SetVolumeData();
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

    // ----------------------------------------------------------------------
    // Name: ResetGameplaySettings (Method)
    // Desc: This method resets the gameplay settings on the application dada
    //       file.
    // ----------------------------------------------------------------------
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
        GameStateManager.Instance.currentApplicationData.DeleteSaveByIndex(index);
        GameStateManager.Instance.SaveApplicationData();

        SaveConteiner.Instance.LoadGameSaves();
        SaveConteiner.Instance.LoadNewGameSaves();
    }

    public void LoadGameData(int saveIndex)
    {

    }
    #endregion

    public void SetMusicEvent(string eventTag)
    {       
        AudioManager.Instance.CallMusicEvent(eventTag);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitToMenu()
    {
        LoadScreen.Instance.LoadScene("Scene_MainMenu", -1);
    }
    public void LoadGameSave(int saveIndex)
    {
        LoadScreen.Instance.LoadScene("Scene_Level1", saveIndex); //Load the scene
    }

    public void RespawnFromLastSave()
    {
        _gameStateManager.LoadGame(_gameStateManager.saveIndexer);
    }
}