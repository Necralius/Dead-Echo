using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility;

[RequireComponent(typeof(Animator))]
public abstract class GunBase : MonoBehaviour
{
    #region - Dependencies -
    protected Animator animator;
    protected FPS_Controller playerController;
    protected InputManager inputManager;
    [SerializeField] protected GunDataConteiner gunDataConteiner = new();
    private GameObject weaponSwayObject;
    protected GunProceduralRecoil recoilAsset => GetComponent<GunProceduralRecoil>();
    private Transform aimHolder => playerController.aimHolder;
    #endregion


    [Header("Gun Ammo"), Tooltip("Gun ammo quantity settings")]
    [SerializeField, Range(0, 500)] protected int bagMaxAmmo = 200;
    [SerializeField, Range(1, 150)] protected int magMaxAmmo = 31;

    [SerializeField] protected int bagAmmo = 60;
    [SerializeField] protected int magAmmo = 31;

    [Header("Gun State"), Tooltip("Gun current states")]
    [SerializeField] protected bool isReloading     = false;
    [SerializeField] protected bool isShooting      = false;
    [SerializeField] protected bool hasSway         = true;
    public bool                     isAiming        = false;

    [Header("Gun Shoo System")]
    [SerializeField] protected Transform _shootPoint = null;
    [SerializeField] protected string bulletTag = "RifleBullet";
    [SerializeField] protected float _bulletSpeed = 50f;
    [SerializeField] protected float _bulletGravity = 10f;

    [Header("Aim System"), Tooltip("Aim settings")]
    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private Vector3 aimPosition;
    private Vector3 aimTargetPos;
    [SerializeField, Range(1, 20)] private float aimSpeed;
    [SerializeField] private float aimOffset;
    [SerializeField] private float aimReloadOffset;

    #region - Animation Hashes -
    private int _isWalkingHash      = Animator.StringToHash("isWalking");
    private int _isRunningHash      = Animator.StringToHash("isRunning");
    private int _isReloadingHash    = Animator.StringToHash("isReloading");
    private int _reloadFactor       = Animator.StringToHash("ReloadFactor");
    #endregion

    protected virtual void Start()
    {
        playerController    = GetComponentInParent<FPS_Controller>();
        inputManager        = InputManager.Instance;
        animator            = GetComponent<Animator>();
        weaponSwayObject = playerController.weaponSwayObject;
        recoilAsset.cameraObject = playerController.cameraObject;
    }
    
    protected virtual void Update()
    {
        if (playerController == null) return;
        if (inputManager == null) return;

        if (!playerController.isSprinting)
        {
            isAiming = inputManager.aiming;
            recoilAsset.isAiming = isAiming;

            if (!isReloading && inputManager.reload && !(magAmmo == magMaxAmmo) && bagAmmo > 0) Reload();
            if (!isShooting && magAmmo > 0 && !isReloading && inputManager.shooting) StartCoroutine(Shoot());

            Aim();
        }

        playerController.armsAnimator.SetBool(_isWalkingHash, playerController._isWalking);
        playerController.armsAnimator.SetBool(_isRunningHash, playerController.isSprinting);

        //NOTE: On override, always mantain the base code using the base.Update(); -> Otherwise, every systems of the gun base will be broken!!
    }
    protected abstract IEnumerator Shoot();
    private void Aim()
    {
        if (isAiming)
        {
            //TO DO -> Aim Sound Trigger

            aimTargetPos = new Vector3(aimPosition.x, aimPosition.y, aimPosition.z + (isReloading ? aimReloadOffset : aimOffset));
        }
        else if (playerController.isSprinting) aimTargetPos = defaultPosition;
        else aimTargetPos = defaultPosition;

        aimHolder.transform.localPosition = Vector3.Lerp(aimHolder.transform.localPosition, aimTargetPos, aimSpeed * Time.deltaTime);
    }

    #region - Reload Behavior -
    protected virtual void Reload()
    {
        isReloading = true;
        //TO DO -> Reload Sound Trigger

        animator.SetTrigger(_isReloadingHash);
        animator.SetFloat(_reloadFactor, magMaxAmmo - magAmmo == magMaxAmmo ? 2 : Random.Range(0, 2));
    }
    public virtual void EndReload()
    {
        int quantityNeeded = magMaxAmmo - magAmmo;
        if (quantityNeeded > bagAmmo)
        {
            magAmmo += bagAmmo;
            bagAmmo = 0;
        }
        else if (quantityNeeded == bagAmmo)
        {
            bagAmmo = 0;
            magAmmo = magMaxAmmo;
        }
        else if (quantityNeeded < bagAmmo)
        {
            magAmmo = magMaxAmmo;
            bagAmmo -= quantityNeeded;
        }
        isReloading = false;
    }
    #endregion
}