using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;

public enum AIStateType         { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType        { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType  { Enter, Stay, Exit}
public enum AIBoneAligmentType  { XAxis, YAxis, ZAxis, XAxisInverted, YAxisInverted, ZAxisInverted }
public enum PatrolType          { Waypoint, RandomPatrol }

// ------------------------------------------------------------------------
// Class    :  AITarget
// Desc     :  This structure storages all data from an potential target.
// ------------------------------------------------------------------------
public struct AITarget
{
    private AITargetType    _type;
    private Collider        _collider;
    private Vector3         _position;
    private float           _distance;
    private float           _time;

    public AITargetType type    { get => _type;      }
    public Collider collider    { get => _collider;  }
    public Vector3 position     { get => _position;  }
    public float distance       { get => _distance; set => _distance = value;  }
    public float time           { get => _time;      }

    public void Set(AITargetType targetType, Collider targetCollider, Vector3 targetPosition, float targetDistance)
    {
        _type       = targetType;
        _collider   = targetCollider;
        _position   = targetPosition;
        _distance   = targetDistance;
        _time       = Time.time;
    }
    public void Clear()
    {
        _type       = AITargetType.None;
        _collider   = null;
        _position   = Vector3.zero;
        _distance   = Mathf.Infinity;
        _time       = 0.0f;
    }
}

public abstract class AiStateMachine : MonoBehaviour
{
    [Header("Waypoint System")]
    [SerializeField] protected PatrolType           _patrolType                = PatrolType.Waypoint;

    [SerializeField] protected bool                 _randomWaypointPatrol      = false;
    [SerializeField] protected WaypointNetwork      _waypointNetwork           = null;
    [SerializeField] protected float                _patrolRadius              = 10f;

    protected int                                   _currentWaypoint           = -1;

    //Public
    public AITarget     VisualThreat    = new AITarget();
    public AITarget     AudioThreat     = new AITarget();

    //Protected
    protected AIState                           _currentState;
    protected Dictionary<AIStateType, AIState>  _states                         = new Dictionary<AIStateType, AIState>();
    protected AITarget                          _target                         = new AITarget();
    protected int                               _rootPositionRefCount           = 0;
    protected int                               _rootRotationRefCount           = 0;
    protected bool                              _isTargetReached                = false;
    protected List<Rigidbody>                   _bodyParts                      = new List<Rigidbody>();
    protected int                               _aiBodyPartLayer                = -1;

    protected Dictionary<string, bool>          _animLayersActive = new Dictionary<string, bool>();

    //Protected Inspector Assigned
    [Header("Dependencies")]
    [SerializeField] protected Transform            _rootBone                   = null;
    [SerializeField] protected SphereCollider       _targetTrigger              = null;
    [SerializeField] protected SphereCollider       _sensorTrigger              = null;

    [Header("State Management")]
    [SerializeField] protected AIStateType          _currentStateType           = AIStateType.Idle;
    [SerializeField] protected AIBoneAligmentType   _rootBoneAligmentType       = AIBoneAligmentType.ZAxis;

    [SerializeField, Range(0, 15)] protected float  _stoppingDistance           = 1f;

    protected ILayeredAudioSource _layeredAudioSource = null;

    // Dependencies Cache
    protected   Animator      _animator   = null;
    protected   NavMeshAgent  _navAgent   = null;
    public      Collider      _collider   = null;
    protected   Transform     _transform  = null;

    //IK
    [Header("Iverse Kinematics")]
    [SerializeField] private bool       _useIK = true;

    [Header("Foot IK")]
    [SerializeField] private bool       _footIK             = true;
    [SerializeField] private float      _distanceToGround   = 1f;
    [SerializeField] private LayerMask  _groundMask;


