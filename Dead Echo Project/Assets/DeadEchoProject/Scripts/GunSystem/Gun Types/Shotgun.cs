using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Mode_Semi
{
    public int bulletsPerShoot = 4;
    private int endReloadHash = Animator.StringToHash("EndReload");

    protected override IEnumerator Shoot()
    {
        if (_isShooting || !_canShoot) yield return null;

        _isShooting = true;
        _canShoot = false;

        for (int i = 0; i < _gunDataConteiner.gunBulletSettings._bulletSpread; i++)
        {
            float bulletSpread = _gunDataConteiner.gunBulletSettings._bulletSpread;
            Vector3 dirVariation = new Vector3(Random.Range(-bulletSpread, bulletSpread), Random.Range(-bulletSpread, bulletSpread), Random.Range(-bulletSpread, bulletSpread));

            BulletBase bullet = ObjectPooler.Instance.SpawnFromPool(_gunDataConteiner.gunBulletSettings._bulletTag,
                _playerController.shootPoint.transform.position,
                _playerController.shootPoint.transform.rotation).GetComponent<BulletBase>();

            bullet.Initialize(_playerController.shootPoint.transform, dirVariation, _gunDataConteiner.gunBulletSettings._bulletSpeed,
                _gunDataConteiner.gunBulletSettings._bulletGravity,
                _gunDataConteiner.gunBulletSettings._bulletLifeTime,
                _gunDataConteiner.gunBulletSettings._collisionMask);
        }
            StartCoroutine(base.Shoot());
    }

    public override void Reload()
    {
        _isReloading = true;
        SS_Reload();

        _animator.SetTrigger(_isReloadingHash);
        _animator.SetFloat(_reloadFactor, 0);
    }
    public override void EndReload()
    {
        UI_Update();

        _isReloading = false;
    }
    public void TriggerBullet()
    {
        if (_magAmmo < (_magMaxAmmo - 1)) _magAmmo++;
        UI_Update();
    }
    public void VerifyNeed()
    {
        if (!(_magAmmo < (_magMaxAmmo - 1))) _animator.SetTrigger(endReloadHash);
    }
    public void TriggerLastBullet()
    {
        _magAmmo++;
        UI_Update();
    }

    private void SS_Reload()
    {
        if (ValidateClip(_gunDataConteiner.gunAudioAsset.ReloadClip))
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ReloadClip);
    }
    public void SS_BulletTrigger()
    {
        if (ValidateClip(_gunDataConteiner.gunAudioAsset.ReloadClipVar1))
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.ReloadClipVar1);
    }
    public void SS_PumpAction()
    {
        if (ValidateClip(_gunDataConteiner.gunAudioAsset.BoltActionClip))
            AudioSystem.Instance.PlayGunClip(_gunDataConteiner.gunAudioAsset.BoltActionClip);
    }
}