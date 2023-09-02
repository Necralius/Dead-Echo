using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachineLink : StateMachineBehaviour
{
    protected AiStateMachine _stateMachine;
    public AiStateMachine stateMachine { set => _stateMachine = value; }
}