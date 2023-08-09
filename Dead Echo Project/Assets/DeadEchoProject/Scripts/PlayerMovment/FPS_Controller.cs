using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static NekraByte.FPS_Utility;

public class FPS_Controller : MonoBehaviour
{
    #region - Singleton Pattern -
    public static FPS_Controller Instance;
    #endregion

    #region - Controller Dependencies -
    private CharacterController controller  => GetComponent<CharacterController>();
    private Transform bodyCamera            => GetComponentInChildren<Camera>().transform.parent;
    public GameObject cameraObject          => GetComponentInChildren<Camera>().gameObject;
    private InputManager inputManager       => InputManager.Instance != null ? InputManager.Instance : null;
    #endregion

    #region - Gun System -
    [Header("Gun System Dependencies")]
    public Transform aimHolder;
    public Animator armsAnimator;
    public GameObject recoilHolder;

    [Header("Weapon Sway")]
    public GameObject weaponSwayObject;
    public GameObject idleSwayObject;

    [Header("Gun System")]
    public GunBase equippedGun;
    public List<GunBase> gunsInHand;
    #endregion

    #region - UI Elements -
    [Header("UI Elements")]
    public TextMeshProUGUI txt_Ammo;
    public TextMeshProUGUI txt_gunState;
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
    [SerializeField, Range(0, 100)] private float walkBobSpeed = 14f;
    [SerializeField, Range(0, 100)] private float walkBobAmount = 0.05f;

    [SerializeField, Range(0, 100)] private float sprintBobSpeed = 18f;
    [SerializeField, Range(0, 100)] private float sprintBobAmount = 0.11f;

    [SerializeField, Range(0, 100)] private float crouchBobSpeed = 8f;
    [SerializeField, Range(0, 100)] private float crouchBobAmount = 0.025f;

    private float defaultYPost = 0;
    private float _timer;
    #endregion

    #region - Player State -
    [Header("Player State")]
    public bool _canMove            = true;
    public bool _isWalking          = false;
    public bool _isSprinting        = false;
    public bool _isCrouching        = false;
    public bool _canCrouch          = false;
    public bool _duringCrouch       = false;
    public bool _isMoving           = false;
    public bool _isGrounded         = true;
    public bool _inAir              = false;
    public bool _changingWeapon     = false;
    private bool _walkingBackwards  = false;
    #endregion

    #region - Private Data -
    private float _rotationX                        = 0;
    private Vector3 _moveDirection                  = Vector3.zero;
    [HideInInspector] public Vector2 _moveInput     = Vector2.zero;
    [HideInInspector] public Vector2 _lookInput     = Vector2.zero;
    #endregion

    #region - Gun Sway System -
    [Header("Gun Sway Data")]
    public SwayEffectors gunSwayEffectors = new SwayEffectors();

    [Space, Header("Weapon Sway System")]
    [Header("Position Sway")]
    [SerializeField, Range(0, 10)] private float swayAmount = 0.01f;
    [SerializeField, Range(0, 10)] private float maxAmount = 0.06f;
    [SerializeField, Range(0, 100)] private float smoothAmount = 6f;

    private Vector3 initialPosition;

    [Header("Rotation Sway")]
    [SerializeField, Range(0, 100)] private float rotationSwayAmount = 4f;
    [SerializeField, Range(0, 100)] private float maxRotationSwayAmount = 5f;
    [SerializeField, Range(0, 100)] private float smoothRotationAmount = 12f;

    private Quaternion initialRotation;

    [SerializeField] private bool swayOnX = true;
    [SerializeField] private bool swayOnY = true;
    [SerializeField] private bool swayOnZ = true;

    [Header("Movment Sway")]
    [SerializeField, Range(0, 10)] private float movmentSwayXAmount = 0.05f;
    [SerializeField, Range(0, 10)] private float movmentSwayYAmount = 0.05f;

    [SerializeField, Range(0, 10)] private float movmentSwaySmooth = 6f;
    [SerializeField, Range(0, 10)] private float maxMovmentSwayAmount = 0.5f;

    [Header("Breathing Weapon Sway")]
    [SerializeField, Range(0, 100)] private float swayAmountA = 4;
    [SerializeField, Range(0, 100)] private float swayAmountB = 2;

    [SerializeField, Range(0, 10000)] private float swayScale = 600;
    [SerializeField, Range(0, 10000)] private float aimSwayScale = 6000;

    [SerializeField, Range(0, 100)] private float swayLerpSpeed = 14f;

