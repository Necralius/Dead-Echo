using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState : AIState
{
    //Private
    protected int _playerLayerMask = -1;
    protected int _bodyPartLayer = -1;

    private void Awake()
    {
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        _bodyPartLayer = LayerMask.GetMask("AI Body Part");
    }

    public override AIStateType GetStateType()
    {
        throw new System.NotImplementedException();
    }

    public override AIStateType OnUpdate()
    {
        throw new System.NotImplementedException();
    }
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (_stateMachine == null) return;
        
        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType curType = _stateMachine.VisualThreat.type;

            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(_stateMachine.sensorPosition, other.transform.position);
                if (curType != AITargetType.Visual_Player || (curType == AITargetType.Visual_Player && distance < _stateMachine.VisualThreat.distance))
                {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, _playerLayerMask))
                    {
                        _stateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            }
        }
    }
    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();
        if (_stateMachine == null || _stateMachine.GetType() != typeof(AIZombieStateMachine)) return false;
        AIZombieStateMachine zombieMachine = (AIZombieStateMachine)_stateMachine;

        Vector3 head = _stateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > zombieMachine.fov * 0.5f) return false;

        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, _stateMachine.sensorRadius * zombieMachine.sight, layerMask);

        float closesColliderDistance = float.MaxValue;
        Collider closestCollider = null;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.distance < closesColliderDistance)
            {
                if (hit.transform.gameObject.layer == _bodyPartLayer)
                {
                    if (_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        closesColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
                else
                {
                    closesColliderDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }

        if (closestCollider && closestCollider.gameObject == other.gameObject) return true;

        return false;
    }
}