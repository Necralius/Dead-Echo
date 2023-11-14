using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieInstanceAudioManager : StateMachineAudioManager
{
    public void OnRagdoll(Vector3 hitPos)
    {
        AudioManager.Instance.PlayOneShotSound(_onRagdoll.audioClip, hitPos, _onRagdoll);  
    }
}