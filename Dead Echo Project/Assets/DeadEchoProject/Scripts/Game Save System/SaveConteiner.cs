using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;


public class SaveConteiner : MonoBehaviour
{
    #region - Singleton Pattern -
    public static SaveConteiner Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;
    }
    #endregion

    [Header("Saves Data")]
    public List<GameSaveData>   _saves          = new List<GameSaveData>();

    [Header("Saves UI Managers")]
    public List<SaveUIManager>  _newGameSaves   = new List<SaveUIManager>();
    public List<SaveUIManager>  _gameSaveLoads  = new List<SaveUIManager>();

    private void Start()
    {
        LoadNewGameSaves();
        LoadGameSaves();
    }
    private void LoadAllSaves()
    {
        //_saves = new List<GameSaveData>();
        if (GameStateManager.Instance) 
            _saves = GameStateManager.Instance.GetAllGameSaves();
    }

    public void LoadNewGameSaves()
    {
        LoadAllSaves();

        if (_saves == null || _saves.Count <= 0 ) return;

        for (int i = 0; i < _saves.Count; i++)
        {
            if (_newGameSaves[i] != null && _saves[i] != null) 
                _newGameSaves[i].SetUpGameSave(_saves[i]);
        }
    }

    public void LoadGameSaves()
    {
        LoadAllSaves();

        if (_saves == null || _saves.Count <= 0) return;

        for (int i = 0; i < _saves.Count; i++)
        {
            if (_gameSaveLoads[i] != null && _saves[i] != null)
                _gameSaveLoads[i].SetUpGameSave(_saves[i]);
        }
    }
}