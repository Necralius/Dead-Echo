using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerEnabler : AIStateMachineLink
{
    public bool OnEnter     = false;
    public bool OnExit      = false;

    // ----------------------------------------------------------------------
    // Name: OnStateEnter
    // Desc: This method is called on the first frame of the animation
    //       assigned to this state.
    // ----------------------------------------------------------------------
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
            _stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnEnter);
    }

    // ----------------------------------------------------------------------
    // Name: OnStateExit
    // Desc: This method is called on the last frame of the animation
    //       assigned to this state.
    // ----------------------------------------------------------------------
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine) 
            _stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnExit);
    }
}