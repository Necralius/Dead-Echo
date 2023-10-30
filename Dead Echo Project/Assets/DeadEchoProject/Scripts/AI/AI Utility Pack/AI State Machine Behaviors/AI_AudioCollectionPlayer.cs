using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_AudioCollectionPlayer : AIStateMachineLink
{
    //Inspector Assigned
    [Header("Main Settings")]
    [SerializeField] private ComChannelName     _commandChannel     = ComChannelName.ComChannel1;
    [SerializeField] private AudioCollection    _collection         = null;
    [SerializeField] private bool               isAnStepBehavior    = false;
    [SerializeField] private CustomCurve        _customCurve        = null;
    [SerializeField] private StringList         _layersExclusions   = null;

    //Private 
    int             _previusCommand     = 0;
    AudioManager    _audioManager       = null;
    int             _commandChannelHash = 0;

    // ----------------------------------------------------------------------
    // Name: OnStateEnter
    // Desc: This method is called on the first frame that the animation has
    //       been played in the current state.
    // ----------------------------------------------------------------------
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _audioManager       = AudioManager.Instance;
        _previusCommand     = 0;


        if (_commandChannelHash == 0) _commandChannelHash = Animator.StringToHash(_commandChannel.ToString());
    }

    // ----------------------------------------------------------------------
    // Name: OnStateUpdate
    // Desc: This method is called by the animation system on each frame that
    //       the animation has been updated.
    // ----------------------------------------------------------------------
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Prevent that sounds been played if the layer is not active or if the state machine instance is invalid.
        if (layerIndex != 0 && animator.GetLayerWeight(layerIndex).Equals(0f)) return;
        if (_stateMachine == null) return;

        if (_layersExclusions != null) 
            for (int i = 0; i < _layersExclusions.Count; i++) 
                if (_stateMachine.IsLayerActive(_layersExclusions[i])) return;

        int customCommand = (_customCurve == null) ? 0 : Mathf.FloorToInt(_customCurve.Evaluate(stateInfo.normalizedTime - (long)stateInfo.normalizedTime));

        int command;

        if (customCommand != 0)     command = customCommand;
        else                        command = Mathf.FloorToInt(animator.GetFloat(_commandChannelHash));

        if (isAnStepBehavior)
        {
            if (_previusCommand != command && command > 0 && _audioManager != null && _stateMachine != null) 
                _instanceAudioManager.OnStepBehavior(_stateMachine.transform.position);
        }
        else
        {
            if (_previusCommand != command  && command > 0 && _audioManager != null && _collection != null && _stateMachine != null)
            {
                int bank = Mathf.Max(0, Mathf.Min(command - 1, _collection.bankCount - 1));
                _audioManager.PlayOneShotSound(_collection[bank], _stateMachine.transform.position, _collection);
            }
        }
        _previusCommand = command;
    }
}