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
            public Vector2 shootDamageRange                         = new(10f, 25f);
            [SerializeField]    public string   _bulletTag          = "RifleBullet";
            [Range(1, 5000)]    public float    _bulletSpeed        = 500f;
            [Range(1f, 50)]     public float    _bulletGravity      = 2f;
            [Range(0.001f, 10)] public float    _bulletSpread       = 1f;
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

        #region - Sway Data Model -
        [Serializable]
        public class SwayData
        {
            [Header("Weapon Sway Data")]
            public SwayEffectors gunSwayEffectors = new SwayEffectors();

            [Space, Header("Weapon Sway System")]
            [Header("Position Sway")]
            [Range(0, 10)] public float swayAmount = 0.01f;
            [Range(0, 10)] public float maxAmount = 0.06f;
            [Range(0, 100)] public float smoothAmount = 6f;

            [HideInInspector] public Vector3 initialPosition;

            [Header("Rotation Sway")]
            [Range(0, 100)] public float rotationSwayAmount = 4f;
            [Range(0, 100)] public float maxRotationSwayAmount = 5f;
            [Range(0, 100)] public float smoothRotationAmount = 12f;

            [HideInInspector] public Quaternion initialRotation;

            public bool swayOnX = true;
            public bool swayOnY = true;
            public bool swayOnZ = true;

            [Header("Movment Sway")]
            [Range(0, 10)] public float movmentSwayXAmount = 0.05f;
            [Range(0, 10)] public float movmentSwayYAmount = 0.05f;

            [Range(0, 10)] public float movmentSwaySmooth = 6f;
            [Range(0, 10)] public float maxMovmentSwayAmount = 0.5f;

            [Header("Breathing Weapon Sway")]
            [Range(0, 100)] public float swayAmountA = 4;
            [Range(0, 100)] public float swayAmountB = 2;

            [Range(0, 10000)] public float swayScale = 600;
            [Range(0, 10000)] public float aimSwayScale = 6000;

            [Range(0, 100)] public float swayLerpSpeed = 14f;

            [HideInInspector] public float swayTime;
            [HideInInspector] public Vector3 swayPosition;
            #region - Sway Effectors Data Model -
            [Serializable]
            public class SwayEffectors
            {
                [Header("Aiming Effectors")]
                [Range(1, 100)] public float lookSwayEffector = 10f;
                [Range(1, 100)] public float maxLoookSwayAmountEffector = 5f;
                [Range(1, 100)] public float xMovmentSwayEffector = 10f;
                [Range(1, 100)] public float yMovmentSwayEffector = 10f;
                [Range(1, 100)] public float maxMovmentSwayEffector = 5f;
                [Range(1, 100)] public float rotationaSwayEffector = 5f;
                [Range(1, 100)] public float maxRotationSwayAmountEffector = 5;
            }
            #endregion
        }
        #endregion
    }
}