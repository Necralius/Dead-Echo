using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string     _paramenter                 = "";
    [SerializeField] int        _bloodParticlesBurstAmount  = 5;
    [SerializeField] float      _damageAmount               = 0.1f;


    //Private
    AiStateMachine      _stateMachine       = null;
    Animator            _animator           = null;
    int                 _parameterHash      = -1;
    GameSceneManager    _gameSceneManager   = null;

    private void Start()
    {
        _stateMachine       = GetComponentInParent<AiStateMachine>();
        _parameterHash      = Animator.StringToHash( _paramenter );
        _gameSceneManager   = GameSceneManager.instance;
        if (_stateMachine != null) _animator = _stateMachine.animator;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_animator           == null) return;
        if (_gameSceneManager   == null) return;

        if (other.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            if (_gameSceneManager.bloodParticles)
            {
                ParticleSystem system = _gameSceneManager.bloodParticles;

                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                var main = system.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                system.Emit(_bloodParticlesBurstAmount);
            }

            if (_gameSceneManager != null)
            {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo(other.GetInstanceID());
                if (info != null && info.characterManager != null) 
                    info.characterManager.TakeDamage(_damageAmount);
            }
        }
    }
}