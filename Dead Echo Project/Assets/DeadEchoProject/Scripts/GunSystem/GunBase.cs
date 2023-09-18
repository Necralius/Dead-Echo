using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static NekraByte.FPS_Utility.Core.Enumerators;
using static NekraByte.FPS_Utility.Core.Procedural;
using static NekraByte.FPS_Utility.Core.DataTypes;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Animator))]
public abstract class GunBase : MonoBehaviour
{
    #region - Dependencies -
    [HideInInspector] public    Animator                _animator           = null;
    protected                   ControllerManager       _playerController   = null;
    [HideInInspector] public    CharacterManager        _characterManager   = null;
    [SerializeField] protected  GunDataConteiner        _gunDataConteiner   = null;
    protected                   InputManager            _inputManager       = null;
    protected                   GunProceduralRecoil     _recoilAsset        = null;
    private                     Transform               _aimHolder          = null;
    public                      ParticleSystem          _muzzleFlash        = null;
    public                      Transform               _rockThrowPosition  = null;
    #endregion

    #region - Ammo System Data -
    [Header("Gun Ammo"), Tooltip("Gun ammo quantity settings")]
    [SerializeField, Range(0, 500)] protected int _bagMaxAmmo = 200;
    [SerializeField, Range(1, 150)] protected int _magMaxAmmo = 31;

    [SerializeField] protected int _bagAmmo = 60;
    [SerializeField] protected int _magAmmo = 31;
    #endregion

    #region - Gun State -
    [Header("Gun State"), Tooltip("Gun current states")]
    [SerializeField] bool           _isEquiped       = false;
    public bool                     _isReloading     = false;
    public bool                     _isShooting      = false;
    public bool                     _isAiming        = false;
    public bool                     _aimOverride     = false;
    public bool                     _canShoot        = true;
    #endregion

    #region - Aim System -
    #endregion

    #region - Animation Hashes -
    private     int _isWalkingHash      = Animator.StringToHash("isWalking");
    private     int _isRunningHash      = Animator.StringToHash("isRunning");
    protected   int _isReloadingHash    = Animator.StringToHash("isReloading");
    protected   int _reloadFactorHash   = Animator.StringToHash("ReloadFactor");
    protected   int _holstWeaponHash    = Animator.StringToHash("HolstWeapon");
    protected   int _shootHash          = Animator.StringToHash("Shoot");
    #endregion

    #region - Gun Mode System -
    //[Header("Gun Mode System")]
    private List<GunMode> gunModes = new List<GunMode>();
    int _gunModeIndex = 0;
    #endregion

    #region - Gun Clipping Prevetion -
    [Header("Gun Clipping Prevention")]
    [SerializeField, Range(0.1f, 3f)] private float     _checkDistance  = 1f;
    [SerializeField] private Vector3                    _newDirection   = Vector3.zero;
    [SerializeField] private bool                       _isClipped      = false;
    [SerializeField] private LayerMask                  _clipingMask;
    private Transform                 _clipProjector  = null;

    private float       _lerpPos;
    private RaycastHit  _hit;
    #endregion

    [SerializeField] private float smoothTime = 10f;

    private Vector3 originalWeaponPosition;
    private Camera _camera;

    //----------------------------------- Methods -----------------------------------//

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name : Start
    // Desc : This method is called on the script start, the script mainly
    //        get all the system dependencies an set the default values of
    //        the class.
    // ----------------------------------------------------------------------
    protected virtual void Start()
    {
        _playerController           = GetComponentInParent<CamLocker>()._playerController;
        _animator                   = GetComponent<Animator>();
        _recoilAsset                = GetComponent<GunProceduralRecoil>();
        _inputManager               = InputManager.Instance;

        _aimHolder                  = AnimationLayer.GetAnimationLayer("AimLayer", _playerController._animLayers).layerObject.transform;
        _clipProjector              = AnimationLayer.GetAnimationLayer("GunLayer", _playerController._animLayers).layerObject.transform;
        _camera                     = AnimationLayer.GetAnimationLayer("CameraLayer", _playerController._animLayers).layerObject.GetComponent<Camera>();

        originalWeaponPosition = _aimHolder.localPosition;
        
        if (_gunDataConteiner == null) return;

        _recoilAsset.InitializeData(_gunDataConteiner.recoilData);

        gunModes = new List<GunMode>();
        switch (_gunDataConteiner.gunData.shootType)
        {
            case ShootType.Semi_Shotgun:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                break;
            case ShootType.Semi_Rifle:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                break;
            case ShootType.Semi_Pistol:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                break;
            case ShootType.Auto_Shotgun:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                gunModes.Add(GunMode.Auto);
                break;
            case ShootType.Auto_Rifle:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                gunModes.Add(GunMode.Auto);
                break;
            case ShootType.Auto_Pistol:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                gunModes.Add(GunMode.Auto);
                break;
            case ShootType.Sniper:
                gunModes.Add(GunMode.Locked);
                gunModes.Add(GunMode.Semi);
                break;
        }

        UI_Update();
    }

