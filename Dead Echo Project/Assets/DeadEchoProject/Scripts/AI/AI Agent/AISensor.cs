using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour
{
    private AiStateMachine _parentStateMachine = null;
    public AiStateMachine parentStateMachine { set { _parentStateMachine = value; } }

    private void OnTriggerEnter(Collider col)
    {
        if (_parentStateMachine != null) _parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, col);
    }
    private void OnTriggerStay(Collider col)
    {
        if (_parentStateMachine != null) _parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, col);
    }

    private void OnTriggerExit(Collider col)
    {
        if (_parentStateMachine != null) _parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, col);
    }

}