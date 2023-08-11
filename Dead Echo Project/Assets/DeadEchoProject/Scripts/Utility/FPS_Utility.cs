using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NekraByte
{
    public static class FPS_Utility
    {       
        #region - Pool Model -
        [Serializable]
        public struct Pool
        {
            public string poolTag;
            public int poolSize;
            public GameObject prefab;
        }
        #endregion

        #region - Gun Data Model -
        [Serializable]
        public class GunData
        {
            [Header("Gun Aspects")]
            [SerializeField] private string gunName = "GunName";
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
                Auto_Shotgun,
                Semi_Shotgun,
                Auto_Rifle,
                Semi_Rifle,
                Semi_Pistol,
                Auto_Pistol,
                Sniper
            }
        }
        #endregion

        #region - Bullet Settings Model -
        [Serializable]
        public class BulletSettings
        {
            [Header("Gun Settings"), Tooltip("Gun aspects settings")]
            public Vector2 shootDamageRange                      = new(10f, 25f);
            [SerializeField]    public string   _bulletTag = "RifleBullet";
            [Range(1, 5000)]    public float    _bulletSpeed        = 500f;
            [Range(1, 50)]      public float    _bulletGravity      = 2f;
            [Range(1, 10)]      public float    _bulletSpread       = 1f;
            [Range(1, 15)]      public float    _bulletLifeTime     = 10f;
            public LayerMask _collisionMask;
        }
        #endregion

        #region - Gun Audio Asset -
        [Serializable]
        public struct AudioAsset
        {
            public AudioClip ShootClip;
            public AudioClip AimClip;
            public AudioClip DrawClip;
            public AudioClip HolstClip;
            public AudioClip ReloadClip;
            public AudioClip ReloadClipVar1;
            public AudioClip FullReloadClip;
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
        [CreateAssetMenu(menuName = "NekraByte/FPS_Utility/Guns/New Gun Data", fileName = "New Gun Data")]
        public class GunDataConteiner : ScriptableObject
        {
            public GunData          gunData               = new GunData();
            public BulletSettings   gunBulletSettings     = new BulletSettings();
            public AudioAsset       gunAudioAsset         = new AudioAsset();
        }
        #endregion

        #region - Reticle Model -
        public enum ReticleState
        {
            Walking,
            Spriting,
            Crouching,
            Shooting,
            Aiming,
            Reloading,
            InAir,
            Idle
        }
        #endregion
    }
}