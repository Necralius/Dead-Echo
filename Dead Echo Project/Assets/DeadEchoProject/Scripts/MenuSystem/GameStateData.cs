using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class GameStateData
{
        
}
public class MenuStateData //TODO -> Menu Complete data holder
{
    public SettingsStateData SettingsState { get; set; }
    public HDRenderPipelineAsset renderingPipelineAsset;



    public MenuStateData()
    {
        SettingsState = new SettingsStateData();
    }

    public void SaveState()
    {
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.antiAliasing = 3;

    }
    public void LoadData()
    {

    }
}
[Serializable]
public class SettingsStateData
{
    public Resolution       _currentResolution;
    public int              _vsyncType              = 0;
    public int              _fpsLimit               = 0;

    public int _textureResolution   = 1;
    public int _shadowResolution    = 1;
    public int _antialisingQuality  = 1;

    [Header("Water Quality")]
    public int      _waterSimulationQuality   = 3;
    public int      _waterDeformationQuality  = 3;
    public int      _waterFoamQuality         = 3;
    public bool     _waterExclusion           = false;

    [Header("Dynamic Resolution")]
    public int      _upscaleFilter      = 4;
    public bool     _forcePercentage    = false;
    public int      _resolutionScale    = 100;

}