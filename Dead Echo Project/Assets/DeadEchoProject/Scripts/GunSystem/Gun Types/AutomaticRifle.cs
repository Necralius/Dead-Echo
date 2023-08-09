using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticRifle : GunBase
{
    protected override IEnumerator Shoot()
    {
        if (_isShooting || !_canShoot) yield return null;

        _isShooting = true;
        _canShoot = false;

        BulletBase bullet = ObjectPooler.Instance.SpawnFromPool(_gunDataConteiner.gunBulletSettings._bulletTag,
            _shootPoint.transform.position,
            _shootPoint.transform.rotation).GetComponent<BulletBase>();

        bullet.Initialize(_shootPoint, _gunDataConteiner.gunBulletSettings._bulletSpeed, 
            _gunDataConteiner.gunBulletSettings._bulletGravity, 
            _gunDataConteiner.gunBulletSettings._bulletLifeTime, 
            _gunDataConteiner.gunBulletSettings._collisionMask);
        StartCoroutine(base.Shoot());
    }
}