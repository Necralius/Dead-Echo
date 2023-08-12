using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.GunData;

public class UI_Manager : MonoBehaviour
{
    #region - Singleton Pattern -
    public static UI_Manager Instance;
    private void Awake() => Instance = this;
    #endregion

    [Header("Gun Mode UI")]
    [SerializeField] private List<GunModeUI> modes;

    [SerializeField] private float activeAlpha = 1f;
    [SerializeField] private float deactiveAlpha = 0.4f;

    [Header("Player State and Life")]
    [SerializeField] private GameObject playerSprite;
    [SerializeField] private Sprite crouchSprite;
    [SerializeField] private Sprite standUpSprite;
    [SerializeField] private TextMeshProUGUI lifeText;
    private Slider lifeSlider => playerSprite.GetComponent<Slider>();

    public void UpdatePlayerState(FPS_Controller controller)
    {
        playerSprite.GetComponent<Slider>().image.sprite = controller._isCrouching ? crouchSprite : standUpSprite;
        lifeSlider.value = controller.HealthValue;
        lifeText.text = controller.HealthValue.ToString() + "%";
    }

    public void UpdateMode(GunMode gunMode, List<GunMode> allModes)
    {
        for (int i = 0; i < modes.Count; i++)
        {

            if (allModes.Contains(modes[i].mode)) modes[i].obj.SetActive(true);
            else modes[i].obj.SetActive(false);

            if (modes[i].mode == gunMode) modes[i].obj.GetComponent<CanvasGroup>().alpha = activeAlpha;
            else modes[i].obj.GetComponent<CanvasGroup>().alpha = deactiveAlpha;
        }
    }
    [Serializable]
    public struct GunModeUI
    {
        public GunMode mode;
        public GameObject obj;
    }
}