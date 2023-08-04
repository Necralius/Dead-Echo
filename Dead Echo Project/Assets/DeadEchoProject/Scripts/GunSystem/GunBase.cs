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
    #endregion

    [Header("Gun Settings"), Tooltip("Gun aspects settings")]
    [SerializeField] protected Vector2 shootDamageRange = new(10f,25f);
    [SerializeField, Range(0.01f, 3f)] protected float rateOfFire = 0.6f;
    [SerializeField, Range(5f, 1000f)] protected float bulletSpeed = 30;

    [Header("Gun Ammo"), Tooltip("Gun ammo quantity settings")]
    [SerializeField, Range(1, 500)] protected int bagAmmo = 60;
    [SerializeField, Range(1, 500)] protected int bagMaxAmmo = 200;

    [SerializeField, Range(1, 150)] protected int magMaxAmmo = 31;
    [SerializeField, Range(1, 150)] protected int magAmmo = 31;

    [Header("Gun State"), Tooltip("Gun current states")]
    [SerializeField] protected bool isReloading;
    [SerializeField] protected bool isShooting;
    [SerializeField] protected bool isAiming;

    [SerializeField] Transform shootPoint = null;

    [Header("Aim System"), Tooltip("Aim settings")]
    [SerializeField] private Transform aimHolder;
    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private Vector3 aimPosition;
    private Vector3 aimTargetPos;
    [SerializeField, Range(1, 20)] private float aimSpeed;
    [SerializeField] private float aimOffset;
    [SerializeField] private float aimReloadOffset;  

    private int _isWalkingHash = Animator.StringToHash("isWalking");
    private int _isRunningHash = Animator.StringToHash("isRunning");
    private int _isReloadingHash = Animator.StringToHash("isReloading");

    protected abstract IEnumerator Shoot();
    protected virtual void Aim()
    {
        if (isAiming) aimTargetPos = new Vector3(aimPosition.x, aimPosition.y, aimPosition.z + (isReloading ? aimReloadOffset : aimOffset));
        else aimTargetPos = defaultPosition;

        transform.localPosition = Vector3.Lerp(transform.localPosition, aimTargetPos, aimSpeed * Time.deltaTime);
    }
    protected virtual void Reload() { isReloading = true; }

    protected virtual void Start()
    {
        inputManager        = InputManager.Instance;
        animator            = GetComponent<Animator>();
        playerController    = GetComponentInParent<FPS_Controller>();
    }
    
    protected virtual void Update()
    {
        if (playerController == null) return;
        if (inputManager == null) return;

        if (!playerController.isSprinting)
        {
            if (inputManager.reload) Reload();
            isAiming = inputManager.aiming;

            Aim();
        }

        animator.SetBool(_isWalkingHash, playerController._isWalking);
        animator.SetBool(_isRunningHash, playerController.isSprinting);

    }
}