using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateMachine : AiStateMachine
{
    //This class activates the inspector for default AiStateMachine Data

    [SerializeField, Range(10f, 360f)] float _fov = 50f; 
    [SerializeField, Range(0f, 1f)] float _sight = 0.5f; 
    [SerializeField, Range(0f, 1f)] float _hearing = 10f; 
    [SerializeField, Range(0f, 1f)] float _agression= 10f; 
    [SerializeField, Range(0, 100)] int _health = 100; 
    [SerializeField, Range(0f, 1f)] float _intelligence = 0.5f; 
    [SerializeField, Range(0f, 1f)] float _satisfaction = 1f;
    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;

    //Private
    private int     _seeking    = 0;
    private bool    _feeding    = false;
    private bool    _crawling   = false;
    private int     _attackType = 0;
    private float _speed = 0.0f;


    //Animation parameters hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _feedingHash = Animator.StringToHash("Feeding");
    private int _attackHash = Animator.StringToHash("Attack");

    //Public
    public float    replenishRate   { get => _replenishRate;    }
    public float    fov             { get => _fov;              }
    public float    hearing         { get => _hearing;          }
    public float    sight           { get => _sight;            }
    public float    intelligence    { get => _intelligence;     }
    public bool     crawling        { get => _crawling;         }
    public float    satisfaction    { get => _satisfaction;     set => _satisfaction    =  value; }
    public float    aggression      { get => _agression;        set => _agression       =  value; }
    public int      health          { get => _health;           set => _health          =  value; }
    public int      attackType      { get => _attackType;       set => _attackType      =  value; }
    public bool     feeding         { get => _feeding;          set => _feeding         =  value; }
    public int      seeking         { get => _seeking;          set => _seeking         =  value; }
    public float    speed           { get => _speed;            set => _speed           =  value; }

    protected override void Update()
    {
        base.Update();

        if (navAgent != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
        }
    }
}