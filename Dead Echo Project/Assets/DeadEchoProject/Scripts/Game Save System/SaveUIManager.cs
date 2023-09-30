using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class SaveUIManager : MonoBehaviour
{
    private Image               _lastScreenshot = null;
    private TextMeshProUGUI     _saveName       = null;
    private TextMeshProUGUI     _saveHour       = null;

    [SerializeField] private GameSaveData _gameData = null;

    private void Start()
    {
        SetUpGameSave(_gameData);
    }

    public void SetUpGameSave(GameSaveData gameData)
    {
        if (gameData == null)
        {
            _saveName.text = "Empty Save";
            _saveHour.gameObject.SetActive(false);

            return;
        }

        _gameData = gameData;

        if (_gameData.lastScreenshot != null)
        {
            Rect rect = new Rect(0,0, _gameData.lastScreenshot.width, _gameData.lastScreenshot.height);

            Sprite lastScreenshot = Sprite.Create(_gameData.lastScreenshot, rect, new Vector2(0, 0), 1);
            _lastScreenshot.sprite = lastScreenshot;
        }

        _saveName.text = _gameData.saveName;
        _saveHour.text = _gameData.saveHour;

    }
}