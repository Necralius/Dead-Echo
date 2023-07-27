using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    public void SetStateMachine(AiStateMachine stateMachine) { _stateMachine = stateMachine; }
    
    //Default AI State Behavior
    public virtual void OnEnterState()          { }
    public virtual void OnExitState()           { }
    public virtual void OnAnimatorUpdated()
    {
        if (_stateMachine.useRootPosition) _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime;
        if (_stateMachine.useRootRotation) _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;
    }
    public virtual void OnAnimatorIKUpdated()   { }
    public virtual void OnTriggerEvent( AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached( bool isReached) { }


    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    protected AiStateMachine _stateMachine;
}