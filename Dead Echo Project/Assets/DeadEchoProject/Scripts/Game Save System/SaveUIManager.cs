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

    private void Start()
    {
        SetUpGameSave(_gameData);
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

            return;
        }

        _saveName.text = _gameData.saveName;
        _saveHour.text = _gameData.saveHour;

        _gameData = gameData;
        Texture2D lastScreenshot = null; // gameData.GetSavedImage();

        if (lastScreenshot == null) return;
        else
        {
            Rect rect = new Rect(0, 0, lastScreenshot.width, lastScreenshot.height);

            Sprite lastScreenshotSprite = Sprite.Create(lastScreenshot, rect, new Vector2(0, 0), 1);
            _lastScreenshot.sprite = lastScreenshotSprite;
        }
    }
}