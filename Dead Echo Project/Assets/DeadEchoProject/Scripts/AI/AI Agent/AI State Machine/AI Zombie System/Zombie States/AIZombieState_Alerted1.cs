using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState
{

    [SerializeField, Range(1f,60f)] float _maxDuration = 10f;
    [SerializeField] float _waypointAngleThreshold = 90f;
    [SerializeField] float _threatAngleThreshold = 10f;

    //Private
    float _timer = 0f;


    // ----------------------------------------------------------------------
    // Name : GetStateType
    // Desc : Returns the type of the state
    // ----------------------------------------------------------------------
    public override AIStateType GetStateType() => AIStateType.Alerted;

    public override void OnEnterState()
    {
        Debug.Log("Entering Alerted!");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _timer = _maxDuration;
    }

    // ----------------------------------------------------------------------
    // Name : OnUpdate 
    // Desc : The engine of this state
    // ----------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Reduce Timer
        _timer -= Time.deltaTime;

        // Transition into a patrol state if available
        if (_timer <= 0.0f) return AIStateType.Patrol;

        // Do we have a visual threat that is the player. These take priority over audio threats
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
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
            _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
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

            if (Random.value < _zombieStateMachine.intelligence) _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
            else _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1f, 1f));
        }
        else
        if (_zombieStateMachine.targetType == AITargetType.Waypoint)
        {
            angle = FindSignedAngle(_zombieStateMachine.transform.forward,
                                            _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;
            _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
        }

        return AIStateType.Alerted;
    }
}