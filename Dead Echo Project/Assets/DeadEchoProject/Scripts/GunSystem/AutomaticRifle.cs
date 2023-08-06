using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticRifle : GunBase
{
    protected override IEnumerator Shoot()
    {
        isShooting = true;
        BulletBase bullet = ObjectPooler.Instance.SpawnFromPool(bulletTag,
            _shootPoint.transform.position,
            _shootPoint.transform.rotation).GetComponent<BulletBase>();

        bullet.Initialize(_shootPoint, gunDataConteiner.gunBulletSettings._bulletSpeed, 
            gunDataConteiner.gunBulletSettings._bulletGravity, 
            gunDataConteiner.gunBulletSettings._bulletLifeTime, 
            gunDataConteiner.gunBulletSettings._collisionMask);

        recoilAsset.RecoilFire();
        magAmmo--;

        yield return new WaitForSeconds(gunDataConteiner.gunData.rateOfFire);

        isShooting = false;
    }
}