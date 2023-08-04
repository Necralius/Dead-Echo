using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FPS_Controller : MonoBehaviour
{
    #region - Singleton Pattern -
    public static FPS_Controller Instance;
    #endregion

    #region - Dependencies -
    private CharacterController controller  => GetComponent<CharacterController>();
    private Animator bodyAnimator           => GetComponent<Animator>();
    private Transform bodyCamera               => GetComponentInChildren<Camera>().transform.parent;
    private InputManager inputManager       => InputManager.Instance != null ? InputManager.Instance : null;
    #endregion

    #region - Player Data Settings -
    [Header("Player Data Settings")]
    [SerializeField, Range(1, 10)] private float _walkSpeed          = 4f;
    [SerializeField, Range(1, 30)] private float _sprintSpeed        = 8f;
    [SerializeField, Range(1, 30)] private float _crouchSpeed        = 2f;
    [SerializeField, Range(1, 50)] private float _jumpForce          = 4f;
    [SerializeField, Range(1, 100)] private float _playerGravity     = 30f;

    [SerializeField, Range(1, 10)]  private float _xLookSensitivity  = 2f;
    [SerializeField, Range(1, 10)]  private float _yLookSensitivity  = 2f;
    [SerializeField, Range(1, 100)] private float _upperLookLimit    = 80f;
    [SerializeField, Range(1, 100)] private float _lowerLookLimit    = 80f;

    [Header("Crouch Settings")]
    [SerializeField] private Vector3 _crouchingCenter   = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 _standingCenter    = new Vector3(0, 0, 0);
    [SerializeField, Range(0.1f, 2f)] private float _crouchHeight        = 0.5f;
    [SerializeField, Range(0.1f, 2f)] private float _standingHeight      = 2f;
    [SerializeField] private float _timeToCrouch        = 0.25f;
    [SerializeField] private LayerMask crouchUpLayer;
    #endregion

    #region - HeadBob System -
    [Header("HeadBob Settings")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;

    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;

    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;

    private float defaultYPost = 0;
    private float _timer;
    #endregion

    #region - Player State -
    [Header("Player State")]
    public bool _canMove        = true;
    public bool _isWalking      = false;
    public bool _isSprinting    = false;
    public bool _isCrouching    = false;
    public bool _canCrouch      = false;
    public bool _duringCrouch   = false;
    public bool _isMoving       = false;
    public bool _isGrounded     = true;
    public bool _inAir          = false;
    public bool _changingWeapon = false;
    #endregion

    #region - Private Data -
    private float _rotationX         = 0;
    private Vector2 _moveInput       = Vector2.zero;
    private Vector3 _moveDirection   = Vector3.zero;
    #endregion


    #region - Public Data -
    public bool isSprinting { get => _isSprinting; }
    #endregion

    // ---------------------------- Methods ----------------------------

    #region - BuiltIn Methods -
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        defaultYPost = bodyCamera.transform.localPosition.y;
    }
    private void Update()
    {
        if (_canMove && inputManager != null)
        {
            LookHandler();
            MoveHandler();
            JumpHandler();
            CrouchHandler();

            if (_isMoving && _isGrounded) HeadBobHandler();
            UpdateCalls();
        }
    }
    #endregion


    // ----------------------------------------------------------------------
    // Name : UpdateCalls
    // Desc : This method manage all data tha need to be updated
    //        every frame call.
    // ----------------------------------------------------------------------
    private void UpdateCalls()
    {
        _isMoving       = controller.velocity != Vector3.zero;
        _isSprinting    = inputManager.sprint;
        _isWalking      = _isMoving;
        _isGrounded     = controller.isGrounded;
        _inAir          = !_isGrounded;
    }

    #region - Look Handler -
    // ----------------------------------------------------------------------
    // Name : LookHandler
    // Desc : This method handles all the player camera and body look system.
    // ----------------------------------------------------------------------
    private void LookHandler()
    {
        //First the method get the Y vector look input and multiplies it for the Y vector sensitivity,
        //later this value is clamped under an certain limit represented as two variables (UpperLookLimit and LowerLookLimit).

        _rotationX -= inputManager.Look.y * _yLookSensitivity / 100;
        _rotationX = Mathf.Clamp(_rotationX, -_upperLookLimit, _lowerLookLimit);
        bodyCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        //Also, the method handles the X rotation, appling it only in the body.
        transform.rotation *= Quaternion.Euler(0,inputManager.Look.x * _xLookSensitivity / 100, 0);
    }
    #endregion

    #region - Movment Handler -
    // ----------------------------------------------------------------------
    // Name : MoveHandler
    // Desc : This method handles the player character movment and gravity.
    // ----------------------------------------------------------------------
    private void MoveHandler()
    {
        float speedValue = 0;
        if (_isCrouching) speedValue = _crouchSpeed;
        else speedValue = _isSprinting && !_isCrouching ? _sprintSpeed : _walkSpeed;

        _moveInput = new Vector2(inputManager.Move.y * speedValue, inputManager.Move.x * speedValue);

        float moveDirectionY = _moveDirection.y;
        _moveDirection = (transform.TransformDirection(Vector3.forward) * _moveInput.x) + (transform.TransformDirection(Vector3.right) * _moveInput.y);
        _moveDirection.y = moveDirectionY;

        if (!controller.isGrounded) _moveDirection.y -= _playerGravity * Time.deltaTime;
        controller.Move(_moveDirection * Time.deltaTime);
    }
    #endregion

    #region - Jump System -
    // ----------------------------------------------------------------------
    // Name : JumpHandler
    // Desc : This method handle the jump action on the player character.
    // ----------------------------------------------------------------------
    private void JumpHandler()
    {
        if (_isGrounded) if (inputManager.jumping) _moveDirection.y = _jumpForce;
    }
    #endregion

    #region - Crouch System -
    // ----------------------------------------------------------------------
    // Name : CrouchHandler
    // Desc : This method handle the crouch system.
    // ----------------------------------------------------------------------
    private void CrouchHandler()
    {
        if (_isGrounded && !_duringCrouch) if (inputManager.crouching) StartCoroutine(CrouchAction());
    }
    private IEnumerator CrouchAction()
    {
        if (_isCrouching && Physics.Raycast(bodyCamera.transform.position, Vector3.up, 1.5f, crouchUpLayer)) yield break;

        _duringCrouch = true;

        float timeElapsed = 0;
        float targetHeight = _isCrouching ? _standingHeight : _crouchHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = _isCrouching ? _standingCenter : _crouchingCenter;
        Vector3 currentCenter = controller.center;

        while(timeElapsed < _timeToCrouch)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / _timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / _timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        _isCrouching = !_isCrouching;

        _duringCrouch = false;
    }
    #endregion

    #region - HeadBob System -
    private void HeadBobHandler()
    {
        _timer += Time.deltaTime * (_isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
        bodyCamera.transform.localPosition = new Vector3(bodyCamera.transform.localPosition.x,
            defaultYPost + Mathf.Sin(_timer) * (_isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount)
            , bodyCamera.transform.localPosition.z);
    }
    #endregion
}