using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class StateMachineAudioManager : MonoBehaviour
{
    protected AiStateMachine        _machineInstance    = null;
    protected AudioManager          _audioManager       = null;

    [SerializeField] private bool   _humanoid           = true;
    [SerializeField] private bool   _stepBehavior       = true;

    [SerializeField] private    List<AudioCollection>  _collectionBases    = new List<AudioCollection>();
    [SerializeField] protected  AudioCollection _onRagdoll;
    
    [SerializeField] private FloorData              _floorData          = null;

    // ----------------------------------------------------------------------
    // Name: Start (Method)
    // Desc: This method is called on the scene start, mainly this method is
    //       getting all the class dependencies.
    // ----------------------------------------------------------------------
    protected virtual void Start()
    {
        _machineInstance    = GetComponent<AiStateMachine>();
        _audioManager       = AudioManager.Instance;
        _floorData          = new FloorData(_machineInstance.transform.gameObject, _machineInstance._collider);
    }

    protected virtual void Update()
    {
        
    }

    // ----------------------------------------------------------------------
    // Name: OnStepBehavior (Method)
    // Desc: This method uses the FloorData instance to detect the ground
    //       type that the GameObject is on top, and plays an equivalent
    //       audio clip on the passed position, this way representing an
    //       footstep behavior on an IA agent.
    // ----------------------------------------------------------------------
    public virtual void OnStepBehavior(Vector3 feetPos)
    {
        if (!_humanoid)     return;
        if (!_stepBehavior) return;

        AudioClip           clipToPlay          = null;
        AudioCollection     selectedCollection  = null;

        string currentFloor = _floorData.Type.ToString();

        //Try to find the correct audio collection, using as base the current action passed as argument.
        foreach (var collection in _collectionBases)
        {
            if (currentFloor == collection.floorTag)
            {
                selectedCollection = collection;
                break;
            }
        }
            
        if (selectedCollection == null)
        {
            Debug.Log("Null Collection! -> Failed Footstep");
            return;
        }

        clipToPlay = selectedCollection.audioClip;

        //Returns the method if the collection or the audio manager are invalid objects.
        if (_audioManager   == null) return;
        if (clipToPlay      == null) return;

        //Play the clip
        _audioManager.PlayOneShotSound(clipToPlay, feetPos, selectedCollection);
    }
}