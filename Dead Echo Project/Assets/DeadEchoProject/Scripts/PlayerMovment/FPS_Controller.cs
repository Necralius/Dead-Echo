using NekraliusDevelopmentStudio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NekraByte.FPS_Utility;

public class FPS_Controller : MonoBehaviour
{
    #region - Singleton Pattern -
    //This code section represents an singleton pattern implementation.
    public static FPS_Controller Instance { get; private set; }
    #endregion

    #region - Controller Dependencies -
    private CharacterController controller  => GetComponent<CharacterController>();
    private Transform bodyCamera            => GetComponentInChildren<Camera>().transform.parent;
    public GameObject cameraObject          => GetComponentInChildren<Camera>().gameObject;
    private InputManager inputManager       => InputManager.Instance != null ? InputManager.Instance : null;
    #endregion

    #region - Gun System -
    [Header("Gun System Dependencies")]
    public Transform    aimHolder;
    public Animator     armsAnimator;
    public GameObject   recoilHolder;
    public GameObject   shootPoint;


    [Header("Gun System")]
    public GunBase          equippedGun;
    public List<GunBase>    gunsInHand;
    #endregion

    #region - UI Elements -
    [Header("UI Elements")]
    public TextMeshProUGUI txt_magAmmo;
    public TextMeshProUGUI txt_bagAmmo;
    public TextMeshProUGUI txt_GunName;
    #endregion

    #region - Player Data -
    [Header("Player Data")]
    public PlayerMovmentData playerData;

    private float defaultYPost = 0;
    private float _timer;
    #endregion

    #region - Player State -
    [Header("Player State")]
    public      bool _canMove            = true;
    public      bool _isWalking          = false;
    public      bool _isSprinting        = false;
    public      bool _isCrouching        = false;
    public      bool _canCrouch          = false;
    public      bool _duringCrouch       = false;
    public      bool _isMoving           = false;
    public      bool _isGrounded         = true;
    public      bool _inAir              = false;
    public      bool _changingWeapon     = false;
    public      bool _isThrowingObject   = false;
    public      bool _flashlightActive   = false;
    private     bool _walkingBackwards   = false;
    #endregion

    #region - Private Data -
    private float _rotationX                        = 0;
    private Vector3 _moveDirection                  = Vector3.zero;
    [HideInInspector] public Vector2 _moveInput     = Vector2.zero;
    [HideInInspector] public Vector2 _lookInput     = Vector2.zero;
    #endregion

    #region - Audio System -
    [Header("Gun Public Sounds")]
    public AudioClip gunShootJam;
    public AudioClip changeGunMode;
    public AudioClip flashlightSound;
    #endregion

    #region - Gun Change System -
    private int gunIndex = 0;
    #endregion

    #region - Gun Sway System -
    [Header("Weapon Sway")]
    public GameObject weaponSwayObject;
    public GameObject idleSwayObject;

    [Header("Sway Data")]
    [SerializeField] private SwayData swayData;

    #endregion

    #region - Animation Hashes -
    private int objectThrowingHash    = Animator.StringToHash("ThrowObject");
    private int objectThrowCancelHash = Animator.StringToHash("ObjectThrowCancel");
    private int objectInstantThrow  = Animator.StringToHash("ObjectInstantThrow");
    #endregion

    #region - Rock Throwing System -
    [Header("Rock Throwing Forces")]
    public float objectThrowForce = 5f;
    public float objectThrowUpForce = 5f;
    #endregion

    #region - Flash Light System -
    public GameObject flashLight;


    #endregion

    float _dragMultiplier       = 1f;
    float _dragMultiplierLimit  = 1f;
    [SerializeField, Range(0f, 1f)] float _npcStickiness = 0.5f;

    public float dragMultiplierLimit    { get => _dragMultiplierLimit;  set => _dragMultiplierLimit = Mathf.Clamp01(value); }
    public float dragMultiplier         { get => _dragMultiplier;       set => _dragMultiplier      = Mathf.Min(value, _dragMultiplierLimit); }

