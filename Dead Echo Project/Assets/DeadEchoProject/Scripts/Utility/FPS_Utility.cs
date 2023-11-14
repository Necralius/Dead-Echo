using System;
using UnityEngine;
using System.Collections.Generic;
using static NekraByte.FPS_Utility.Core.Enumerators;
using static NekraByte.FPS_Utility.Core.DataTypes;
using System.IO;
using System.Collections;
using UnityEngine.Audio;
using UnityEditor;

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

                #region - Data Saving System -
                [Serializable]
                public class SaveDirectoryData
                {
                    public string savePath          = string.Empty;
                    public string saveFolderPath    = string.Empty;
                    public string screenshotPath    = string.Empty;
                }

                #region - Game Save Data -
                [Serializable]
                public class GameSaveData
                {
                    public GameSaveData(string saveName)
                    {
                        this.saveName   = saveName;
                        saveTime        = new DateTimeSerialized(DateTime.Now);
                    }

                    [Header("Save Data")]
                    public string               saveName                = string.Empty;
                    public DateTimeSerialized   saveTime                = null;
                    public SaveDirectoryData    saveDirectoryData       = null;
                    public int                  saveIndex               = 0;

                    [Header("Player Data")]
                    public Vector3      playerPosition      = Vector3.zero;
                    public Vector3      currentVelocity     = Vector3.zero;
                    public Quaternion   playerRotation      = Quaternion.identity;
                    public Quaternion   cameraRotation      = Quaternion.identity;

                    public float        playerHealth        = 0f;

                    [Header("Gun Data")]
                    public int GunID = 0;

                    public List<GunDataConteiner.AmmoData> guns = new List<GunDataConteiner.AmmoData>();

                    public void UpdateSaveHour() => saveTime = new DateTimeSerialized(DateTime.Now);

                    public Texture2D GetImage()
                    {
                        Texture2D newText = new Texture2D(2, 2);

                        if (File.Exists(saveDirectoryData.screenshotPath)) 
                            newText.LoadImage(File.ReadAllBytes(saveDirectoryData.screenshotPath));

                        return newText;
                    }
                }
                #endregion

                #region - Application Permanent Data -
                [Serializable]
                public class ApplicationData
                {
                    public ApplicationData()
                    {
                        //Updates the save time hour.
                        saveTime = new DateTimeSerialized(DateTime.Now);

                        //Create defaul save directorys holders 
                        for (int i = 0; i < 3; i++)
                            saveDirectorys.Add(new SaveDirectoryData());

                        //Create a default resolution
                        currentResolution.width = 1920;
                        currentResolution.height = 1080;

                        //Reset all settings to default
                        ResetToDefault();
                    }

                    public List<SaveDirectoryData> saveDirectorys = new List<SaveDirectoryData>();

                    public DateTimeSerialized saveTime;

                    [Header("Game Settings")]

                    [Space]

                    [Header("Audio Settings")]
                    public List<AudioTrackVolume> _volumes = new List<AudioTrackVolume>();

                    [Space]

                    [Header("Graphics Settings")]

                    [Header("Resolution")]
                    public Resolution   currentResolution;
                    public int          resolutionIndex    = 0;

                    public bool         isFullscreen       = true;
                    [Header("vSync")]
                    public bool         vSyncActive        = false;
                    public int          vSyncCount         = 0;

                    [Header("Shadows")]
                    public int shadowQuality        = 1;
                    public int shadowResolution     = 1;

                    [Header("Quality Settings")]
                    public int qualityLevelIndex    = 1;

                    public int anisotropicFiltering = 1;
                    public int antialiasing         = 1;
                    public int globalTextureQuality = 1;

                    public float brightness         = 1f;

                    [Header("Gameplay Settings")]
                    public float xSensitivity       = 7f;
                    public float ySensitivity       = 7f;

                    public bool invertX             = false;
                    public bool invertY             = false;

                    public int aimType              = 0;
                    public int crouchType           = 0;                 

                    public void StartNewSave(int saveIndex, SaveDirectoryData info)
                    {
                        saveDirectorys[saveIndex] = info;
                    }

                    public bool GetSavePathByIndex(int saveIndexer, out SaveDirectoryData directory)
                    {
                        if (saveIndexer >= saveDirectorys.Count || saveIndexer < 0)
                        {
                            directory = null;
                            return false;
                        }
                        directory = saveDirectorys[saveIndexer];
                        return true;
                    }
                    public bool ExistSaves() => saveDirectorys.Count > 0;

                    public bool DeleteSaveByIndex(int saveIndex)
                    {
                        try
                        {
                            if (saveDirectorys[saveIndex] != null)
                            {
                                DeleteData(saveDirectorys[saveIndex]);
                                saveDirectorys[saveIndex] = new SaveDirectoryData();
                            }                       
                            else return false;

                            return true;
                        }
                        catch(Exception e)
                        {
                            Debug.LogWarning(e);
                            return false;
                        }
                    }

                    private void DeleteData(SaveDirectoryData data)
                    {
                        if (File.Exists(data.savePath))             File.Delete(data.savePath);
                        if (File.Exists(data.screenshotPath))       File.Delete(data.screenshotPath);
                        if (Directory.Exists(data.saveFolderPath))  Directory.Delete(data.saveFolderPath);
                    }

                    public void UpdateSave() => saveTime = new DateTimeSerialized(DateTime.Now);

                    public void ResetToDefault()
                    {
                        ResetVolumeSettings();
                        ResetGraphicsSettings();
                        ResetGameplaySettings();
                    }

                    public void ResetVolumeSettings()
                    {
                        _volumes.Clear();
                    }

                    public void ResetGraphicsSettings()
                    {
                        resolutionIndex    = 0;

                        isFullscreen       = true;

                        vSyncCount         = 0;
                        vSyncActive        = false;

                        shadowQuality       = 1;
                        shadowResolution    = 1;

                        anisotropicFiltering    = 1;
                        antialiasing            = 1;
                    }              
                    public void ResetGameplaySettings()
                    {
                        xSensitivity    = 7f;
                        ySensitivity    = 7f;

                        invertX         = false;
                        invertY         = false;

                        aimType         = 0;
                        crouchType      = 0;
                    }
                }

                [Serializable]
                public class DateTimeSerialized
                {
                    public string Hour      = "00";
                    public string Minute    = "00";

                    public string Day       = "00";
                    public string Month     = "00";
                    public string Year      = "00";

                    public DateTimeSerialized(DateTime dateTime)
                    {
                        Hour    = dateTime.Hour.    ToString();
                        Minute  = dateTime.Minute.  ToString();
                        Day     = dateTime.Day.     ToString();
                        Month   = dateTime.Month.   ToString();
                        Year    = dateTime.Year.    ToString();
                    }
                }

                #endregion

                #region - Save System Interaction -
                public interface IDataPersistence
                {
                    void RegisterDataSaver();
                    void Load(GameSaveData gameData);
                    void Save(GameSaveData gameData);
                }
                #endregion

                #endregion

                #region - Gun Data Main Conteiner -       
                [Serializable]
                public class GunDataConteiner
                {
                    [Header("Gun General Data")]
                    public GunData          gunData             = new GunData();
                    public BulletSettings   gunBulletSettings   = new BulletSettings();
                    public RecoilData       recoilData          = new RecoilData();
                    public AmmoData         ammoData            = new AmmoData();

                    public void LoadData(AmmoData savedConteiner) => ammoData = savedConteiner;

                    #region - Gun Data Model -
                    [Serializable]
                    public class GunData
                    {
                        [Header("Gun Aspects")]
                        public string gunName = "GunName";
                        public ShootType shootType;
                        public GunMode gunMode;

                        [Range(0.01f, 2f)] public float rateOfFire = 0.1f;

                        [Header("Aim Data")]
                        public Vector3 aimOffset = new Vector3();
                        public float _aimReloadOffset = 0.3f;
                    }
                    #endregion

                    #region - Bullet Settings Model -
                    [Serializable]
                    public class BulletSettings
                    {
                        [Header("Gun Settings"), Tooltip("Gun Bullet Settings")]
                        public Vector2                          _shootDamageRange   = new(10f, 25f);
                        [SerializeField]        public string   _bulletTag          = "RifleBullet";
                        [Range(1, 1000)]        public float    _bulletSpeed        = 200f;
                        [Range(0.1f, 50)]       public float    _bulletGravity      = 2f;
                        [Range(0.001f, 10)]     public float    _bulletSpread       = 0.1f;
                        [Range(1f, 15f)]        public float    _bulletLifeTime     = 5f;
                        [Range(1, 10)]          public int      _bulletsPerShoot    = 1;
                        [Range(1f, 30f)]        public float    _bulletImpactForce  = 10f;
                        public LayerMask                        _collisionMask;
                    }
                    #endregion

                    #region - Gun Recoil Asset -
                    [Serializable]
                    public class RecoilData
                    {
                        [Header("Recoil Vertical Settings")]
                        public float _recoilX = -3;
                        public float _recoilY = 3;
                        public float _recoilZ = 2;

                        [Header("Back Recoil")]
                        public float _zKickback = 0.4f;

                        [Header("Recoil Smoothing")]
                        public float _snappiness = 6f;
                        public float _returnSpeed = 20f;

                        [Header("Aim Effector"), Tooltip("When aiming the player receive less recoil, this variable change the recoil reduction")]
                        public float _recoilReduction = 0.3f;
                    }
                    #endregion

                    #region - Ammo Data -
                    [Serializable]
                    public class AmmoData
                    {
                        [Header("Gun Ammo"), Tooltip("Gun ammo quantity settings")]
                        [Range(0, 500)] public int  _bagMaxAmmo     = 200;
                        [Range(1, 150)] public int  _magMaxAmmo     = 31;

                        public int                  _bagAmmo        = 60;
                        public int                  _magAmmo        = 31;
                    }
                    #endregion
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

                #region - Sway Data Model -
                [Serializable]
                public class SwayData
                {
                    [Header("Sway State")]
                    public bool _inputSway      = true;
                    public bool _movementSway   = true;
                    public bool _idleSway       = true;

                    [Header("Input Sway")]
                    public float inpt_amountX;
                    public float inpt_amountY;
                    public float inpt_smoothAmount;
                    public float inpt_swayResetSmooth;

                    public float inpt_clampX = 12f;
                    public float inpt_clampY = 12f;

                    [Header("Weapon Movement Sway")]
                    public float move_amountX;
                    public float move_amountY;
                    public float move_SmoothAmount;

                    [Header("Idle Sway")]
                    public float swayAmountA    = 1f;
                    public float swayAmountB    = 2f;
                    public float swayScale      = 400f;
                    public float swayLerpSpeed  = 14f;

                    [HideInInspector] public float swayTime;

                    [Header("Aim Effectors")]
                    public float idle_AimEffector = 0.3f;
                    public float inpt_AimEffector = 0.3f;
                    public float move_AimEffector = 0.3f;
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

                #region - Character UI State -
                [Serializable]
                public struct CharacterState
                {                  
                    public StateType type;
                    public Sprite stateSprite;
                }
                #endregion

                #region - Audio Data -

                #region - Audio Pool Data -
                // ----------------------------------------------------------
                // Name: AudioPoolItem (Class)
                // Desc: This class declares an data holder for an audio entity that is used in an audio pooling system.
                // ----------------------------------------------------------
                public class AudioPoolItem
                {
                    public GameObject   GameObject  = null;
                    public Transform    Transform   = null;
                    public AudioSource  AudioSource = null;
                    public float        Uniportance = float.MaxValue;
                    public bool         Playing     = false;
                    public IEnumerator  Coroutine   = null;
                    public ulong        ID          = 0;


                }
                #endregion

                #region - Audio Track Info -
                // --------------------------------------------------------------------------
                // Name: TrackInfo (Class)
                // Desc: This class holds the AudioMixerGroup base information and holds an
                //       Coroutine that executes the track fading system.
                // --------------------------------------------------------------------------
                public class TrackInfo
                {
                    public string           Name        = string.Empty;
                    public AudioMixerGroup  Group       = null;
                    public IEnumerator      TrackFader  = null;
                }

                [Serializable]
                public class AudioTrackVolume
                {
                    public string   Name    = string.Empty;
                    public float    Volume  = 0f;
                    public AudioTrackVolume(string name, float volume)
                    {
                        Name = name;
                        Volume = volume;
                    }
                }
                #endregion

                #endregion

                #region - Floor Data -
                [Serializable]
                public class FloorData
                {
                    //Data Types
                    public enum FloorType       { Grass, Dirt, Stone, Metal, Wood, None}
                    public enum FloorHolder     { Terrain, GameObject, None}

                    //Private Data
                    private FloorHolder             _holder                 = FloorHolder.GameObject;
                    private GameObject              _objectToFindFloor      = null;
                    private Collider                _objectCollider         = null;
                    private GameObject              _findedFloor            = null;

                    [SerializeField] private FloorType               _type                      = FloorType.None;
                    [SerializeField] private TerrainTextureDetector  _terrainTextureDetector    = null;
                    [SerializeField] private float                   _footDistance              = 0.5f;

                    //Public Data
                    public FloorType Type { get => _type = UpdateFloorType(_objectToFindFloor.transform); }

                    public FloorData(GameObject objectToFindFloor, Collider objectCollider)
                    {
                        _objectToFindFloor      = objectToFindFloor;
                        _objectCollider         = objectCollider;
                        _terrainTextureDetector = new TerrainTextureDetector(objectToFindFloor, _objectCollider);
                    }

                    public FloorType UpdateFloorType(Transform objectToFindFloor)
                    {
                        GameObject floorFinded = null;
                        RaycastHit hit;

                        if (Physics.Raycast(objectToFindFloor.position, Vector3.down, out hit, _objectCollider.bounds.extents.y + _footDistance))
                        {
                            _holder = hit.transform.CompareTag("Terrain") ? FloorHolder.Terrain : FloorHolder.GameObject;

                            floorFinded = hit.transform.gameObject;
                            //Debug.Log($"FD -> Finded the object: {floorFinded.gameObject.name}");
                        }
                        else
                        {
                            _findedFloor = null;
                            Debug.Log("FD -> Floor object not finded!");
                            return FloorType.None;
                        }

                        _findedFloor = floorFinded;

                        if (floorFinded == null) return FloorType.None;
                        else if (_holder == FloorHolder.GameObject)
                        {
                            //Debug.Log($"Floor Finded: {floorFinded.gameObject.name}, Tag: {floorFinded.tag}");
                            switch(floorFinded.tag)
                            {
                                case "Dirt"     : return FloorType.Dirt;
                                case "Stone"    : return FloorType.Stone;
                                case "Concrete" : return FloorType.Stone;
                                case "Metal"    : return FloorType.Metal;
                                case "Wood"     : return FloorType.Wood;
                                default         : return FloorType.None;
                            }
                        }
                        else if (_holder == FloorHolder.Terrain)
                        {
                            //Debug.Log($"FD -> Terrain Texture Finded: {_terrainTextureDetector.GetCurrentTexture()}");
                            switch (_terrainTextureDetector.GetCurrentTexture())
                            {
                                case "Dirt"     : return FloorType.Dirt;
                                case "Grass"    : return FloorType.Grass;
                                case "Stone"    : return FloorType.Stone;
                                case "StonePath": return FloorType.Stone;
                                case "None"     : return FloorType.None;
                                default         : return FloorType.None;
                            }
                        }
                        return FloorType.None;
                    }
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

                #region - Json File Data Handler -
                public class FileDataHandler
                {
                    private string dataDirPath = "";
                    private string dataFileName = "";

                    public GameSaveData data;

                    public FileDataHandler(string dataDirPath, string dataFileName)
                    {
                        this.dataDirPath = dataDirPath;
                        this.dataFileName = dataFileName;
                    }

                    public void SaveGunData(GameSaveData data)
                    {
                        string fullPath = Path.Combine(dataDirPath, dataFileName);
                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                            string dataToStore = JsonUtility.ToJson(data, true);

                            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                            {
                                using (StreamWriter writer = new StreamWriter(stream)) writer.Write(dataToStore);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.ToString());
                        }
                    }
                    public GameSaveData LoadGunData()
                    {
                        string fullPath = Path.Combine(dataDirPath, dataFileName);
                        GameSaveData loadedData = null;
                        if (File.Exists(fullPath))
                        {
                            try
                            {
                                string dataToLoad = "";
                                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                                {
                                    using (StreamReader reader = new StreamReader(stream)) dataToLoad = reader.ReadToEnd();
                                }

                                loadedData = JsonUtility.FromJson<GameSaveData>(dataToLoad);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                        return loadedData;
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

                #region - Animation Layer Type -
                public enum LayerBehavior
                {
                    None, 
                    Additive, 
                    Override, 
                    Referencial
                }
                #endregion

                #region - Player State Types -
                public enum StateType 
                { 
                    Stand,
                    Crouch,
                    Sliding,
                    Jumping
                }
                public enum MovementState { Idle, Walking, Sprinting, Crouching, Air, Sliding }
                #endregion
            }
        }
    }
}