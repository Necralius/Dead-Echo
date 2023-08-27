using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIBoneControlType { Animated, Ragdoll, RagdollToAnim }

// --------------------------------------------------------------------------
// Name: BodyPartSnapshot
// Desc: This method is used to store information about the positions of each
//       body part that is transitioning from a ragdoll state.
// --------------------------------------------------------------------------
public class BodyPartSnapshot
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion localRotation;
}

// --------------------------------------------------------------------------
// Name: AIZombieStateMachine
// Desc: State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AiStateMachine
{
    //This datas are active in the inspector for default AiStateMachine Data

    [SerializeField, Range(10f, 360f)]      float   _fov                    = 50f;
    [SerializeField, Range(0f, 1f)]         float   _sight                  = 0.5f;
    [SerializeField, Range(0f, 1f)]         float   _hearing                = 10f;
    [SerializeField, Range(0f, 1f)]         float   _agression              = 10f;
    [SerializeField, Range(0, 100)]         int     _health                 = 100;
    [SerializeField, Range(0, 100)]         int     _lowerBodyDamage        = 0;
    [SerializeField, Range(0, 100)]         int     _upperBodyDamage        = 0;
    [SerializeField, Range(0, 100)]         int     _upperBodyThreshold     = 30;
    [SerializeField, Range(0, 100)]         int     _limpThreshold          = 30;
    [SerializeField, Range(0, 100)]         int     _crawlThreshold         = 90;
    [SerializeField, Range(0f, 1f)]         float   _intelligence           = 0.5f;
    [SerializeField, Range(0f, 1f)]         float   _satisfaction           = 1f;
    [SerializeField, Range(0f, 1f)]         float   _replenishRate          = 0.5f;
    [SerializeField, Range(0f, 1f)]         float   _depletionRate          = 0.1f;
    [SerializeField]                        float   _reanimationBlendTime   = 0.5f;
    [SerializeField]                        float   _reanimationWaitTime    = 3f;

    //Private
    private int     _seeking        = 0;
    private bool    _feeding        = false;
    private bool    _crawling       = false;
    private int     _attackType     = 0;
    private float   _speed          = 0.0f;

    //Animation parameters hashes
    private int _speedHash      = Animator.StringToHash("Speed");
    private int _seekingHash    = Animator.StringToHash("Seeking");
    private int _feedingHash    = Animator.StringToHash("Feeding");
    private int _attackHash     = Animator.StringToHash("Attack");
    private int _crawlingHash   = Animator.StringToHash("Crawling");
    private int _hitTriggerHash = Animator.StringToHash("Hit");
    private int _hitTypeHash    = Animator.StringToHash("HitType");

    //Ragdoll System Data
    private AIBoneControlType           _boneControllType       = AIBoneControlType.Animated;
    private List<BodyPartSnapshot>      _bodyPartSnapshots      = new List<BodyPartSnapshot>();
    private float                       _ragdollEndTime         = float.MinValue;
    private Vector3                     _ragdollHipPosition;
    private Vector3                     _ragdollFeetPosition;
    private Vector3                     _ragdoolHeadPosition;
    private IEnumerator                 _reanimationCoroutine   = null;
    private float                       _mecanimTransitionTime  = 0.1f;

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
    public bool     isCrawling      { get => _upperBodyDamage >= _crawlThreshold; }

    // ----------------------------------------------------------------------
    // Name: Start
    // Desc: This method is called on the game start, the method is being
    //       overrided for another code parts implementation.
    // ----------------------------------------------------------------------
    protected override void Start()
    {
        base.Start();

        if (_rootBone != null)
        {
            Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();
            foreach(var trans in transforms)
            {
                BodyPartSnapshot snapshot = new BodyPartSnapshot();
                snapshot.transform = trans;
                _bodyPartSnapshots.Add(snapshot);
            }
        }

        UpdateAnimatorDamage();
    }

    // ----------------------------------------------------------------------
    // Name: Update
    // Desc: This method is called every frame, mainly the method update the
    //       animator values
    // ---------------------------------------------------------------------- 
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

        _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime)/ 100f) * Mathf.Pow(_speed, 3f));
    }
    protected void UpdateAnimatorDamage()
    {
        if (_animator != null) _animator.SetBool(_crawlingHash, isCrawling);
    }

    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection)
    {
        if (GameSceneManager.instance != null && GameSceneManager.instance.bloodParticles != null)
        {
            ParticleSystem blood = GameSceneManager.instance.bloodParticles;
            blood.transform.position = position;

            var main = blood.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            blood.Emit(60);
        }

        float hitStrenght = force.magnitude;

        if (_boneControllType == AIBoneControlType.Ragdoll)
        {
            if (bodyPart != null)
            {
                if (hitStrenght > 5f) bodyPart.AddForce(force, ForceMode.Impulse);

                if (bodyPart.CompareTag("Head")) _health = Mathf.Max(_health - damage, 0);
                else if (bodyPart.CompareTag("Upper Body")) _upperBodyDamage += damage;
                else if (bodyPart.CompareTag("Lower Body")) _lowerBodyDamage += damage;

                UpdateAnimatorDamage();

                if (_health > 0)
                {
                    if (_reanimationCoroutine != null) StopCoroutine(_reanimationCoroutine);

                    _reanimationCoroutine = Reanimate();
                    StartCoroutine(Reanimate());
                }
            }
            return;
        }

        //Get local space position of attacker
        Vector3 attackerLocPos = transform.InverseTransformPoint(characterManager.transform.position);

        //Local space position of hit
        Vector3 hitLocPos = transform.InverseTransformPoint(position);
       
        bool shouldRagdoll = (hitStrenght > 5f);

        if (bodyPart != null)
        {
            if (bodyPart.CompareTag("Head"))
            {
                _health = Mathf.Max(_health - damage, 0);
                if (_health == 0) shouldRagdoll = true;
            }
            else if (bodyPart.CompareTag("Upper Body"))
            {
                _upperBodyDamage += damage;
                UpdateAnimatorDamage();
            }
            else if (bodyPart.CompareTag("Lower Body"))
            {
                _lowerBodyDamage += damage;
                UpdateAnimatorDamage();
                shouldRagdoll = true;
            }
        }

        if (_boneControllType != AIBoneControlType.Animated || isCrawling || CinematicEnabled || attackerLocPos.z < 0) shouldRagdoll = true;

        if (!shouldRagdoll)
        {
            float angle = 0f;
            if (hitDirection == 0)
            {
                Vector3 vecToHit = (position - transform.position).normalized;
                angle = AIState.FindSignedAngle(vecToHit, transform.forward);
            }

            int hitType = 0;
            if (bodyPart.gameObject.CompareTag("Head"))
            {
                if          (angle < -10    || hitDirection == -1)      hitType = 1;
                else if     (angle > 10     || hitDirection == 1)       hitType = 3;
                else                                                    hitType = 2;
            }
            else if (bodyPart.gameObject.CompareTag("Upper Body"))
            {
                if          (angle < -20    || hitDirection == -1)      hitType = 4;
                else if     (angle > 20     || hitDirection == 1)       hitType = 6;
                else                                                    hitType = 5;
            }
            if (_animator)
            {
                animator.SetInteger(_hitTypeHash, hitType);
                animator.SetTrigger(_hitTriggerHash);
            }
            return;
        }
        else
        {
            if (_currentState)
            {
                _currentState.OnExitState();
                _currentState = null;
                _currentStateType = AIStateType.None;
            }

            if (_navAgent) _navAgent.enabled = false;
            if (_animator) _animator.enabled = false;
            if (_collider) _collider.enabled = false;

            inMeleeRange = false;

            foreach (Rigidbody body in _bodyParts) if (body) body.isKinematic = false;

            if (hitStrenght > 5f) if (bodyPart != null) bodyPart.AddForce(force, ForceMode.Impulse);

            _boneControllType = AIBoneControlType.Ragdoll;

            if (_health > 0)
            {
                if (_reanimationCoroutine != null) StopCoroutine(_reanimationCoroutine);

                _reanimationCoroutine = Reanimate();
                StartCoroutine(Reanimate());
            }
        }
    }

    // ----------------------------------------------------------------------
    // Name: Reanimate (Coroutine)
    // Desc: This method reanimates the zombie body.
    // ----------------------------------------------------------------------
    private IEnumerator Reanimate()
    {

        yield return null;

    }
}