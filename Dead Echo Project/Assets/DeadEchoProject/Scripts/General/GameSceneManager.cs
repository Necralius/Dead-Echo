using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NekraByte.FPS_Utility;

public class PlayerInfo
{
    public Collider             collider            = null;
    public CharacterManager     characterManager    = null;
    public Camera               camera              = null;
    public CapsuleCollider      meleeTrigger        = null;
}

public class GameSceneManager : MonoBehaviour
{
    #region - Singleton Pattern -
    private static GameSceneManager _instance = null;
    public static GameSceneManager instance
    {
        get
        {
            if (_instance == null) _instance = (GameSceneManager)FindFirstObjectByType(typeof(GameSceneManager));
            return _instance;
        }
    }
    #endregion

    //Private
    private Dictionary<int, AiStateMachine> _stateMachines = new Dictionary<int, AiStateMachine>();
    private Dictionary<int, PlayerInfo> _playerInfos = new Dictionary<int, PlayerInfo>();

    [SerializeField] private ParticleSystem _bloodParticles = null;

    [SerializeField] private GameObject deathScreen = null;
    
    //Public
    public ParticleSystem bloodParticles { get => _bloodParticles; }
    //Public Methods

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    // ----------------------------------------------------------------------
    // Name : RegisterAIStateMachine
    // Desc : Stores the passed state machine in the dictionary with the
    //        instance ID as an key.
    // ----------------------------------------------------------------------
    public void RegisterAIStateMachine(int key, AiStateMachine stateMachine) { if (!_stateMachines.ContainsKey(key)) _stateMachines[key] = stateMachine; }


    // ----------------------------------------------------------------------
    // Name : GetAIStateMachine
    // Desc : Returns an AI State Machine reference searched on by the
    //        intrance ID of an object.
    // ----------------------------------------------------------------------
    public AiStateMachine GetAIStateMachine(int key)
    {
        AiStateMachine machine = null;
        
        if (_stateMachines.TryGetValue(key, out machine)) return machine;
        return null;
    }

    // ----------------------------------------------------------------------
    // Name : RegisterPlayerInfo
    // Desc : Stores the passed PlayerInfo in the dictionary with the
    //        instance ID as an key.
    // ----------------------------------------------------------------------
    public void RegisterPlayerInfo(int key, PlayerInfo playerInfo) { if (!_playerInfos.ContainsKey(key)) _playerInfos[key] = playerInfo; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;
        }
    }


    // ----------------------------------------------------------------------
    // Name : GetAIStateMachine
    // Desc : Returns an PlayerInfo reference searched on by the
    //        instance ID of an object.
    // ----------------------------------------------------------------------
    public PlayerInfo GetPlayerInfo(int key)
    {
        PlayerInfo info = null;

        if (_playerInfos.TryGetValue(key, out info)) return info;
        return null;
    }

    public void DeathScreen()
    {
        deathScreen.SetActive(true);
    }
}