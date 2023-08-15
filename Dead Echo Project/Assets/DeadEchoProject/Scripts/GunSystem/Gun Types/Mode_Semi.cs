using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode_Semi : GunBase
{
    public void CanShoot() => _canShoot = true;
    public void CannotShoot() => _canShoot = false;


}