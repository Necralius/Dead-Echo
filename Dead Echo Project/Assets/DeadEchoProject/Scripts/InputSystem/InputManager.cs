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

    //Private Data
    private InputActionMap currentMap;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction reloadAction;
    private InputAction shootAction;
    private InputAction aimAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
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
        moveAction      = currentMap.FindAction("Move");
        lookAction      = currentMap.FindAction("Look");
        sprintAction    = currentMap.FindAction("SprintAction");
        reloadAction    = currentMap.FindAction("ReloadAction");
        shootAction     = currentMap.FindAction("ShootAction");
        aimAction       = currentMap.FindAction("AimAction");
        jumpAction      = currentMap.FindAction("JumpAction");
        crouchAction    = currentMap.FindAction("CrouchAction");

        moveAction.performed    += onMove;
        lookAction.performed    += onLook;
        sprintAction.performed  += onSprint;
        reloadAction.performed  += onReload;
        shootAction.performed   += onShoot;
        aimAction.performed     += onAim;
        jumpAction.performed    += onJump;
        crouchAction.performed  += onCrouch; 

        moveAction.canceled     += onMove;
        lookAction.canceled     += onLook;
        sprintAction.canceled   += onSprint;
        reloadAction.canceled   += onReload;
        shootAction.canceled    += onShoot;
        aimAction.canceled      += onAim;
        jumpAction.canceled     += onJump;
        crouchAction.canceled   += onCrouch;
    }
    #endregion

    #region - Input Gethering -
    private void onMove(InputAction.CallbackContext context)    => Move         = context.ReadValue<Vector2>();
    private void onLook(InputAction.CallbackContext context)    => Look         = context.ReadValue<Vector2>();
    private void onSprint(InputAction.CallbackContext context)  => sprint       = context.ReadValueAsButton();
    private void onReload(InputAction.CallbackContext context)  => reload       = context.ReadValueAsButton();
    private void onShoot(InputAction.CallbackContext context)   => shooting     = context.ReadValueAsButton();
    private void onAim(InputAction.CallbackContext context)     => aiming       = context.ReadValueAsButton();
    private void onJump(InputAction.CallbackContext context)    => jumping      = context.ReadValueAsButton();
    private void onCrouch(InputAction.CallbackContext context)  => crouching    = context.ReadValueAsButton();
    #endregion
}