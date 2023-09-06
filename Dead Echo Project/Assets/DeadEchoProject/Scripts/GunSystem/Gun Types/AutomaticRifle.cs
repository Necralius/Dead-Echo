using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticRifle : Mode_Auto
{
    protected override IEnumerator Shoot()
    {
        if (_isShooting || !_canShoot) yield return null;

        _isShooting = true;
        _canShoot = false;

        BulletBase bullet = ObjectPooler.Instance.SpawnFromPool(_gunDataConteiner.gunBulletSettings._bulletTag,
            _playerController.shootPoint.transform.position,
            _playerController.shootPoint.transform.rotation).GetComponent<BulletBase>();

        bullet.Initialize(_playerController.shootPoint.transform, 
            _gunDataConteiner.gunBulletSettings._bulletSpread,
            _gunDataConteiner.gunBulletSettings._bulletSpeed, 
            _gunDataConteiner.gunBulletSettings._bulletGravity, 
            _gunDataConteiner.gunBulletSettings._bulletLifeTime, 
            _gunDataConteiner.gunBulletSettings._collisionMask,
            _gunDataConteiner.gunBulletSettings._bulletDamage,
            _gunDataConteiner.gunBulletSettings._bulletImpactForce, 
            _playerController.transform);
        StartCoroutine(base.Shoot());
    }
}