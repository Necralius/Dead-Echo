using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NekraByte.FPS_Utility.Core.DataTypes;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Speed Modifier")]
    [SerializeField] float  _speedModifier   = 2f;
    [SerializeField] float  _playerSpeed     = 0f;

    private float           _distanceCovered = 0f;
    private bool            _landed = false;

    [Header("Dependencies")]
    //Dependencies
    private                     ControllerManager           _playerController           = null;
    [SerializeField] private    FloorData                   _floorData                  = null;
    [SerializeField] private    Transform                   _footstepArea               = null;
    [SerializeField] private    List<FootstepCollection>    _footstepCollection         = new List<FootstepCollection>();

    private                     AudioManager                _audioManager               = null;

    private void Start()
    {
        _playerController   = GetComponent<ControllerManager>();
        _audioManager       = AudioManager.Instance;
        _floorData          = new FloorData(_playerController.gameObject, _playerController._playerCol);
    }

    //
    //
    //
    //
    private void Update()
    {
        _playerSpeed = _playerController._rb.velocity.magnitude;

        if (_playerController._isWalking)
        {
            _distanceCovered += (_playerSpeed * Time.deltaTime) * _speedModifier;
            if (_distanceCovered > 1) 
                FootstepBehavior(_floorData.Type);
        }

        if (_playerController._isGrounded && !_landed)
        {
            PlayLandSound(_floorData.Type);
            _landed = true;
        }
    }

    private void FootstepBehavior(FloorData.FloorType groundType)
    {
        AudioCollection collection = _footstepCollection.Find(e => e.collectionTag == groundType.ToString()).footstepCollection;

        //Get Clip
        AudioClip footstepClip = collection.audioClip;

        //Play Clip
        _audioManager.PlayOneShotSound("Effects", footstepClip, _footstepArea.position, 1f, 1f, 128);
    }

    public void PlayJumpSound(FloorData.FloorType groundType)
    {
        _landed = false;

        AudioCollection collection = _footstepCollection.Find(e => e.collectionTag == groundType.ToString()).footstepCollection;
        AudioClip jumpClip = collection.audioClip;

        _audioManager.PlayOneShotSound("Effects", jumpClip, _footstepArea.position, 1f, 1f, 128);
    }

    private void PlayLandSound(FloorData.FloorType groundType)
    {
        AudioCollection collection = _footstepCollection.Find(e => e.collectionTag == groundType.ToString()).footstepCollection;
        AudioClip landClip = collection.audioClip;

        _audioManager.PlayOneShotSound("Effects", landClip, _footstepArea.position, 1f, 1f, 128);
    }
}

[Serializable]
public class FootstepCollection
{
    public string collectionTag;
    public AudioCollection footstepCollection;
    public AudioCollection jumpCollection;
    public AudioCollection landCollection;
}