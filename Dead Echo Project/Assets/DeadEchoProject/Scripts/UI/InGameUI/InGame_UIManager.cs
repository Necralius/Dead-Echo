using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NekraByte.FPS_Utility.Core.Enumerators;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class InGame_UIManager : MonoBehaviour
{
    #region - Singleton Pattern -
    public static InGame_UIManager Instance;
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

    [SerializeField] private List<CharacterState> _stanceStates;

    // ----------------------------------------------------------------------
    // Name: UpdatePlayerState
    // Desc: This method updates completly the character UI.
    // ----------------------------------------------------------------------
    public void UpdatePlayerState(ControllerManager controller, CharacterManager manager)
    {
        UpdatePlayerState(controller);
        lifeText.text           = lifeSlider.value.ToString("F0") + "%";
        lifeSlider.value        = manager.CurrentHealth;
        lifeSlider.maxValue     = manager._maxHealth;
    }

    // ----------------------------------------------------------------------
    // Name: UpdatePlayerState (Overcharge)
    // Desc: This method updates the player UI stance state, changing the character
    //       state sprite to the correct state.
    // ----------------------------------------------------------------------
    public void UpdatePlayerState(ControllerManager controller)
    {
        CharacterState state;
        switch(controller._currentState)
        {
            case MovementState.Idle:        state = _stanceStates.Find(e => e.type == StateType.Stand);     break;
            case MovementState.Walking:     state = _stanceStates.Find(e => e.type == StateType.Stand);     break;
            case MovementState.Sprinting:   state = _stanceStates.Find(e => e.type == StateType.Stand);     break;
            case MovementState.Sliding:     state = _stanceStates.Find(e => e.type == StateType.Crouch);    break;
            case MovementState.Crouching:   state = _stanceStates.Find(e => e.type == StateType.Crouch);    break;
            case MovementState.Air:         state = _stanceStates.Find(e => e.type == StateType.Jumping);   break;
            default: state = _stanceStates[0]; break;
        }

        Sprite correctStateSprt = state.stateSprite;
        playerSprite.GetComponent<Slider>().image.sprite = correctStateSprt;
    }

    // ----------------------------------------------------------------------
    // Name: UpdateMode
    // Desc: This method updates the gun mode
    // ----------------------------------------------------------------------
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