    // Public Encapsulated Data
    public bool             isTargetReached { get =>_isTargetReached;   }
    public bool             inMeleeRange    { get; set;                 }
    public Animator         animator        { get => _animator;         }
    public NavMeshAgent     navAgent        { get => _navAgent;         }  
    public Vector3          sensorPosition
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
    public bool useRootPosition     { get => _rootPositionRefCount > 0;                                 }
    public bool useRootRotation     { get => _rootRotationRefCount > 0;                                 }
    public AITargetType targetType  { get => _target.type;                                              }
    public Vector3 targetPosition   { get => _target.position;                                          }
    public int targetColliderID     { get => _target.collider ? _target.collider.GetInstanceID() : -1;  }
    public bool UseIK               { get => _useIK; set => _useIK = value; }
    public bool UseFootIK           { get => _footIK; set => _footIK = value; }
    public PatrolType patrolType    { get => _patrolType; }
    public bool RandomWaypointPatrol { get => _randomWaypointPatrol; set => _randomWaypointPatrol = value; }

    public void SetLayerActive(string layerName, bool active)
    {
        _animLayersActive[layerName] = active;
        if (active == false && _layeredAudioSource != null) 
            _layeredAudioSource.Stop(_animator.GetLayerIndex(layerName));
    }

    public bool IsLayerActive(string layerName)
    {
        bool result;
        if (_animLayersActive.TryGetValue(layerName, out result)) 
            return result;
        return false;
    }

    public bool PlayAudio(AudioCollection clipPool, int bank, int layer, bool looping = true)
    {
        if (_layeredAudioSource == null) return false;
        return _layeredAudioSource.Play(clipPool, bank, layer, looping);
    }
    public void StopAudio(int layer)
    {
        if (_layeredAudioSource != null) 
            _layeredAudioSource.Stop(layer);
    }

    public void MuteAudio(bool mute)
    {
        if (_layeredAudioSource != null) 
            _layeredAudioSource.Mute(mute);
    }

    // ------------------------------------------ Methods ------------------------------------------ //

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name : Awake
    // Desc : Cache all needed components. 
    // ----------------------------------------------------------------------
    protected virtual void Awake()
    {
        //Get and store all frequently acessed components.
        _transform  = transform;
        _animator   = GetComponent<Animator>();
        _navAgent   = GetComponent<NavMeshAgent>();
        _collider   = GetComponent<Collider>();

        //Cache the audio source reference for the layered AI audio.
        AudioSource audioSource = GetComponent<AudioSource>();

        //Get bodyPart correct layer
        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        //Check if has any valid GameSceneManager instance on the scene.
        if (GameSceneManager.Instance != null)
        {
            //Register State Machines with Scene Database.
            if (_collider)      GameSceneManager.Instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) GameSceneManager.Instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }

        if (_rootBone != null)
        {
            Rigidbody[] bodies = _rootBone.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody bodyPart in bodies)
            {
                if (bodyPart != null && bodyPart.gameObject.layer == _aiBodyPartLayer)
                {
                    bodyPart.isKinematic = true;

                    _bodyParts.Add(bodyPart);
                    GameSceneManager.Instance.RegisterAIStateMachine(bodyPart.GetInstanceID(), this);
                }
            }
        }

        // Register the Layered Audio Source
        if (_animator && audioSource && AudioManager.Instance) 
            _layeredAudioSource = AudioManager.Instance.RegisterLayeredAudioSource(audioSource, _animator.layerCount);      
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
            foreach (AIStateMachineLink link in _scripts)
            {
                link.stateMachine = this;
                link.instanceAudioManager = GetComponent<ZombieInstanceAudioManager>();
            }
        }
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

        if (_target.type != AITargetType.None) _target.distance = Vector3.Distance(transform.position, _target.position);

        _isTargetReached = false;
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
    #endregion

    #region - Target Management -

    // ----------------------------------------------------------------------
    // Name: SetTarget
    // Desc: This method setup an new target to the actual AI State Machine
    //       instance.
    // ----------------------------------------------------------------------
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

