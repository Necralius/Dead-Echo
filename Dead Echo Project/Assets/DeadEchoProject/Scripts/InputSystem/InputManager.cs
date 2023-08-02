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

    public bool sprinting;

    //Private Data
    private InputActionMap currentMap;
    private InputAction moveAction;
    private InputAction lookAction;

    private InputAction sprintAction;
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

        currentMap = playerInput.currentActionMap;
        moveAction = currentMap.FindAction("Move");
        lookAction = currentMap.FindAction("Look");
        sprintAction = currentMap.FindAction("SprintAction");

        moveAction.performed += onMove;
        lookAction.performed += onLook;
        sprintAction.performed += onSprint;

        moveAction.canceled += onMove;
        lookAction.canceled += onLook;
        sprintAction.canceled += onSprint;
    }
    #endregion

    #region - Input Gethering -
    private void onMove(InputAction.CallbackContext context) => Move = context.ReadValue<Vector2>();
    private void onLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
    private void onSprint(InputAction.CallbackContext context) => sprinting = context.ReadValueAsButton();
    #endregion
}