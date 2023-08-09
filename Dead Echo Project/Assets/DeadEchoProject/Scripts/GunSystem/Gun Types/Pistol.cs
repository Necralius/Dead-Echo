using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility;

public class Pistol : GunBase
{

    private void OnValidate()
    {
        _gunDataConteiner.gunData.shootType = GunData.ShootType.Semi_Pistol;
    }


}