using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //Public Methods

    // ----------------------------------------------------------------------
    // Name : RegisterAIStateMachine
    // Desc : Stores the passed state machine in the dictionary with the
    //        instance ID as an key.
    // ----------------------------------------------------------------------
    public void RegisterAIStateMachine(int key, AiStateMachine stateMachine)
    {
        if (!_stateMachines.ContainsKey(key)) _stateMachines[key] = stateMachine;
    }

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

    






}