    // ----------------------------------------------------------------------
    // Name: SetTarget (Method Overcharge)
    // Desc: This method setup an new target but manually set the target
    //       aspects as collider, position and distance.
    // ----------------------------------------------------------------------
    public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPosition, float targetDistance)
    {
        _target.Set(targetType, targetCollider, targetPosition, targetDistance);

        if (_targetTrigger != null)
        {
            _targetTrigger.radius               = _stoppingDistance;
            _targetTrigger.transform.position   = _target.position;
            _targetTrigger.enabled              = true;
        }
    }

    // ----------------------------------------------------------------------
    // Name: SetTarget (Method Overcharge)
    // Desc: This method overcharges an other method, but modifying the
    //       stopping distance, seting the stopping distance passed as
    //       argument.
    // ----------------------------------------------------------------------
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

    // ----------------------------------------------------------------------
    // Name : ClearTarget
    // Desc : Literaly clears the current target.
    // ----------------------------------------------------------------------
    public void ClearTarget()
    {
        _target.Clear();
        if (_targetTrigger != null) _targetTrigger.enabled = false;
    }
    #endregion

    private void NextWaypoint()
    {
        if (_randomWaypointPatrol && _waypointNetwork.waypoints.Count > 1)
        {
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint) _currentWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count);
        }
        else _currentWaypoint = _currentWaypoint == _waypointNetwork.waypoints.Count - 1 ? 0 : _currentWaypoint + 1;
    }

    // ----------------------------------------------------------------------
    // Name : GetWaypointPosition
    // Desc : Fetched the world space position of the state machine's
    //        currently set waypoint with optional increment.
    // ----------------------------------------------------------------------
    public Vector3 GetWaypointPosition(bool increment)
    {
        switch (_patrolType)
        {
            case PatrolType.Waypoint:

                if (_currentWaypoint == -1)
                {
                    if (_randomWaypointPatrol) _currentWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count);
                    else _currentWaypoint = 0;
                }
                else if (increment) NextWaypoint();

                if (_waypointNetwork.waypoints[_currentWaypoint] != null)
                {
                    Transform newWaypoint = _waypointNetwork.waypoints[_currentWaypoint];

                    SetTarget(AITargetType.Waypoint,
                        null, newWaypoint.position, Vector3.Distance(newWaypoint.position, transform.position));
                    return newWaypoint.position;
                }
                break;

            case PatrolType.RandomPatrol:
                Vector3 point;
                while (true) if (GetRandomPoint(transform.position, out point)) return point;
        }
        return Vector3.zero;
    }

    bool GetRandomPoint(Vector3 center, out Vector3 point)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * _patrolRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }

    public void SetStateOverride(AIStateType state)
    {
        if (state != _currentStateType && _states.ContainsKey(state))
        {
            if (_currentState != null) 
                _currentState.OnExitState();

            _currentState       = _states[state];
            _currentStateType   = state;
            _currentState.OnEnterState();
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

        _isTargetReached = true;

        if (_currentState) _currentState.OnDestinationReached(true);
    }
    protected virtual void OnTriggerStay(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;
        _isTargetReached = true;
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other) return;

        _isTargetReached = false;

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
        if (_animator == null) return;

        if (_useIK)
        {
            if (_currentState != null) _currentState.OnAnimatorIKUpdated();

            if (_footIK)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

                _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

                RaycastHit hit;
                Ray ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
                if (Physics.Raycast(ray, out hit, _distanceToGround + 1f, _groundMask))
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += _distanceToGround;
                    _animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    _animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }

                ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
                if (Physics.Raycast(ray, out hit, _distanceToGround + 1f, _groundMask))
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += _distanceToGround;
                    _animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    _animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
            //Foot IK
        }
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

    public virtual void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection)
    {
        
    }

    protected virtual void OnDestroy()
    {
        if (_layeredAudioSource != null && AudioManager.Instance) 
            AudioManager.Instance.UnregisterLayeredAudioSource(_layeredAudioSource);
    }
}