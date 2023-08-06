using System;
using UnityEngine;

namespace NekraByte
{
    public class FPS_Utility
    {    
        [Serializable]
        public struct Pool
        {
            public string poolTag;
            public int poolSize;
            public GameObject prefab;
        }

        #region - Gun Data Model -
        [Serializable]
        public class GunData
        {
            [Header("Gun Aspects")]
            public ShootType shootType;
            public GunMode gunMode;

            [Range(0.01f, 2f)] public float rateOfFire = 0.1f;

            public enum GunMode
            {
                Auto,
                Semi,
                Locked
            }
            public enum ShootType
            {
                Shotgun,
                Sniper,
                Rifle,
                Pistol
            }
        }
        #endregion

        #region - Bullet Settings Model -
        [Serializable]
        public class BulletSettings
        {
            [Header("Gun Settings"), Tooltip("Gun aspects settings")]
            public Vector2 shootDamageRange                      = new(10f, 25f);
            [Range(1, 5000)]    public float _bulletSpeed        = 100f;
            [Range(1, 50)]      public float _bulletGravity      = 2f;
            [Range(1, 10)]      public float _bulletSpread       = 2f;
            [Range(1, 15)]      public float _bulletLifeTime     = 10f;
            public LayerMask                 _collisionMask;
        }
        #endregion

        #region - Gun Audio Asset -
        [Serializable]
        public struct AudioAsset
        {
            public AudioClip ShootClip;
            public AudioClip AimClip;
            public AudioClip DrawClip;
            public AudioClip ReloadClip;
            public AudioClip EmptyMagClip;
            public AudioClip ChangeGunModeClip;
            public AudioClip BoltActionClip;
        }
        #endregion

        #region - Sway Effectors Data Model -
        [Serializable]
        public class SwayEffectors
        {
            [Header("Aiming Effectors")]
            [Range(1, 100)] public float lookSwayEffector               = 10f;
            [Range(1, 100)] public float maxLoookSwayAmountEffector     = 5f;
            [Range(1, 100)] public float xMovmentSwayEffector           = 10f;
            [Range(1, 100)] public float yMovmentSwayEffector           = 10f;
            [Range(1, 100)] public float maxMovmentSwayEffector         = 5f;
            [Range(1, 100)] public float rotationaSwayEffector          = 5f;
            [Range(1, 100)] public float maxRotationSwayAmountEffector  = 5;
        }
        #endregion

        #region - Gun Data Main Conteiner -
        [Serializable]
        public class GunDataConteiner
        {
            public GunData          gunData               = new GunData();
            public BulletSettings   gunBulletSettings     = new BulletSettings();
            public AudioAsset       gunAudioAsset         = new AudioAsset();
        }
        #endregion
    }
}