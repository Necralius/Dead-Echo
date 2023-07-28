using UnityEngine;
using Random = UnityEngine.Random;

public class AIZombieState_Patrol1 : AIZombieState
{
    [SerializeField] WaypointNetwork _waypointNetwork = null;
    [SerializeField] bool _randomPatrol = false;
    [SerializeField] int _currentWaypoint = 0;
    [SerializeField] float _turnOnSpotThershold = 80f;
    [SerializeField] float _slerpRotationSpeed = 5f;

    [SerializeField, Range(0, 3)] float _speed = 1f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State!");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        if (_zombieStateMachine.targetType != AITargetType.Waypoint)
        {
            _zombieStateMachine.ClearTarget();
            if (_waypointNetwork != null && _waypointNetwork.waypoints.Count > 0)
            {
                if (_randomPatrol) _currentWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count);

                if (_currentWaypoint < _waypointNetwork.waypoints.Count)
                {
                    Transform waypoint = _waypointNetwork.waypoints[_currentWaypoint];
                    if (waypoint != null)
                    {
                        _zombieStateMachine.SetTarget(AITargetType.Waypoint,
                            null, waypoint.position, Vector3.Distance(_zombieStateMachine.transform.position, waypoint.position));
                        
                        _zombieStateMachine.navAgent.SetDestination(waypoint.position);
                    }
                }
            }
        }
        _zombieStateMachine.navAgent.isStopped = false;
    }
    public override AIStateType OnUpdate()
    {
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            if ((1f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);
        
        if (Mathf.Abs(angle) > _turnOnSpotThershold)
        {

            return AIStateType.Alerted;
        }
        
        if (!_zombieStateMachine.useRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpRotationSpeed);
        }

        if (_zombieStateMachine.navAgent.isPathStale || 
            !_zombieStateMachine.navAgent.hasPath || 
            _zombieStateMachine.navAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            NextWaypoint();
        }

        return AIStateType.Patrol;
    }
    private void NextWaypoint()
    {
        if (_randomPatrol && _waypointNetwork.waypoints.Count > 1)
        {
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint) _currentWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count);
        }
        else _currentWaypoint = _currentWaypoint == _waypointNetwork.waypoints.Count - 1 ? 0 : _currentWaypoint + 1;

        if (_waypointNetwork.waypoints[_currentWaypoint] != null)
        {
            Transform newWaypoint = _waypointNetwork.waypoints[_currentWaypoint];

            _zombieStateMachine.SetTarget(AITargetType.Waypoint, 
                null, newWaypoint.position, Vector3.Distance(newWaypoint.position, _zombieStateMachine.transform.position));
            
            _zombieStateMachine.navAgent.SetDestination(newWaypoint.position);
        }
    }
    public override void OnDestinationReached(bool isReached)
    {
        if (_zombieStateMachine == null || !isReached) return;

        if (_zombieStateMachine.targetType == AITargetType.Waypoint) NextWaypoint();
    }

    // ----------------------------------------------------------------------
    // Name : OnAnimatorIKUpdated
    // Desc : Overrid IK Goals
    // ----------------------------------------------------------------------
    public override void OnAnimatorIKUpdated()
    {
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);
        _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    }

}