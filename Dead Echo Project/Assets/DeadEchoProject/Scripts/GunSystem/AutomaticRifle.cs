using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticRifle : GunBase
{
    protected override void Aim()
    {
        base.Aim();
    }
    protected override void Reload()
    {
        base.Reload();
    }
    protected override IEnumerator Shoot()
    {
        throw new System.NotImplementedException();
    }
}