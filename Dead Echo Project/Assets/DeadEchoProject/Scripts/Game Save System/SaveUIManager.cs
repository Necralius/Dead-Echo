using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class SaveUIManager : MonoBehaviour
{
    [SerializeField] private Image               _lastScreenshot = null;
    [SerializeField] private TextMeshProUGUI     _saveName       = null;
    [SerializeField] private TextMeshProUGUI     _saveHour       = null;

    [SerializeField] private int    _saveIndex      = 0;
    [SerializeField] private bool   _isloadAction   = false;

    [SerializeField] private GameSaveData   _gameData = null;

    private UnityEvent _loadSave = new UnityEvent();

    private Button _onClickBtn;
    MenuSystem _menuSystem;

    private void Start()
    {
        _loadSave = new UnityEvent();
        _onClickBtn = GetComponent<Button>();
        _menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();

        SetUpGameSave(_gameData);

        _loadSave.AddListener(delegate { PosTransitionActions(); });
        _onClickBtn.onClick.AddListener(delegate { ButtonAction(); });
    }
    private void ButtonAction()
    {
        if (_gameData == null) return;

        FadeSystemManager.Instance.CallFadeAction(_loadSave);

    }
    private void PosTransitionActions()
    {
        if (_isloadAction)
        {
            if (!_menuSystem)
            {
                Debug.LogWarning("Menu System not founded!");
                return;
            }
            _menuSystem.LoadGameSave(_saveIndex);
        }
        else GameStateManager.Instance.NewGameSave(_saveIndex);
    }

    public void OnGameDelete()
    {
        _saveName.text          = "Empty Save";
        _lastScreenshot.sprite  = GameStateManager.Instance.noSaveImage;

        _saveName.gameObject.SetActive(true);
        _saveHour.gameObject.SetActive(false);
        _menuSystem.DeleteGameSave(_saveIndex);
    }

    public void SetUpGameSave(GameSaveData gameData)
    {
        if (gameData == null || gameData.saveName.Equals(string.Empty))
        {
            _saveName.text          = "Empty Save";
            _lastScreenshot.sprite  = GameStateManager.Instance?.noSaveImage;

            _saveName.gameObject.SetActive(true);
            _saveHour.gameObject.SetActive(false);

            return;
        }

        _gameData = gameData;

        _saveName.text = _gameData.saveName;
        _saveHour.text = $"Game Saved at: {_gameData.saveTime.Hour}:{_gameData.saveTime.Minute} - {_gameData.saveTime.Day}/{_gameData.saveTime.Month}/{_gameData.saveTime.Year}";

        Texture2D lastScreenshot = gameData.GetImage();

        if (lastScreenshot == null) return;
        else
        {
            Rect rect = new Rect(0, 0, lastScreenshot.width, lastScreenshot.height);

            Sprite lastScreenshotSprite = Sprite.Create(lastScreenshot, rect, new Vector2(0, 0), 1);
            _lastScreenshot.sprite = lastScreenshotSprite;
        }
    }
}