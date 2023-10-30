using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComChannelName { ComChannel1, ComChannel2, ComChannel3, ComChannel4 }

// --------------------------------------------------------------------------
// Name: AIStateMachineLink (Class - StateMachineBehavior)
// Desc: This class is used as a base class for any StateMachineBehavior that
//       needs to communicate with its AI State Machine.
// --------------------------------------------------------------------------
public class AIStateMachineLink : StateMachineBehaviour
{
    protected AiStateMachine                _stateMachine;
    protected ZombieInstanceAudioManager    _instanceAudioManager;

    public AiStateMachine               stateMachine            { set => _stateMachine          = value; }
    public ZombieInstanceAudioManager   instanceAudioManager    { set => _instanceAudioManager  = value; }
}