using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static NekraByte.FPS_Utility.Core.Enumerators;

namespace NekraByte
{

    public static class FPS_Utility
    {       
        public static class Core
        {
            // --------------------------------------------------------------
            // Name: DataTypes (Class)
            // Desc: This class storages all costum DataTypes from the
            //       NekraByte Unity Package.
            // --------------------------------------------------------------
            public static class DataTypes
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

                #region - Gun Data Main Conteiner -       
                [CreateAssetMenu(menuName = "NekraByte/FPS_Utility/Guns/New Gun Data", fileName = "New Gun Data")]
                public class GunDataConteiner : ScriptableObject
                {
                    public GunData gunData = new GunData();
                    public BulletSettings gunBulletSettings = new BulletSettings();
                    public AudioAsset gunAudioAsset = new AudioAsset();

                    #region - Gun Data Model -
                    [Serializable]
                    public class GunData
                    {
                        [Header("Gun Aspects")]
                        public string gunName = "GunName";
                        public ShootType shootType;
                        public GunMode gunMode;

                        [Range(0.01f, 2f)] public float rateOfFire = 0.1f;
                    }
                    #endregion

                    #region - Bullet Settings Model -
                    [Serializable]
                    public class BulletSettings
                    {
                        [Header("Gun Settings"), Tooltip("Gun aspects settings")]
                        public Vector2 shootDamageRange = new(10f, 25f);
                        [SerializeField] public string _bulletTag = "RifleBullet";
                        [Range(1, 5000)] public float _bulletSpeed = 500f;
                        [Range(1f, 50)] public float _bulletGravity = 2f;
                        [Range(0.001f, 10)] public float _bulletSpread = 1f;
                        [Range(1, 15)] public float _bulletLifeTime = 10f;
                        [Range(1, 10)] public int _bulletsPerShoot = 1;
                        public float _bulletDamage = 15f;
                        public float _bulletImpactForce = 10f;
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
                }
                #endregion

                #region - Sway Data Model -
                [Serializable]
                public class SwayData
                {
                    [Header("Sway State")]
                    public bool _inputSway = true;
                    public bool _movementSway = true;
                    public bool _rotationalSway = true;
                    public bool _idleSway = true;

                    [Header("Input Sway")]
                    public float inpt_amount;
                    public float inpt_maxAmout;
                    public float inpt_smoothAmount;

                    public float inpt_damping;
                    public float inpt_stiffness;
                }
                #endregion

                #region - Player Movment Data Conteiner -
                [Serializable]
                public class PlayerMovmentData
                {
                    #region - Player Data Settings -
                    [Header("Player Movmennt Settings")]
                    [Range(0.01f, 10f)]     public float _walkSpeed         = 4.0f;
                    [Range(0.01f, 30f)]     public float _sprintSpeed       = 8.0f;
                    [Range(0.01f, 30f)]     public float _crouchSpeed       = 2.0f;
                    [Range(0.01f, 50f)]     public float _jumpForce         = 4.0f;
                    [Range(0.01f, 100f)]    public float _playerGravity     = 30f;

                    [Header("Controller Settings Data")]
                    [Range(0.01f, 10f)]     public float _xLookSensitivity  = 2.0f;
                    [Range(0.01f, 10f)]     public float _yLookSensitivity  = 2.0f;
                    [Range(0.01f, 100f)]    public float _upperLookLimit    = 80f;
                    [Range(0.01f, 100f)]    public float _lowerLookLimit    = 80f;

                    [Header("Crouch Settings")]
                    [Range(0.1f, 2f)]       public float _crouchHeight      = 0.5f; 
                    [Range(0.1f, 2f)]       public float _standingHeight    = 2.0f;
                    public float                         _timeToCrouch      = 0.25f;
                    public LayerMask                     _crouchUpLayer;
                    public Vector3                       _crouchingCenter   = new Vector3(0, 0.5f, 0);
                    public Vector3                       _standingCenter    = new Vector3(0, 0, 0);
                    #endregion

                    #region - HeadBob System -
                    [Header("HeadBob Settings")]
                    [Range(0, 100)] public float walkBobSpeed       = 14.0f;
                    [Range(0, 100)] public float walkBobAmount      = 0.05f;

                    [Range(0, 100)] public float sprintBobSpeed     = 18.0f;
                    [Range(0, 100)] public float sprintBobAmount    = 0.11f;

                    [Range(0, 100)] public float crouchBobSpeed     = 8.000f;
                    [Range(0, 100)] public float crouchBobAmount    = 0.025f;
                    #endregion      
                }
                #endregion
                 
            }

            // --------------------------------------------------------------
            // Name: Procedural (Class)
            // Desc: Mainly, this static class, brings some procedural tools,
            //       that can help the gun movimentation and handling.
            // --------------------------------------------------------------
            public static class Procedural
            {
                #region - Spring Lerping -
                public class SpringInterpolator
                {
                    private Vector3 _currentPos;
                    private Vector3 velocity;

                    // ------------------------------------------------------
                    // Name: SpringInterpolator (Constructor)
                    // Desc: This constructor set an importat variable to
                    //       this class, and is called on the object
                    //       instatiation.
                    // ------------------------------------------------------
                    public SpringInterpolator(Vector3 currentPos) => _currentPos = currentPos;
                    
                    // ------------------------------------------------------
                    // Name: SpringLerp
                    // Desc: This method produces an movimentation using as
                    //       base the spring interpolation math principle.
                    // ------------------------------------------------------
                    public Vector3 SpringLerp(Vector3 targetPos,float damping, float stiffness, float deltaTime)
                    {
                        Vector3 springForce = stiffness * targetPos - _currentPos;
                        Vector3 dampingForce = -damping * velocity;

                        Vector3 totalForce = springForce + dampingForce;

                        Vector3 acceleration = totalForce;
                        velocity += acceleration * deltaTime;
                        _currentPos += velocity * deltaTime;
                        return _currentPos;
                    }
                }
                #endregion
            }

            // --------------------------------------------------------------
            // Name: Enumerators
            // Desc: Mainly, this static class, brings some enumerators
            //       tools, that can help to build new descriptives
            //       DataTypes.
            // --------------------------------------------------------------
            public static class Enumerators
            {
                #region - Reticle State -
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

                #region - Gun Mode -
                public enum GunMode
                {
                    Auto,
                    Semi,
                    Locked
                }
                #endregion

                #region - ShootType -
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
                #endregion

                public enum LayerBehavior
                {
                    None, Additive, Override, Referencial
                }
            }
        }

    }
}