using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class GunBase : MonoBehaviour
{
    #region - Dependencies -
    protected Animator animator;
    protected FPS_Controller playerController;
    protected InputManager inputManager;
    protected GunProceduralRecoil recoilAsset => GetComponent<GunProceduralRecoil>();
    private Transform aimHolder => playerController.aimHolder;
    #endregion

    [Header("Gun Settings"), Tooltip("Gun aspects settings")]
    [SerializeField] protected Vector2 shootDamageRange = new(10f,25f);
    [SerializeField, Range(0.01f, 3f)] protected float rateOfFire = 0.6f;
    [SerializeField, Range(5f, 1000f)] protected float bulletSpeed = 30;

    [Header("Gun Ammo"), Tooltip("Gun ammo quantity settings")]
    [SerializeField, Range(0, 500)] protected int bagMaxAmmo = 200;
    [SerializeField, Range(1, 150)] protected int magMaxAmmo = 31;

    [SerializeField] protected int bagAmmo = 60;
    [SerializeField] protected int magAmmo = 31;

    [Header("Gun State"), Tooltip("Gun current states")]
    [SerializeField] protected bool isReloading;
    [SerializeField] protected bool isShooting;
    [SerializeField] protected bool isAiming;

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

    private int _isWalkingHash      = Animator.StringToHash("isWalking");
    private int _isRunningHash      = Animator.StringToHash("isRunning");
    private int _isReloadingHash    = Animator.StringToHash("isReloading");
    private int _reloadFactor       = Animator.StringToHash("ReloadFactor");

    protected virtual void Start()
    {
        playerController    = GetComponentInParent<FPS_Controller>();
        inputManager        = InputManager.Instance;
        animator            = GetComponent<Animator>();
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
            if ((!isShooting && magAmmo > 0 && !isReloading) && inputManager.shooting) StartCoroutine(Shoot());

            Aim();
        }

        playerController.armsAnimator.SetBool(_isWalkingHash, playerController._isWalking);
        playerController.armsAnimator.SetBool(_isRunningHash, playerController.isSprinting);
    }
    protected abstract IEnumerator Shoot();
    protected virtual void Aim()
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

}