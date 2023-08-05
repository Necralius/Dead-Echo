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

        bullet.Initialize(_shootPoint, bulletSpeed, _bulletGravity);
        recoilAsset.RecoilFire();
        magAmmo--;

        yield return new WaitForSeconds(rateOfFire);

        isShooting = false;
    }
}