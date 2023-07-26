using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType { Enter, Stay, Exit}


// ------------------------------------------------------------------------
// Class    :  AITarget
// Desc     :  Descrives a potential target to the AI System
// ------------------------------------------------------------------------
public struct AITarget
{
    private AITargetType _type;
    private Collider _collider;
    private Vector3 _position;
    private float _distance;
    private float _time;

    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position { get { return _position; } }
    public float distance { get { return _distance; } set { _distance = value; } }
    public float time { get { return _time; } }

    public void Set(AITargetType targetType, Collider targetCollider, Vector3 targetPosition, float targetDistance)
    {
        _type = targetType;
        _collider = targetCollider;
        _position = targetPosition;
        _distance = targetDistance;
        _time = Time.time;
    }
    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _distance = Mathf.Infinity;
        _time = 0.0f;
    }
}

public abstract class AiStateMachine : MonoBehaviour
{
    public AITarget  VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    protected AIState _currentState;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();

    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;
    [SerializeField, Range(0, 15)] protected float _stoppingDistance = 1.0f;

    //Component Cache
    protected Animator _animator = null;
    protected NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    //Public properties
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }  


    // ---------------------------------
    // Name : Awake
    // Desc : Cache Components 
    // ---------------------------------
    protected virtual void Awake()
    {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
    }


    // ---------------------------------------------------------
    // Name: Start
    // Desc: Called by Unity to first update to setup the object
    // ---------------------------------------------------------
    protected virtual void Start()
    {
        AIState[] currentStates = GetComponents<AIState>();
        foreach (AIState state in currentStates)
        {
            if (state != null && _states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
        }

        if (_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else _currentState = null;
    }

    public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPosition, float targetDistance)
    {
        _target.Set(targetType, targetCollider, targetPosition, targetDistance);

        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }
    public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPosition, float targetDistance, float stoppingDistance)
    {
        _target.Set(targetType, targetCollider, targetPosition, targetDistance);

        if (_targetTrigger != null)
        {
            _targetTrigger.radius = stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    // ---------------------------------------
    // Name : ClearTarget
    // Desc : Clears the current target
    // ---------------------------------------
    public void ClearTarget()
    {
        _target.Clear();
        if (_targetTrigger != null) _targetTrigger.enabled = false;
    }

    // ----------------------------------------------------------------------
    // Name: FixedUpdate
    // Desc: Called by Unity with each tick of the Physics system. It clears
    //       the audio and visual threats each update and re-calculates the
    //       distance to the current target.
    // ----------------------------------------------------------------------
    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(transform.position, _target.position);
        }
    }

    // ----------------------------------------------------------------------
    // Name : Update
    // Desc : Called by Unity each frame. Gives the current state a chance
    //        to update itself and perform transitions.
    // ----------------------------------------------------------------------
    protected virtual void Update()
    {
        if (_currentState == null) return;
        
        AIStateType newStateType = _currentState.OnUpdate();
        if (newStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            _currentStateType = newStateType;
        }
    }
}