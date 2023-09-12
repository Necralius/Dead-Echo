using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// --------------------------------------------------------------------------
// Name : InputManager
// Desc : This class handle all the game input.
// --------------------------------------------------------------------------
public class InputManager : MonoBehaviour
{
    #region - Singleton Pattern -
    public static InputManager Instance;
    #endregion

    #region - Dependencies -
    private PlayerInput playerInput => GetComponent<PlayerInput>();
    #endregion

    #region - Input Class Data -
    //Public Data
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public bool sprint;
    public bool reload;
    public bool shooting;
    public bool aiming;
    public bool jumping;
    public bool crouching;
    public bool changingGunMode;
    public bool isHoldingRock;
    public bool flashlightActive;

    //Private Data
    public InputActionMap currentMap;

    //Movment Actions
    private InputAction moveAction;
    private InputAction lookAction;
    public InputAction jumpAction;
    public InputAction crouchAction;
    public InputAction sprintAction;

    //Gun Behavior Actions
    public InputAction reloadAction;
    public InputAction shootAction;
    public InputAction aimAction;
    public InputAction gunModeAction;
    public InputAction throwRockAction;

    public InputAction flashLightAction;

    public InputAction primaryGun;
    public InputAction secondaryGun;
    #endregion

    // ---------------------------- Methods ----------------------------

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name : Awake
    // Desc : This method its called in the very first application frame,
    //        also this method get all the map actions and translate them
    //        in to literal game inputs.
    // ----------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;

        currentMap      = playerInput.currentActionMap;

        //Movment Actions
        moveAction          = currentMap.FindAction("Move");
        lookAction          = currentMap.FindAction("Look");
        sprintAction        = currentMap.FindAction("SprintAction");
        jumpAction          = currentMap.FindAction("JumpAction");
        crouchAction        = currentMap.FindAction("CrouchAction");
        throwRockAction     = currentMap.FindAction("ThrowRockAction");

        //Gun Behavior Actions
        reloadAction        = currentMap.FindAction("ReloadAction");
        shootAction         = currentMap.FindAction("ShootAction");
        aimAction           = currentMap.FindAction("AimAction");
        gunModeAction       = currentMap.FindAction("ChangeGunMode");

        flashLightAction    = currentMap.FindAction("FlashLightAction");

        primaryGun          = currentMap.FindAction("PrimaryGun");
        secondaryGun        = currentMap.FindAction("SecondaryGun"); 

        moveAction.performed        += onMove;
        lookAction.performed        += onLook;
        sprintAction.performed      += onSprint;
        reloadAction.performed      += onReload;
        shootAction.performed       += onShoot;
        aimAction.performed         += onAim;
        jumpAction.performed        += onJump;
        crouchAction.performed      += onCrouch;
        gunModeAction.performed     += onModeChanged;
        throwRockAction.performed   += onThrowRocked;
        flashLightAction.performed  += onFlashlight;

        moveAction.canceled         += onMove;
        lookAction.canceled         += onLook;
        sprintAction.canceled       += onSprint;
        reloadAction.canceled       += onReload;
        shootAction.canceled        += onShoot;
        aimAction.canceled          += onAim;
        jumpAction.canceled         += onJump;
        crouchAction.canceled       += onCrouch;
        gunModeAction.canceled      += onModeChanged;
        throwRockAction.canceled    += onThrowRocked;
        flashLightAction.canceled   += onFlashlight;
    }
    #endregion

    #region - Input Gethering -
    private void onMove(InputAction.CallbackContext context)        => Move             = context.ReadValue<Vector2>();
    private void onLook(InputAction.CallbackContext context)        => Look             = context.ReadValue<Vector2>();
    private void onSprint(InputAction.CallbackContext context)      => sprint           = context.ReadValueAsButton();
    private void onReload(InputAction.CallbackContext context)      => reload           = context.ReadValueAsButton();
    private void onShoot(InputAction.CallbackContext context)       => shooting         = context.ReadValueAsButton();
    private void onAim(InputAction.CallbackContext context)         => aiming           = context.ReadValueAsButton();
    private void onJump(InputAction.CallbackContext context)        => jumping          = context.ReadValueAsButton();
    private void onCrouch(InputAction.CallbackContext context)      => crouching        = context.ReadValueAsButton();
    private void onModeChanged(InputAction.CallbackContext context) => changingGunMode  = context.ReadValueAsButton();
    private void onThrowRocked(InputAction.CallbackContext context) => isHoldingRock    = context.ReadValueAsButton();
    private void onFlashlight(InputAction.CallbackContext context)  => flashlightActive = context.ReadValueAsButton();
    #endregion
}