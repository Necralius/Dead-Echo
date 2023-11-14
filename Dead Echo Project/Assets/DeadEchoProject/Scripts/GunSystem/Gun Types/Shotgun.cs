using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Mode_Semi
{
    //Private Data
    private int endReloadHash       = Animator.StringToHash("EndReload");
    private int cancelReloadHash    = Animator.StringToHash("CancelReload");


    protected override void Update()
    {
        if (_isReloading && _inputManager.shootAction.WasPressedThisFrame())
        {
            _animator.SetTrigger(cancelReloadHash);
            _isReloading = false;
            UI_Update();
        }
        base.Update();
    }
    protected override IEnumerator Shoot()
    {
        if (_isShooting || !_canShoot) yield return null;

        _isShooting = true;
        _canShoot   = false;

        for (int i = 0; i < _gunDataConteiner.gunBulletSettings._bulletsPerShoot; i++)
        {            
            BulletBase bullet = ObjectPooler.Instance.SpawnFromPool(_gunDataConteiner.gunBulletSettings._bulletTag,
                _playerController._shootPoint.transform.position,
                _playerController._shootPoint.transform.rotation).GetComponent<BulletBase>();

            bullet.Initialize(_playerController._shootPoint.transform,
           _gunDataConteiner.gunBulletSettings._bulletSpread,
           _gunDataConteiner.gunBulletSettings._bulletSpeed,
           _gunDataConteiner.gunBulletSettings._bulletGravity,
           _gunDataConteiner.gunBulletSettings._bulletLifeTime,
           _gunDataConteiner.gunBulletSettings._collisionMask,
           _gunDataConteiner.gunBulletSettings._shootDamageRange,
           _gunDataConteiner.gunBulletSettings._bulletImpactForce,
           _playerController.transform);
        }
        StartCoroutine(base.Shoot());
    }

    protected override void Reload()
    {
        _isReloading = true;
        SS_Reload();

        _animator.SetTrigger(_isReloadingHash);
        _animator.SetFloat(_reloadFactorHash, 0);
    }
    public override void EndReload()
    {
        UI_Update();

        _isReloading = false;
    }
    public void TriggerBullet()
    {
        if (_gunDataConteiner.ammoData._magAmmo < (_gunDataConteiner.ammoData._magMaxAmmo - 1)) _gunDataConteiner.ammoData._magAmmo++;
        UI_Update();
    }
    public void VerifyNeed()
    {
        if (!(_gunDataConteiner.ammoData._magAmmo < (_gunDataConteiner.ammoData._magMaxAmmo - 1))) _animator.SetTrigger(endReloadHash);
    }
    public void TriggerLastBullet()
    {
        _gunDataConteiner.ammoData._magAmmo++;
        UI_Update();
    }

    private void SS_Reload()
    {
        if (_gunAudioAsset.ReloadClip != null)
            AudioManager.Instance.PlayOneShotSound("Effects", _gunAudioAsset.AimClip, transform.position, 1f, 0f, 128);
    }
    public void SS_BulletTrigger()
    {
        if (_gunAudioAsset.ReloadClipVar1 != null)
            AudioManager.Instance.PlayOneShotSound("Effects", _gunAudioAsset.ReloadClipVar1, transform.position, 1f, 0f, 128);
    }
    public void SS_PumpAction()
    {
        if (_gunAudioAsset.BoltActionClip != null)
            AudioManager.Instance.PlayOneShotSound("Effects", _gunAudioAsset.BoltActionClip, transform.position, 1f, 0f, 128);
    }
}