    // ----------------------------------------------------------------------
    // Name : Update
    // Desc : This method its called at every frame rendered, the method
    //        explanation its inside it.
    // ----------------------------------------------------------------------
    protected virtual void Update()
    {
        //First, the method verifies if all the class dependencies is valid, if its not the program behavior is returned, otherwise,
        //the program continue to his default behavior.
        if (!_isEquiped)                             return;
        if (_playerController                   == null) return;
        if (_inputManager                       == null) return;
        if (_playerController._armsAnimator     == null) return;
        if (_animator                           == null) return;
        if (_recoilAsset                        == null) return;
        
        _recoilAsset.InitializeData(_gunDataConteiner.recoilData);

        //The class focus on limiting the functionalitys, using ifs to limit the actions based in expressions.
        if (!_playerController._isSprinting)
        {
            if (!_aimOverride) 
                _isAiming = _inputManager.aiming;

            _recoilAsset._isAiming = _isAiming;

            if (_playerController._isThrowingObject) return;

            /* The below statements verifies if the player triggered the reload button
             * and if is not reloading, if the current mag ammo is different  from its
             * maximum and if has any ammo in the inventory.
             */
            if (_inputManager.reloadAction.WasPressedThisFrame() && !_isReloading && !(_magAmmo == _magMaxAmmo) && _bagAmmo > 0) Reload();

            /* The  below  statements execute  the  main gun  behavior  action,  first
             * verifing  if  the  isn't  reloading  and if  can shoot,  later  also is 
             * verified if the mag ammo is greater than zero, if it is, the  statement 
             * enters in an switch structure, that defies the shoot behavior  based on 
             * the gun state.
            */

            if (_inputManager.shootAction.WasPressedThisFrame() && _magAmmo <= 0) SS_GunShootJam();
            if (!_isReloading && _canShoot && !_isClipped)
            {
                if (_magAmmo > 0)
                {
                    switch (_gunDataConteiner.gunData.gunMode)
                    {
                        case GunMode.Auto:
                            if (_inputManager.shootAction.IsPressed()) StartCoroutine(Shoot());
                            break;
                        case GunMode.Semi:
                            if (_inputManager.shootAction.WasPressedThisFrame()) StartCoroutine(Shoot());
                            break;
                        case GunMode.Locked:
                            if (_inputManager.shootAction.WasPressedThisFrame()) SS_GunShootJam();
                            break;
                    }
                }

                if (_inputManager.shootAction.WasPressedThisFrame()) UI_Update();
                /* The below statement verifies if the user pressed the change gun mode
                 * buttons, if it is, the gun mode is changed.
                 */
                if (_inputManager.gunModeAction.WasPressedThisFrame()) ChangeGunMode();
            }
        }
        if (_inputManager.aimAction.WasPressedThisFrame()) SS_Aim();
        Aim(); //-> This statement calls the aim position calculation method.

        //The below statements set the animations on the main arms animator controler.
        _playerController._armsAnimator.SetBool(_isWalkingHash, _playerController._isWalking);
        _playerController._armsAnimator.SetBool(_isRunningHash, _playerController._isSprinting);

        ClipPrevetionBehavior();

        //NOTE: On override, always mantain the base code using the base.Update();
        //-> Otherwise, every systems of the gun base will be broken!!
    }
    #endregion

