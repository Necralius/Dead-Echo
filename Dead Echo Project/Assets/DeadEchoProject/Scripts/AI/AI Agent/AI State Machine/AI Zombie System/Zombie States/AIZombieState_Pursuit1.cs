using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Pursuit1 : AIZombieState
{
    [SerializeField, Range(0, 10)] private float _speed = 1f;
    [SerializeField] private float _slerpSeepd = 5f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5f;

    [SerializeField] private float _repathAudioMinDuruation = 0.25f;
    [SerializeField] private float _repathAudioMaxDuration = 5f;

    [SerializeField] private float _maxDuration = 40f;
    //Private
    private float _timer = 0;
    private float _repathTimer = 0;

    public override AIStateType GetStateType() => AIStateType.Pursuit;
    public override void OnEnterState()
    {
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);       
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _timer = 0f;
        _repathTimer = 0f;

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.isStopped = false;
    }
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration) return AIStateType.Patrol;

        if (_stateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.inMeleeRange) return AIStateType.Attack;

        if (_zombieStateMachine.isTargetReached)
        {
            switch(_stateMachine.targetType)
            {
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    _stateMachine.ClearTarget();
                    return AIStateType.Alerted;
                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }

        if (_zombieStateMachine.navAgent.isPathStale ||
            (!_zombieStateMachine.navAgent.hasPath && 
            !_zombieStateMachine.navAgent.pathPending) ||
            _zombieStateMachine.navAgent.pathStatus != 
            NavMeshPathStatus.PathComplete) return AIStateType.Alerted;

        if (_zombieStateMachine.navAgent.pathPending) _zombieStateMachine.speed = 0;
        else
        {
            _zombieStateMachine.speed = _speed;

            if (!_zombieStateMachine.useRootRotation &&
                _zombieStateMachine.targetType == AITargetType.Visual_Player &&
                _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player &&
                _zombieStateMachine.isTargetReached)
            {
                Vector3 targetPos = _zombieStateMachine.targetPosition;
                targetPos.y = _zombieStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
                _zombieStateMachine.transform.rotation = newRot;
            }
            else if (!_stateMachine.useRootRotation && !_zombieStateMachine.isTargetReached)
            {
                Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);

                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSeepd);
            }
            else if (_zombieStateMachine.isTargetReached) return AIStateType.Alerted;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
            {
                if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                {
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                    _repathTimer = 0f;
                }
            }
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.targetType == AITargetType.Visual_Player) return AIStateType.Pursuit;

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            if (_zombieStateMachine.targetType == AITargetType.Audio ||
                _zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Alerted;
            }
            else if (_zombieStateMachine.targetType == AITargetType.Visual_Light)
            {
                int currentID = _zombieStateMachine.targetColliderID;

                if (currentID == _zombieStateMachine.VisualThreat.collider.GetInstanceID())
                {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
                    {
                        if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                        {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                            _repathTimer = 0f;
                        }
                    }
                     _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                     return AIStateType.Pursuit;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        else if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            int currentID = _zombieStateMachine.targetColliderID;

            if (currentID == _zombieStateMachine.AudioThreat.collider.GetInstanceID())
            {
                if (_zombieStateMachine.targetPosition != _zombieStateMachine.AudioThreat.position)
                {
                    if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuruation, _repathAudioMaxDuration) < _repathTimer)
                    {
                        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.AudioThreat.position);
                        _repathTimer = 0f;
                    }
                }
                _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                return AIStateType.Pursuit;
            }
            else
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                return AIStateType.Alerted;
            }
        }

        return AIStateType.Pursuit;
    }
}