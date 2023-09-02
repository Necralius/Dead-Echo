using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateData
{
        
}
public class MenuStateData //TODO -> Menu Complete data holder
{
    public SettingsStateData SettingsState { get; set; }


    public MenuStateData()
    {
        SettingsState = new SettingsStateData();
    }

    public void SaveState()
    {

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

}