    #region - Clip Prevetion Behavior -
    // ----------------------------------------------------------------------
    // Name: ClipPreventionBehavior
    // Desc: This method prevent the gun clipping in surfaces, basically the
    //       method detects an surface collision in front of the gund,
    //       later, the gun rotates towards the new rotation seted on the
    //       _newDirection.
    // ----------------------------------------------------------------------
    private void ClipPrevetionBehavior()
    {
        if (_clipProjector == null)
        {
            Debug.Log("Clip projector is Null");
            return;
        }
        
        if (Physics.Raycast(_clipProjector.transform.position, _clipProjector.transform.forward, out _hit, _checkDistance, _clipingMask))
        {
            _lerpPos = 1 - (_hit.distance / _checkDistance);
            _isClipped = true;
        }
        else
        {
            _lerpPos = 0;
            _isClipped = false;
        }

        Mathf.Clamp01(_lerpPos);

        _aimHolder.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(_newDirection), _lerpPos);
    }
    #endregion

    #region - Shoot Behavior -
    // ----------------------------------------------------------------------
    // Name : Shoot
    // Desc : This method represents an base shoot behavior that compulsorily
    //        needs to be overrided on inherited class.
    // ----------------------------------------------------------------------
    protected virtual IEnumerator Shoot()
    {
        if (_gunDataConteiner == null) yield break;
        if (_muzzleFlash == null) yield break;
        if (_recoilAsset == null) yield break;

        SS_Shoot();


        if (_gunDataConteiner.gunData.shootType == ShootType.Semi_Shotgun ||
            _gunDataConteiner.gunData.shootType == ShootType.Semi_Pistol  ||
            _gunDataConteiner.gunData.shootType == ShootType.Sniper) _animator.SetTrigger(_shootHash);

        _recoilAsset.RecoilFire();
        _magAmmo--;
        _muzzleFlash.Emit(1);
        UI_Update();

        yield return new WaitForSeconds(_gunDataConteiner.gunData.rateOfFire);

        _isShooting = false;
        _canShoot = true;
    }
    #endregion

    #region - Aim System -
    // ----------------------------------------------------------------------
    // Name : Aim
    // Desc : This method manages the aim behavior, changing the aim target
    //        position and adding offsets based in the current gun state. 
    // ----------------------------------------------------------------------
    private void Aim()
    {
        if (_aimHolder  == null) return;
        if (_camera     == null) return;
        if (_isClipped)         return;

        if (_isAiming)
        {
            // Calculate the target screen position at the center of the screen
            Vector3 targetScreenPosition = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

            // Convert the screen position to a world position based on the weapon's distance from the camera
            float distanceFromCamera = Vector3.Distance(_aimHolder.position, _camera.transform.position);
            Vector3 targetWorldPosition = _camera.ScreenToWorldPoint(new Vector3(targetScreenPosition.x, targetScreenPosition.y, distanceFromCamera));

            // Convert the world position to a local position relative to the weapon
            Vector3 targetLocalPosition = _aimHolder.parent.InverseTransformPoint(targetWorldPosition);

            // Apply the specified offsets
            targetLocalPosition += new Vector3(_gunDataConteiner.gunData.aimOffset.x, 
                _gunDataConteiner.gunData.aimOffset.y, 
                _gunDataConteiner.gunData.aimOffset.z);

            // Apply the reload offset if applicable
            targetLocalPosition.z = _isReloading ? _gunDataConteiner.gunData.aimOffset.z + _gunDataConteiner.gunData._aimReloadOffset : 
                _gunDataConteiner.gunData.aimOffset.z;

            // Lerp the weapon position to match the target position
            _aimHolder.localPosition = Vector3.Lerp(_aimHolder.localPosition, targetLocalPosition, Time.deltaTime * smoothTime);
        }
        else _aimHolder.localPosition = Vector3.Lerp(_aimHolder.localPosition, originalWeaponPosition, Time.deltaTime * smoothTime);
    }
    #endregion

    #region - Reload Behavior -
    // ----------------------------------------------------------------------
    // Name : Reload
    // Desc : This method execute an reload action, playing the proper
    //        reload animation.
    // OBS  : The reload animation selection works based on the ammo needs,
    //        if the need is equals the maximum mag  ammo, the  full reload
    //        animation is played, is equals the maximum mag ammo, the full
    //        reload  animation  is  played, otherwise, one  of  two reload
    //        variations is randomly selected.
    // ----------------------------------------------------------------------
    protected virtual void Reload()
    {
        _isReloading = true;

        int reloadIndex = _magMaxAmmo - _magAmmo == _magMaxAmmo ? 2 : Random.Range(0, 2);

        SS_Reload(reloadIndex);

        _animator.SetTrigger(_isReloadingHash);
        _animator.SetFloat(_reloadFactorHash, reloadIndex);
    }

    // ----------------------------------------------------------------------
    // Name : EndReload
    // Desc : This method triggers the reload calculations and end the
    //        reload state boolean.
    // ----------------------------------------------------------------------
    public virtual void EndReload()
    {
        int quantityNeeded = _magMaxAmmo - _magAmmo;
        if (quantityNeeded > _bagAmmo)
        {
            _magAmmo += _bagAmmo;
            _bagAmmo = 0;
        }
        else if (quantityNeeded == _bagAmmo)
        {
            _bagAmmo = 0;
            _magAmmo = _magMaxAmmo;
        }
        else if (quantityNeeded < _bagAmmo)
        {
            _magAmmo = _magMaxAmmo;
            _bagAmmo -= quantityNeeded;
        }

        UI_Update();

        _isReloading = false;
    }
    #endregion

    #region - UI Update -
    // ----------------------------------------------------------------------
    // Name : UI_Update
    // Desc : This method updates the gun UI system.
    // ----------------------------------------------------------------------
    public void UI_Update()
    {
        if (!_isEquiped) return;
        _playerController.txt_magAmmo.text = ($"{_magAmmo}");
        _playerController.txt_bagAmmo.text = ($"/{_bagAmmo}");
        _playerController.txt_GunName.text = _gunDataConteiner.gunData.gunName;

        InGame_UIManager.Instance.UpdateMode(_gunDataConteiner.gunData.gunMode, gunModes);
    }
    #endregion

    #region - Change Gun Mode -
    // ----------------------------------------------------------------------
    // Name : ChangeGunMode
    // Desc : This method manages the gun mode change system, changing and
    //        clamping  the gun  mode represented in  an int  value, later
    //        passing the value to the gun data conteiner, also the method
    //        play the change gun mode sound and updates the UI.
    // ----------------------------------------------------------------------
    private void ChangeGunMode()
    {
        _gunModeIndex++;
        if (_gunModeIndex > gunModes.Count - 1) _gunModeIndex = 0;

        _gunModeIndex = Mathf.Clamp(_gunModeIndex, 0, gunModes.Count);

        _gunDataConteiner.gunData.gunMode = gunModes[_gunModeIndex];

        UI_Update(); // -> Updates the gun UI.
        SS_ChangeGunMode(); // -> Trigger the method sound.        
    }
    #endregion

    #region - Sound System -
    // The following methods manages all the sound system.
    // SS -> SoundSystem
    private void SS_ChangeGunMode()
    {
        if (_playerController.changeGunMode != null) 
            AudioSystem.Instance.PlayGunClip(_playerController.changeGunMode);
    }
    private void SS_Aim()
    {
        if (_gunDataConteiner.gunAudioAsset.AimClip != null)
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.AimClip);
    }
    protected void SS_Shoot()
    {
        if (_gunDataConteiner.gunAudioAsset.ShootClip != null) 
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ShootClip, Vector2.zero, Vector2.zero);
    }
    protected void SS_Reload(int reloadIndex)
    {
        switch (reloadIndex)
        {
            case 0:

                if (_gunDataConteiner.gunAudioAsset.ReloadClip != null) 
                    AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ReloadClip);
                break;
            case 1:

                if (_gunDataConteiner.gunAudioAsset.ReloadClipVar1 != null) 
                    AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ReloadClipVar1);
                break;
            case 2:

                if (_gunDataConteiner.gunAudioAsset.FullReloadClip != null) 
                    AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.FullReloadClip);
                break;
        }
    }
    private void SS_GunAwake()
    {
        if (_gunDataConteiner.gunAudioAsset.DrawClip != null) 
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.DrawClip);   
    }
    private void SS_GunHolst()
    {
        if (_gunDataConteiner.gunAudioAsset.HolstClip != null) 
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.HolstClip);
    }
    private void SS_GunShootJam()
    {
        if (_playerController.gunShootJam != null)
            AudioSystem.Instance.PlayGunClip(_playerController.gunShootJam);
    }
    #endregion

    #region - Inventory Guns Change -
    public void DrawGun()
    {
        gameObject.SetActive(true);
        SS_GunAwake();        
    }
    public void EndDraw()
    {
        _isEquiped  = true;
        _canShoot   = true;
        UI_Update();
    }
    public void GunHolst()
    {
        _animator.SetTrigger(_holstWeaponHash);
        _isEquiped  = false;
        _canShoot   = false;
        SS_GunHolst();
    }
    public void EndHolst()
    {
        gameObject.SetActive(false);
        _playerController._changingWeapon = false;
        _playerController.EquipCurrentGun();
    }
    #endregion

    #region - Throw Rock System -
    public void ThrowRock()
    {
        GameObject rock = ObjectPooler.Instance.SpawnFromPool("Rock", _rockThrowPosition.position, _rockThrowPosition.rotation);
        if (rock.GetComponent<Rigidbody>())
        {
            Rigidbody rb = rock.GetComponent<Rigidbody>();
            Vector3 forceToAdd = _playerController._cameraObject.forward *
                _playerController.objectThrowForce +
                transform.up *
                _playerController.objectThrowUpForce;

            rb.AddForce(forceToAdd, ForceMode.Impulse);
        }
    }
    #endregion
}