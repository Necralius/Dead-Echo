using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIZombieState_Alerted1 : AIZombieState
{

    [SerializeField, Range(1f,60f)] float   _maxDuration                = 10f;
    [SerializeField] float                  _waypointAngleThreshold     = 90f;
    [SerializeField] float                  _threatAngleThreshold       = 10f;
    [SerializeField] float                  _directionChangeTime        = 1.5f;
    [SerializeField] float                  _slerpSpeed                 = 45f;

    //Private
    float _timer                    = 0f;
    float _directionChangeTimer     = 0f;
    float _screamChance             = 0f;

    // ----------------------------------------------------------------------
    // Name : GetStateType
    // Desc : Returns the type of the state
    // ----------------------------------------------------------------------
    public override AIStateType GetStateType() => AIStateType.Alerted;

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _timer = _maxDuration;
        _directionChangeTimer = 0f;
        _screamChance = _zombieStateMachine.screamChance - Random.value;
    }

    // ----------------------------------------------------------------------
    // Name : OnUpdate 
    // Desc : The engine of this state
    // ----------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Reduce Timer
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        // Transition into a patrol state if available
        if (_timer <= 0.0f)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.isStopped = false;
            _timer = _maxDuration;
        }

        // Do we have a visual threat that is the player. These take priority over audio threats
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            if (_screamChance > 0f)
            {
                if (_zombieStateMachine.Scream())
                {
                    _screamChance = float.MinValue;
                    return AIStateType.Alerted;
                }
            }
            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.None &&
            _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food &&
            _zombieStateMachine.targetType == AITargetType.None)
        {
            _zombieStateMachine.SetTarget(_stateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        float angle;

        if ((_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.Visual_Light) && !_zombieStateMachine.isTargetReached)
        {
            angle = FindSignedAngle(_zombieStateMachine.transform.forward,
                                            _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);       

            if (_zombieStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < _threatAngleThreshold) return AIStateType.Pursuit;

            if (_directionChangeTimer > _directionChangeTime)
            {
                if (Random.value < _zombieStateMachine.intelligence) _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                else _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1f, 1f));
                _directionChangeTimer = 0f;
            }
        }
        else if (_zombieStateMachine.targetType == AITargetType.Waypoint && !_zombieStateMachine.navAgent.pathPending)
        {
            angle = FindSignedAngle(_zombieStateMachine.transform.forward,
                                            _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;

            if (_directionChangeTimer > _directionChangeTime)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0;
            }
        }
        else
        {
            if (_directionChangeTimer > _directionChangeTime)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0;
            }
        }

        if (!_zombieStateMachine.useRootRotation) _zombieStateMachine.transform.Rotate(new Vector3(0f, _slerpSpeed * _zombieStateMachine.seeking * Time.deltaTime, 0f));

        return AIStateType.Alerted;
    }
}