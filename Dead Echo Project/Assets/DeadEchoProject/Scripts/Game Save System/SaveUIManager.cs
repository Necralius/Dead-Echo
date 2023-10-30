using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class SaveUIManager : MonoBehaviour
{
    [SerializeField] private Image               _lastScreenshot = null;
    [SerializeField] private TextMeshProUGUI     _saveName       = null;
    [SerializeField] private TextMeshProUGUI     _saveHour       = null;

    [SerializeField] private GameSaveData   _gameData = null;

    [SerializeField] private bool isLoadable = false;
    [SerializeField] private int loadIndex = -1;

    MenuSystem _menuSystem = null;

    private void ButtonAction()
    {
        if (loadIndex <= -1) return;

        _menuSystem.LoadGameSave(loadIndex);
    }

    private void Start()
    {
        _menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
        SetUpGameSave(_gameData);
        GetComponent<Button>().onClick.AddListener(delegate { ButtonAction(); });
    }
    public void OnGameDelete()
    {
        _saveName.text          = "Empty Save";
        _lastScreenshot.sprite  = GameStateManager.Instance.noSaveImage;

        _saveName.gameObject.SetActive(true);
        _saveHour.gameObject.SetActive(false);

        loadIndex   = -1;
        isLoadable  = false;
    }

    public void SetUpGameSave(GameSaveData gameData)
    {
        if (GameStateManager.Instance == null)  return;

        if (gameData == null || gameData.saveName.Equals(string.Empty))
        {
            _saveName.text          = "Empty Save";
            _lastScreenshot.sprite  = GameStateManager.Instance.noSaveImage;

            _saveName.gameObject.SetActive(true);
            _saveHour.gameObject.SetActive(false);

            loadIndex   = -1;
            isLoadable  = false;

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

        isLoadable = true;
        loadIndex = gameData.saveIndex;
    }
}