using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility;

public class Pistol : Mode_Semi
{
    protected override IEnumerator Shoot()
    {
        if (_isShooting || !_canShoot) yield return null;

        _isShooting     = true;
        _canShoot       = false;

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
        StartCoroutine(base.Shoot());
    }

}