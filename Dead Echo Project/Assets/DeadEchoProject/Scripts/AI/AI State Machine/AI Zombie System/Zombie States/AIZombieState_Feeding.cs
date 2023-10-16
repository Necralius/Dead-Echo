using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding : AIZombieState
{
    //Inspector Assigned
    [SerializeField] float _slerpSpeed = 5f;
    [SerializeField] Transform _bloodParticleMount = null;
    [SerializeField, Range(0.01f, 1f)] float _bloodParticleFrequency = 0.1f;
    [SerializeField, Range(1, 100)] int _bloodParticleAmount = 10; 

    //Private
    private int _eatingStateHash = Animator.StringToHash("Eating_Idle");
    private int _crawlEatingStateHash = Animator.StringToHash("Crawl Feeding State");

    private int _eatingLayerIndex = -1;
    private float _timer = 0f;
    
    public override AIStateType GetStateType() => AIStateType.Feeding;

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_zombieStateMachine == null) return;

        if (_eatingLayerIndex == -1) _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("CinematicLayer");

        _timer = 0f;

        _zombieStateMachine.feeding = true;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.attackType = 0;

        _zombieStateMachine.NavAgentControl(true, false);
    }
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;

        if (_zombieStateMachine.satisfaction > 0.9f)
        {
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.VisualThreat.type != AITargetType.None && _zombieStateMachine.VisualThreat.type != AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        int currentHash = _zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash;

        if (currentHash == _eatingStateHash || currentHash == _crawlEatingStateHash)
        {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + 
                ((Time.deltaTime * _zombieStateMachine.replenishRate) / 100), 1.0f);

            if (GameSceneManager.Instance && GameSceneManager.Instance.bloodParticles && _bloodParticleMount)
            {
                if (_timer > _bloodParticleFrequency)
                {
                    ParticleSystem blood = GameSceneManager.Instance.bloodParticles;

                    blood.transform.position = _bloodParticleMount.position;
                    blood.transform.rotation = _bloodParticleMount.rotation;

                    var main = blood.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;

                    blood.Emit(_bloodParticleAmount);
                    _timer = 0;
                }
            }
        }

        if (!_zombieStateMachine.useRootRotation)
        {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }
        
        Vector3 headToTarget = _zombieStateMachine.targetPosition - _zombieStateMachine.animator.GetBoneTransform(HumanBodyBones.Head).position;
        _zombieStateMachine.transform.position = Vector3.Lerp(_zombieStateMachine.transform.position, 
                                                              _zombieStateMachine.transform.position + headToTarget, Time.deltaTime);

        //Stay in feeding state
        return AIStateType.Feeding;
    }
    public override void OnExitState()
    {
        if (_zombieStateMachine != null) _zombieStateMachine.feeding = false;
    }
}