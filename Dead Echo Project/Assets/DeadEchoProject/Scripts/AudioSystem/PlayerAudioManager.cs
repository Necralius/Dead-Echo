using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class PlayerAudioManager : MonoBehaviour
{
    //Inspector assinged
    [Header("Speed Modifier")]
    [SerializeField] float  _speedModifier      = 2f;
    [SerializeField] float  _distacePerStep     = 4f; 
    private          float  _playerSpeed        = 0f;

    //Private Data
    private float           _distanceCovered    = 0f;

    [SerializeField] private float _airTime     = 0f;

    //Dependencies
    [Header("Dependencies")]
    [SerializeField] private    FloorData                   _floorData                  = null;
    [SerializeField] private    Transform                   _footstepArea               = null;
    [SerializeField] private    List<AudioCollection>       _footstepCollection         = new List<AudioCollection>();
    private                     ControllerManager           _playerController           = null;
    private                     AudioManager                _audioManager               = null;

    // ------------------------------------------ Methods ------------------------------------------ //

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name: Start (Method)
    // Desc: This method is called on the game start, mainly the method get
    //       all the class depedencies, and neclare the class floor data.
    // ----------------------------------------------------------------------
    private void Start()
    {
        _playerController   = GetComponent<ControllerManager>();
        _audioManager       = AudioManager.Instance;
        _floorData          = new FloorData(_playerController.gameObject, _playerController._playerCol);
    }

    // ----------------------------------------------------------------------
    // Name: Update (Method)
    // Desc: This method is called on every unity frame update.
    // ----------------------------------------------------------------------
    private void Update()
    {
        //Get the current player speed
        _playerSpeed = _playerController._rb.velocity.magnitude;

        //Verify if the player need an footstep audio, using as base the distance that the player has walked.
        if (_playerController._isGrounded && _playerController._isWalking)
        {
            _distanceCovered += (_playerSpeed * Time.deltaTime) * _speedModifier;
            if (_distanceCovered > _distacePerStep)
            {
                _distanceCovered = 0f;
                AudioBehavior(_floorData.Type, ActionType.Footstep);
            }          
        }

        //Detects if the player is in the air.
        if (_playerController._inAir) _airTime += Time.deltaTime;
        else if (_airTime >= 0.10f && _playerController._isGrounded)
        {
            //If the player touch the ground and have an air time greater than 0.10 seconds, the land sound is played.
            _airTime = 0f;
            AudioBehavior(_floorData.Type, ActionType.Land);
            //Debug.Log("PAM: Landed 1."); -> Debug Line
        }
    }
    #endregion

    #region - Audio Management -
    // ----------------------------------------------------------------------
    // Name: AudioBehavior (Method)
    // Desc: This method call an audio using as base the ground type, and the
    //       action type, basically, the floor data verify what ground the
    //       player is stepping, and the action type, represents the action
    //       that will be shoted (Exp: Jump, Land, Footstep).
    // ----------------------------------------------------------------------
    private void AudioBehavior(FloorData.FloorType groundType, ActionType actionType)
    {        
        AudioClip       clipToPlay          = null;
        AudioCollection selectedCollection  = null;

        //Try to find the correct audio collection, using as base the current action passed as argument.
        foreach (var collection in _footstepCollection)
        {
            //Debug.Log($"PAM -> Floor Finded: {groundType}, Collection Verified: {collection.floorTag}"); -> Debug Line
            if (groundType.ToString() ==  collection.floorTag)
            {
                selectedCollection = collection;
                //Debug.Log("PAM -> Finded the correct floor collection!"); -> Debug Line
                switch (actionType)
                {
                    case ActionType.Footstep    : clipToPlay = collection[0];   break;
                    case ActionType.Jump        : clipToPlay = collection[1];   break;
                    case ActionType.Land        : clipToPlay = collection[2];   break;
                    default: break;
                }
            }
        }

        //Returns the method if the collection or the audio manager are invalid objects.
        if (_audioManager       == null) return;
        if (clipToPlay          == null) return;

        //Plays the clip
        _audioManager.PlayOneShotSound(clipToPlay, _footstepArea.position, selectedCollection);
    }

    // ----------------------------------------------------------------------
    // Name: JumpAudioAction (Method)
    // Desc: This method plays the jump action, the method is public, so is
    //       called in the player jump action.
    // ----------------------------------------------------------------------
    public void JumpAudioAction()
    {
        //Debug.Log("PAM: Started Jump."); -> Debug Line
        AudioBehavior(_floorData.Type, ActionType.Jump);
    }
    #endregion
}
//Enumeration that is used to identify an audio action.
public enum ActionType { Footstep, Land, Jump }