    private float swayTime;
    [SerializeField, Range(0, 100)] private float swaySpeed;
    private Vector3 swayPosition;
    #endregion

    #region - Audio System -
    [Header("Gun Public Sounds")]
    public AudioClip gunShootJam;
    public AudioClip aimClip;
    public AudioClip changeGunMode;
    #endregion

    #region - Gun Change System -
    private int gunIndex = 0;
    #endregion

    // ---------------------------- Methods ----------------------------//

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

            if (inputManager.primaryGun.WasPressedThisFrame() && !_changingWeapon) EquipGun(0);
            if (inputManager.secondaryGun.WasPressedThisFrame() && !_changingWeapon) EquipGun(1);
        }
    }
    #endregion

    #region - Update Actions -
    // ----------------------------------------------------------------------
    // Name : UpdateCalls
    // Desc : This method manage all data tha need to be updated
    //        every frame call.
    // ----------------------------------------------------------------------
    private void UpdateCalls()
    {
        _isMoving       =   controller.velocity != Vector3.zero;
        _isSprinting    =   inputManager.sprint && _isMoving && !_walkingBackwards;
        _isWalking      =   _isMoving;
        _isGrounded     =   controller.isGrounded;
        _inAir          =   !_isGrounded;
        _lookInput      =   inputManager.Look;

        if (equippedGun != null) CalculateWeaponSway();
    }
    #endregion

    #region - Look Handler -
    // ----------------------------------------------------------------------
    // Name : LookHandler
    // Desc : This method handles all the player camera and body look system.
    // ----------------------------------------------------------------------
    private void LookHandler()
    {
        //First the method get the Y vector look input and multiplies it for the Y vector sensitivity,
        //later this value is clamped under an certain limit represented as two variables (UpperLookLimit and LowerLookLimit).

        _rotationX -= _lookInput.y * _yLookSensitivity / 100;
        _rotationX = Mathf.Clamp(_rotationX, -_upperLookLimit, _lowerLookLimit);
        bodyCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        //Also, the method handles the X rotation, appling it only in the body.
        transform.rotation *= Quaternion.Euler(0, _lookInput.x * _xLookSensitivity / 100, 0);
    }
    #endregion

    #region - Movment Handler -
    // ----------------------------------------------------------------------
    // Name : MoveHandler
    // Desc : This method handles the player character movment and gravity.
    // ----------------------------------------------------------------------
    private void MoveHandler()
    {
        float speedValue = _isSprinting ? (_isCrouching ? _crouchSpeed : _sprintSpeed) : _walkSpeed;

        _walkingBackwards = inputManager.Move.y == -1;
        if (_walkingBackwards) speedValue = _walkSpeed;

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
    // Desc : This method handle the crouch system input an action limits.
    // ----------------------------------------------------------------------
    private void CrouchHandler()
    {
        if (_isGrounded && !_duringCrouch) if (inputManager.crouching) StartCoroutine(CrouchAction());
    }

    // ----------------------------------------------------------------------
    // Name : CrouchAction
    // Desc : This method alternate the crouch  state, changing the
    //        collider height and center  using an Lerp function to
    //        transit gradually between the values, also the method
    //        makes an simple check, to see if the player is trying
    //        stand up and if have  anything up of  the player, and
    //        if has, the stand is canceled.
    // ----------------------------------------------------------------------
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
    // ----------------------------------------------------------------------
    // Name : HeadBobHandler
    // Desc : This method manages the headbob movment system.
    // ----------------------------------------------------------------------
    private void HeadBobHandler()
    {
        if (!_isMoving) return;
        _timer += Time.deltaTime * (_isCrouching ? crouchBobSpeed : _isSprinting ? sprintBobSpeed : walkBobSpeed);
        bodyCamera.transform.localPosition = new Vector3(bodyCamera.transform.localPosition.x,
            defaultYPost + Mathf.Sin(_timer) * (_isCrouching ? crouchBobAmount : _isSprinting ? sprintBobAmount : walkBobAmount)
            , bodyCamera.transform.localPosition.z);
    }
    #endregion

    #region - Weapon Sway Calculations -
    private void CalculateWeaponSway()
    {
        #region - Aim Effectors -
        //This statements change all the sway mechanics values whether or not the player is aiming
        float calcSwayAmount = equippedGun._isAiming ? swayAmount / gunSwayEffectors.lookSwayEffector : swayAmount;
        float calcMaxAmount = equippedGun._isAiming ? maxAmount / gunSwayEffectors.maxLoookSwayAmountEffector : maxAmount;
        float calcMovmentSwayXAmount = equippedGun._isAiming ? movmentSwayXAmount / gunSwayEffectors.xMovmentSwayEffector : movmentSwayXAmount;
        float calcMovmentSwayYAmount = equippedGun._isAiming ? movmentSwayYAmount / gunSwayEffectors.yMovmentSwayEffector : movmentSwayYAmount;
        float calcMaxMovmentSwayAmount = equippedGun._isAiming ? maxMovmentSwayAmount / gunSwayEffectors.maxMovmentSwayEffector : maxMovmentSwayAmount;
        float calcRotationSwayAmount = equippedGun._isAiming ? rotationSwayAmount / gunSwayEffectors.rotationaSwayEffector : rotationSwayAmount;
        float calcMaxRotationSwayAmount = equippedGun._isAiming ? maxRotationSwayAmount / gunSwayEffectors.maxRotationSwayAmountEffector : maxRotationSwayAmount;
        #endregion

        #region - Weapon Position Sway Calculations -
        //This statements represent the weapon look movment sway calculations 
        float lookInputX = -_lookInput.x * calcSwayAmount;
        float lookInputY = -_lookInput.y * calcSwayAmount;

        lookInputX = Mathf.Clamp(lookInputX, -calcMaxAmount, calcMaxAmount);
        lookInputY = Mathf.Clamp(lookInputY, -calcMaxAmount, calcMaxAmount);

        Vector3 finalPosition = new Vector3(lookInputX, lookInputY, 0);
        weaponSwayObject.transform.localPosition = Vector3.Lerp(weaponSwayObject.transform.localPosition, finalPosition + initialPosition, smoothAmount * Time.deltaTime);
        #endregion

        #region - Movment Sway Calculations -
        //This statementes represent the weapon sway based on the player movment

        float movmentInputX = _moveInput.x * calcMovmentSwayXAmount;
        float movmentInputY = _moveInput.y * calcMovmentSwayYAmount;

        movmentInputX = Mathf.Clamp(movmentInputX, -calcMaxMovmentSwayAmount, calcMaxMovmentSwayAmount);
        movmentInputY = Mathf.Clamp(movmentInputY, -calcMaxMovmentSwayAmount, calcMaxMovmentSwayAmount);

        Vector3 movmentSwayFinalPosition = new Vector3(movmentInputY, movmentInputX, 0);

        weaponSwayObject.transform.localPosition = Vector3.Lerp(weaponSwayObject.transform.localPosition, movmentSwayFinalPosition + initialPosition, movmentSwaySmooth * Time.deltaTime);
        #endregion

        #region - Weapon Rotation Sway Calculations -
        //This statementes represent the weapon rotational sway calculations

        float rotationX = Mathf.Clamp(_lookInput.y * calcRotationSwayAmount, -calcMaxRotationSwayAmount, calcMaxRotationSwayAmount);
        float rotationY = Mathf.Clamp(_lookInput.x * calcRotationSwayAmount, -calcMaxRotationSwayAmount, calcMaxRotationSwayAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(swayOnX ? -rotationX : 0f, swayOnY ? -rotationY : 0f, swayOnZ ? -rotationY : 0f));

        weaponSwayObject.transform.localRotation = Quaternion.Slerp(weaponSwayObject.transform.localRotation, finalRotation * initialRotation, smoothRotationAmount * Time.deltaTime);
        #endregion

        #region - Breathing Idle Sway -
        //This statements use the LissajousCurve calculation to make an breathing idle procedural animation
        Vector3 targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / (equippedGun._isAiming ? aimSwayScale : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);

        idleSwayObject.transform.localPosition = swayPosition;

        swayTime += Time.deltaTime;

        if (swayTime > 6.3f) swayTime = 0;
        #endregion
    }
    private Vector3 LissajousCurve(float Time, float A, float B) => new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));//This method return an calculation that is used to make an procedural horizontal and vertical wave that represent an breathing idle animation
    #endregion

    private void EquipGun(int gunIndex)
    {
        if (gunsInHand[gunIndex].gameObject.activeInHierarchy || equippedGun._isReloading) return;
        this.gunIndex = gunIndex;
        _changingWeapon = true;

        if (gunIndex == 0) gunsInHand[1].GunHolst();//Selecting the gun and holsting it
        else gunsInHand[0].GunHolst();

        equippedGun = gunsInHand[gunIndex];
    }
    public void StartEquippedGun()
    {
        equippedGun.gameObject.SetActive(true);
    }
}