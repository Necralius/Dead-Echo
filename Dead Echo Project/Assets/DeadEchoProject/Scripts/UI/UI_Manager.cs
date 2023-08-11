using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Sprite crouch;
    [SerializeField] private Sprite standUp;
    private Slider lifeSlider => playerSprite.GetComponent<Slider>();

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