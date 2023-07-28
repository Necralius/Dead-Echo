using System.Collections.Generic;
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
    //Public
    public AITarget  VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    //Protected
    protected AIState _currentState;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;


    //Protected Inspector Assigned
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
    public Vector3 sensorPosition
    {
        get
        {
            if (_sensorTrigger == null) return Vector3.zero;
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }
    public float sensorRadius
    {
        get
        {
            if (_sensorTrigger == null) return 0.0f;
            float radius = Mathf.Max(   _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x, 
                                        _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);
            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }
    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }   
    public AITargetType targetType {  get { return _target.type; } }
    public Vector3 targetPosition { get { return _target.position; } }

    // ---------------------------------
    // Name : Awake
    // Desc : Cache Components 
    // ---------------------------------
    protected virtual void Awake()
    {
        //Get and store all frequently acessed components
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        if (GameSceneManager.instance != null)
        {
            //Register State Machines with Scene Database
            if (_collider) GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }
    }


    // ---------------------------------------------------------
    // Name: Start
    // Desc: Called by Unity to first update to setup the object
    // ---------------------------------------------------------
    protected virtual void Start()
    {
        if (_sensorTrigger != null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();
            if (script != null) script.parentStateMachine = this;
        }

        AIState[] states = GetComponents<AIState>();
        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
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

        if (_animator)
        {
            AIStateMachineLink[] _scripts = _animator.GetBehaviours<AIStateMachineLink>();
            foreach(AIStateMachineLink link in _scripts) link.stateMachine = this;
        }
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

    public void SetTarget(AITarget target)
    {
        // Assign the new target
        _target = target;

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = target.position;
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
            else
            if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    // ----------------------------------------------------------------------
    // Name : OnTriggerEnter
    // Desc : Called by Physics system when the AI's Main collider enters
    //        its trigger. This Allows the child state to know when it has
    //        entered the sphere of influence of a waypoint or last player
    //        sighted position.
    // ----------------------------------------------------------------------
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        if (_currentState) _currentState.OnDestinationReached(true);
    }
    public virtual void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other) return;
        if (_currentState != null) _currentState.OnDestinationReached(false);
    }
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (_currentState != null) _currentState.OnTriggerEvent(type, other);
    }

    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null) _currentState.OnAnimatorUpdated();
    }
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null) _currentState.OnAnimatorIKUpdated();
    }
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (navAgent)
        {
            navAgent.updatePosition = positionUpdate;
            navAgent.updateRotation = rotationUpdate;
        }
    }
    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }
}