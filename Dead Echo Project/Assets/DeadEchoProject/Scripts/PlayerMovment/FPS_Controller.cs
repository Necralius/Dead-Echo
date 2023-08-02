using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FPS_Controller : MonoBehaviour
{
    #region - Dependencies -
    private CharacterController controller  => GetComponent<CharacterController>();
    private Animator bodyAnimator           => GetComponent<Animator>();
    private Camera bodyCamera               => GetComponentInChildren<Camera>();
    private InputManager inputManager       => InputManager.Instance != null ? InputManager.Instance : null;
    #endregion


    #region - Private Data -
    //Look Data
    [Header("Player Data Settings")]
    [SerializeField, Range(1, 10)] private float _walkSpeed;
    [SerializeField, Range(1, 30)] private float _sprintSpeed;
    [SerializeField, Range(1, 50)] private float _jumpForce;
    [SerializeField, Range(1, 100)] private float playerGravity     = 30f;

    [SerializeField, Range(1, 10)]  private float xLookSensitivity  = 2f;
    [SerializeField, Range(1, 10)]  private float yLookSensitivity  = 2f;
    [SerializeField, Range(1, 100)] private float upperLookLimit    = 80f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit    = 80f;
    #endregion

    [Header("Player State")]
    [SerializeField] private bool _canMove;
    [SerializeField] private bool _isWalking;
    [SerializeField] private bool _isSprinting;
    [SerializeField] private bool _isMoving;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private bool _inAir;


    private float rotationX         = 0;
    private Vector2 moveInput       = Vector2.zero;
    private Vector3 moveDirection   = Vector3.zero;


    // ---------------------------- Methods ----------------------------

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_canMove && inputManager != null)
        {
            LookHandler();
            MoveHandler();
            UpdateCalls();
        }
    }
    private void UpdateCalls()
    {
        _isMoving = controller.velocity != Vector3.zero;
        _isSprinting = inputManager.sprinting;
        _isWalking = _isMoving;
        _isGrounded = controller.isGrounded;
        _inAir = !_isGrounded;
    }
    //
    // Name : LookHandler
    // Desc : This method handles all the player camera and body look system.
    //
    private void LookHandler()
    {
        //First the method get the Y vector look input and multiplies it for the Y vector sensitivity,
        //later this value is clamped under an certain limit represented as two variables (UpperLookLimit and LowerLookLimit).

        rotationX -= inputManager.Look.y * yLookSensitivity / 100;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        bodyCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        //Also, the method handles the X rotation, appling it only in the body.
        transform.rotation *= Quaternion.Euler(0,inputManager.Look.x * xLookSensitivity / 100, 0);
    }

    // ----------------------------------------------------------------------
    // Name : MoveHandler
    // Desc : This method handles the player character movment and gravity.
    // ----------------------------------------------------------------------
    private void MoveHandler()
    {
        float speedValue = _isSprinting ? _sprintSpeed : _walkSpeed;
        moveInput = new Vector2(inputManager.Move.y * speedValue, inputManager.Move.x * speedValue);

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * moveInput.x) + (transform.TransformDirection(Vector3.right) * moveInput.y);
        moveDirection.y = moveDirectionY;

        if (!controller.isGrounded) moveDirection.y -= playerGravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}