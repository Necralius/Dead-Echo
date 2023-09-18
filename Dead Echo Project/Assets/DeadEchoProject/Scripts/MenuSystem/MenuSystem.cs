using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    
    public void StartGame()
    {
        //Start a new game from the starting point
    }
    public void LoadData() //TODO -> LoadData Type gonna be used as argument
    {
        //Load an Game data on the scene
    }

    public void ApplySettingsData() //TODO -> Apply the current settings
    {

    }

    public void ResetSettingsData() //TODO -> Reset the actual settings panel
    {

    }
    public void StartNewSettings() //TODO -> Apply an automatic reset to the new settings to prevent settings that are breaked.
    {

    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitToMenu()
    {

    }
}