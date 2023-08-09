using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static NekraByte.FPS_Utility;
using static NekraByte.FPS_Utility.GunData;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public abstract class GunBase : MonoBehaviour
{
    #region - Dependencies -
    protected Animator _animator;
    [HideInInspector] public FPS_Controller _playerController;
    protected InputManager _inputManager;
    [SerializeField] protected GunDataConteiner _gunDataConteiner = new();
    protected GunProceduralRecoil _recoilAsset => GetComponent<GunProceduralRecoil>();
    private Transform _aimHolder => _playerController.aimHolder;
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
    [SerializeField] protected bool _isShooting      = false;
    [SerializeField] protected bool _canShoot        = true;
    public bool                     _isEquiped       = false;
    public bool                     _isReloading     = false;
    public bool                     _isAiming        = false;
    #endregion

    #region - Gun Shoot System -
    [Header("Gun Shoot System")]
    [SerializeField] protected Transform _shootPoint = null;    
    #endregion

    #region - Aim System -
    [Header("Aim System"), Tooltip("Aim settings")]
    [SerializeField] private Vector3 _defaultPosition;
    [SerializeField] private Vector3 _aimPosition;
    private Vector3 _aimTargetPos;
    [SerializeField, Range(1, 20)] private float _aimSpeed;
    [SerializeField] private float _aimOffset;
    [SerializeField] private float _aimReloadOffset;
    #endregion

    #region - Animation Hashes -
    private int _isWalkingHash      = Animator.StringToHash("isWalking");
    private int _isRunningHash      = Animator.StringToHash("isRunning");
    private int _isReloadingHash    = Animator.StringToHash("isReloading");
    private int _reloadFactor       = Animator.StringToHash("ReloadFactor");
    private int _holstWeaponHash    = Animator.StringToHash("HolstWeapon");
    private int _shootHash          = Animator.StringToHash("Shoot");
    #endregion

    #region - Gun Mode System -
    [Header("Gun Mode System")]
    [SerializeField] private List<GunMode> gunModes;
    int gunModeIndex = 0;
    #endregion

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
        _playerController           = GetComponentInParent<FPS_Controller>();
        _inputManager               = InputManager.Instance;
        _animator                   = GetComponent<Animator>();
        _recoilAsset.cameraObject   = _playerController.cameraObject;

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
        if (_playerController               == null) return;
        if (_inputManager                   == null) return;
        if (_playerController.armsAnimator  == null) return;
        if (_animator                       == null) return;

        //The class focus on limiting the functionalitys, using ifs to limit the actions based in expressions.
        if (!_playerController._isSprinting)
        {
            _isAiming = _inputManager.aiming;
            _recoilAsset.isAiming = _isAiming;

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
            if (!_isReloading && _canShoot)
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
        Aim(); //-> This statement calls the aim position calculation method.

        //The below statements set the animations on the main arms animator controler.
        _playerController.armsAnimator.SetBool(_isWalkingHash, _playerController._isWalking);
        _playerController.armsAnimator.SetBool(_isRunningHash, _playerController._isSprinting);

        //NOTE: On override, always mantain the base code using the base.Update();
        //-> Otherwise, every systems of the gun base will be broken!!
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
        SS_Shoot();
        UI_Update();
        
        if (_gunDataConteiner.gunData.shootType == ShootType.Semi_Shotgun ||
            _gunDataConteiner.gunData.shootType == ShootType.Semi_Pistol ||
            _gunDataConteiner.gunData.shootType == ShootType.Sniper) _animator.SetTrigger(_shootHash);

        _recoilAsset.RecoilFire();
        _magAmmo--;

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
        if (_playerController._isSprinting) _aimTargetPos = _defaultPosition;
        else if (_isAiming)
        {
            SS_Aim();
            _aimTargetPos = new Vector3(_aimPosition.x, _aimPosition.y, _aimPosition.z + (_isReloading ? _aimReloadOffset : _aimOffset));
        }
        else _aimTargetPos = _defaultPosition;

        _aimHolder.transform.localPosition = Vector3.Lerp(_aimHolder.transform.localPosition, _aimTargetPos, _aimSpeed * Time.deltaTime);
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
        _animator.SetFloat(_reloadFactor, reloadIndex);
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
        _playerController.txt_Ammo.text = $"{_magAmmo}/{_bagAmmo}";
        _playerController.txt_gunState.text = _gunDataConteiner.gunData.gunMode.ToString();
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
        gunModeIndex++;
        if (gunModeIndex > gunModes.Count - 1) gunModeIndex = 0;

        gunModeIndex = Mathf.Clamp(gunModeIndex, 0, gunModes.Count);

        _gunDataConteiner.gunData.gunMode = gunModes[gunModeIndex];

        UI_Update(); // -> Updates the gun UI.
        SS_ChangeGunMode(); // -> Trigger the method sound.
    }
    #endregion

    #region - Sound System -
    // The following methods manages all the sound system.
    // SS -> SoundSystem
    private void SS_ChangeGunMode()
    {
        if (ValidateClip(_playerController.changeGunMode)) 
            AudioSystem.Instance.PlayGunClip(_playerController.changeGunMode);
    }
    private void SS_Aim()
    {
        if (ValidateClip(_playerController.aimClip))
            AudioSystem.Instance.PlayGunClip(_playerController.changeGunMode);
    }
    protected void SS_Shoot()
    {
        if (ValidateClip(_gunDataConteiner.gunAudioAsset.ShootClip)) 
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ShootClip, Vector2.zero, Vector2.zero);
    }
    private void SS_Reload(int reloadIndex)
    {
        switch (reloadIndex)
        {
            case 0:

                if (ValidateClip(_gunDataConteiner.gunAudioAsset.ReloadClip)) return;
                else AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ReloadClip);
                break;
            case 1:

                if (ValidateClip(_gunDataConteiner.gunAudioAsset.ReloadClipVar1)) return;
                else AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ReloadClipVar1);
                break;
            case 2:

                if (ValidateClip(_gunDataConteiner.gunAudioAsset.FullReloadClip)) return;
                else AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.FullReloadClip);
                break;
        }
    }
    private void SS_GunAwake()
    {

    }
    private void SS_GunHolst()
    {

    }
    private void SS_GunShootJam()
    {
        if (ValidateClip(_playerController.gunShootJam))
            AudioSystem.Instance.PlayGunClip(_playerController.gunShootJam);
    }
    private bool ValidateClip(AudioClip clip) => !clip.Equals(null);
    #endregion

    public void EndDraw()
    {
        _isEquiped = true;
        _canShoot = true;
    }
    public void GunHolst()
    {
        _isEquiped = false;
        _canShoot = false;
        _animator.SetTrigger(_holstWeaponHash);
    }
    public void EndHolst()
    {
        gameObject.SetActive(false);
        _playerController._changingWeapon = false;
        _playerController.StartEquippedGun();
    }
}