    // ---------------------------- Methods ----------------------------//

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name: Awake
    // Desc: This method is called on the first application frame.
    // ----------------------------------------------------------------------
    private void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    //
    // Name: Start
    // Desc: This method is called on the game start, mainly he 
    //
    private void Start()
    {
        InGame_UIManager.Instance.UpdatePlayerState(this);
        defaultYPost = bodyCamera.transform.localPosition.y;
    }
    // ----------------------------------------------------------------------
    // Name: Update
    // Desc: This method is called every frame, mainly the method handle the
    //       input actions, the movment complete system, the flashlight and
    //       the object throwing system.
    // ----------------------------------------------------------------------
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

            if (!equippedGun._isReloading && !_isSprinting)
            {
                if (inputManager.throwRockAction.WasPressedThisFrame()) StartThrowing();
                if (_isThrowingObject)
                {
                    if (inputManager.throwRockAction.WasReleasedThisFrame()) ThrowRock();

                    if (inputManager.aimAction.WasPressedThisFrame() || 
                        inputManager.reloadAction.WasPressedThisFrame()) CancelThrowRock();
                }
            }
            if (inputManager.flashLightAction.WasPressedThisFrame()) ChangeFlashlightState();
        }
        _dragMultiplier = Mathf.Min(_dragMultiplier + Time.deltaTime, _dragMultiplierLimit);
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
        _isSprinting    =   inputManager.sprint && _isMoving && !_walkingBackwards && !_isCrouching;
        _isWalking      =   _isMoving;
        _isGrounded     =   controller.isGrounded;
        _inAir          =   !_isGrounded;
        _lookInput      =   inputManager.Look;

        if (equippedGun != null) CalculateWeaponSway();

        ReticleManager.Instance.DataReceiver(this);
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

        _rotationX -= _lookInput.y * playerData._yLookSensitivity / 100;
        _rotationX = Mathf.Clamp(_rotationX, -playerData._upperLookLimit, playerData._lowerLookLimit);
        bodyCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        //Also, the method handles the X rotation, appling it only in the body.
        transform.rotation *= Quaternion.Euler(0, _lookInput.x * playerData._xLookSensitivity / 100, 0);
    }
    #endregion

    #region - Movment Handler -
    // ----------------------------------------------------------------------
    // Name : MoveHandler
    // Desc : This method handles the player character movment and gravity.
    // ----------------------------------------------------------------------
    private void MoveHandler()
    {
        float speedValue = _isSprinting ? playerData._sprintSpeed : (_isCrouching ? playerData._crouchSpeed : playerData._walkSpeed);

        _walkingBackwards = inputManager.Move.y == -1;
        if (_walkingBackwards) speedValue = _isCrouching ? playerData._crouchSpeed : playerData._walkSpeed;

        _moveInput = new Vector2(inputManager.Move.y * speedValue * dragMultiplier, inputManager.Move.x * speedValue * dragMultiplier);

        float moveDirectionY = _moveDirection.y;
        _moveDirection = (transform.TransformDirection(Vector3.forward) * _moveInput.x) + (transform.TransformDirection(Vector3.right) * _moveInput.y);
        _moveDirection.y = moveDirectionY;

        if (!controller.isGrounded) _moveDirection.y -= playerData._playerGravity * Time.deltaTime;
        controller.Move(_moveDirection * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (GameSceneManager.instance.GetAIStateMachine(hit.collider.GetInstanceID()) != null) 
            _dragMultiplier = 1f - _npcStickiness;     
    }
    #endregion

    #region - Jump System -
    // ----------------------------------------------------------------------
    // Name : JumpHandler
    // Desc : This method handle the jump action on the player character.
    // ----------------------------------------------------------------------
    private void JumpHandler()
    {
        if (_isGrounded) if (inputManager.jumping) _moveDirection.y = playerData._jumpForce;
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
        InGame_UIManager.Instance.UpdatePlayerState(this);
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
        if (_isCrouching && Physics.Raycast(bodyCamera.transform.position, Vector3.up, 1.5f, playerData._crouchUpLayer)) yield break;

        _duringCrouch = true;

        float timeElapsed = 0;
        float targetHeight = _isCrouching ? playerData._standingHeight : playerData._crouchHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = _isCrouching ? playerData._standingCenter : playerData._crouchingCenter;
        Vector3 currentCenter = controller.center;

        while(timeElapsed < playerData._timeToCrouch)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / playerData._timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / playerData._timeToCrouch);
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
        _timer += Time.deltaTime * (_isCrouching ? playerData.crouchBobSpeed : _isSprinting ? playerData.sprintBobSpeed : playerData.walkBobSpeed);
        bodyCamera.transform.localPosition = new Vector3(bodyCamera.transform.localPosition.x,
            defaultYPost + Mathf.Sin(_timer) * (_isCrouching ? playerData.crouchBobAmount : _isSprinting ? playerData.sprintBobAmount : playerData.walkBobAmount)
            , bodyCamera.transform.localPosition.z);
    }
    #endregion

    #region - Weapon Sway Calculations -
    private void CalculateWeaponSway()
    {
        #region - Aim Effectors -
        //This statements change all the sway mechanics values whether or not the player is aiming
        float calcSwayAmount = equippedGun._isAiming ? swayData.swayAmount / swayData.gunSwayEffectors.lookSwayEffector : swayData.swayAmount;
        float calcMaxAmount = equippedGun._isAiming ? swayData.maxAmount / swayData.gunSwayEffectors.maxLoookSwayAmountEffector : swayData.maxAmount;
        float calcMovmentSwayXAmount = equippedGun._isAiming ? swayData.movmentSwayXAmount / swayData.gunSwayEffectors.xMovmentSwayEffector : swayData.movmentSwayXAmount;
        float calcMovmentSwayYAmount = equippedGun._isAiming ? swayData.movmentSwayYAmount / swayData.gunSwayEffectors.yMovmentSwayEffector : swayData.movmentSwayYAmount;
        float calcMaxMovmentSwayAmount = equippedGun._isAiming ? swayData.maxMovmentSwayAmount / swayData.gunSwayEffectors.maxMovmentSwayEffector : swayData.maxMovmentSwayAmount;
        float calcRotationSwayAmount = equippedGun._isAiming ? swayData.rotationSwayAmount / swayData.gunSwayEffectors.rotationaSwayEffector : swayData.rotationSwayAmount;
        float calcMaxRotationSwayAmount = equippedGun._isAiming ? swayData.maxRotationSwayAmount / swayData.gunSwayEffectors.maxRotationSwayAmountEffector : swayData.maxRotationSwayAmount;
        #endregion

        #region - Weapon Position Sway Calculations -
        //This statements represent the weapon look movment sway calculations 
        float lookInputX = -_lookInput.x * calcSwayAmount;
        float lookInputY = -_lookInput.y * calcSwayAmount;

        lookInputX = Mathf.Clamp(lookInputX, -calcMaxAmount, calcMaxAmount);
        lookInputY = Mathf.Clamp(lookInputY, -calcMaxAmount, calcMaxAmount);

        Vector3 finalPosition = new Vector3(lookInputX, lookInputY, 0);
        weaponSwayObject.transform.localPosition = Vector3.Lerp(weaponSwayObject.transform.localPosition, finalPosition + swayData.initialPosition, swayData.smoothAmount * Time.deltaTime);
        #endregion

        #region - Movment Sway Calculations -
        //This statementes represent the weapon sway based on the player movment

        float movmentInputX = _moveInput.x * calcMovmentSwayXAmount;
        float movmentInputY = _moveInput.y * calcMovmentSwayYAmount;

        movmentInputX = Mathf.Clamp(movmentInputX, -calcMaxMovmentSwayAmount, calcMaxMovmentSwayAmount);
        movmentInputY = Mathf.Clamp(movmentInputY, -calcMaxMovmentSwayAmount, calcMaxMovmentSwayAmount);

        Vector3 movmentSwayFinalPosition = new Vector3(movmentInputY, movmentInputX, 0);

        weaponSwayObject.transform.localPosition = Vector3.Lerp(weaponSwayObject.transform.localPosition, movmentSwayFinalPosition + swayData.initialPosition, swayData.movmentSwaySmooth * Time.deltaTime);
        #endregion

        #region - Weapon Rotation Sway Calculations -
        //This statementes represent the weapon rotational sway calculations

        float rotationX = Mathf.Clamp(_lookInput.y * calcRotationSwayAmount, -calcMaxRotationSwayAmount, calcMaxRotationSwayAmount);
        float rotationY = Mathf.Clamp(_lookInput.x * calcRotationSwayAmount, -calcMaxRotationSwayAmount, calcMaxRotationSwayAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(swayData.swayOnX ? -rotationX : 0f, swayData.swayOnY ? -rotationY : 0f, swayData.swayOnZ ? -rotationY : 0f));

        weaponSwayObject.transform.localRotation = Quaternion.Slerp(weaponSwayObject.transform.localRotation, finalRotation * swayData.initialRotation, swayData.smoothRotationAmount * Time.deltaTime);
        #endregion

        #region - Breathing Idle Sway -
        //This statements use the LissajousCurve calculation to make an breathing idle procedural animation
        Vector3 targetPosition = LissajousCurve(swayData.swayTime, swayData.swayAmountA, swayData.swayAmountB) / (equippedGun._isAiming ? swayData.aimSwayScale : swayData.swayScale);

        swayData.swayPosition = Vector3.Lerp(swayData.swayPosition, targetPosition, Time.smoothDeltaTime * swayData.swayLerpSpeed);

        idleSwayObject.transform.localPosition = swayData.swayPosition;

        swayData.swayTime += Time.deltaTime;

        if (swayData.swayTime > 6.3f) swayData.swayTime = 0;
        #endregion
    }
    private Vector3 LissajousCurve(float Time, float A, float B) => new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));//This method return an calculation that is used to make an procedural horizontal and vertical wave that represent an breathing idle animation
    #endregion

    #region - Gun Equip System -
    private void EquipGun(int gunToEquip)
    {
        gunIndex = gunToEquip;

        if (gunsInHand[gunIndex].gameObject.activeInHierarchy || 
            equippedGun._isReloading || 
            _changingWeapon) return;

        _changingWeapon = true;

        if (gunIndex == 0) gunsInHand[1].GunHolst();//Selecting the gun and holsting it
        else gunsInHand[0].GunHolst();

        equippedGun = gunsInHand[gunIndex];
    }
    public void EquipCurrentGun() => equippedGun.DrawGun();
    #endregion

    #region - Throw Rock State -
    private void StartThrowing()
    {
        equippedGun._animator.SetBool(objectThrowingHash, true);
        _isThrowingObject = true;
    }
    public void ThrowRock()
    {
        //TODO -> Get object from pool and set on position, activate the throw interaction with gravity.
        equippedGun._animator.SetBool(objectThrowingHash, false);
        _isThrowingObject = false;
    }
    private void CancelThrowRock()
    {
        equippedGun._animator.SetTrigger(objectThrowCancelHash);
        _isThrowingObject = false;
    }
    #endregion

    #region - Sound System -
    private void SS_Flashlight()
    {
        if (!flashlightSound.Equals(null)) AudioSystem.Instance.PlayGunClip(flashlightSound);
    }
    #endregion

    #region - Flashlight System -
    private void ChangeFlashlightState()
    {
        flashLight.SetActive(!flashLight.activeInHierarchy);
        SS_Flashlight();
    }
    #endregion
}