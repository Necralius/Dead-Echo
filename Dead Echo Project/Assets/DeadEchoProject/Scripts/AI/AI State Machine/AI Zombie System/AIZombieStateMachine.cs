using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIBoneControlType   { Animated, Ragdoll, RagdollToAnim }
public enum AIScreamPosition    { Entity, Player}

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
}

// --------------------------------------------------------------------------
// Name: AIZombieStateMachine
// Desc: State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AiStateMachine
{
    //This datas are active in the inspector for default AiStateMachine Data

    [Header("Zombie Settings")]
    [SerializeField, Range(10f, 360f)]      float               _fov                    = 50f;
    [SerializeField, Range(0f, 1f)]         float               _sight                  = 0.5f;
    [SerializeField, Range(0f, 1f)]         float               _hearing                = 10f;
    [SerializeField, Range(0f, 1f)]         float               _agression              = 10f;
    [SerializeField, Range(0f, 1f)]         float               _intelligence           = 0.5f;

    [Header("Feed System")]
    [SerializeField, Range(0f, 1f)]         float               _satisfaction           = 1f;
    [SerializeField, Range(0f, 1f)]         float               _replenishRate          = 0.5f;
    [SerializeField, Range(0f, 1f)]         float               _depletionRate          = 0.1f;

    [Header("Zombie Body State")]
    [SerializeField, Range(0, 100)]         int                 _health                 = 100;
    [SerializeField, Range(0, 100)]         int                 _lowerBodyDamage        = 0;
    [SerializeField, Range(0, 100)]         int                 _upperBodyDamage        = 0;
    [SerializeField, Range(0, 100)]         int                 _upperBodyThreshold     = 30;
    [SerializeField, Range(0, 100)]         int                 _limpThreshold          = 30;
    [SerializeField, Range(0, 100)]         int                 _crawlThreshold         = 90;

    [Header("Scream System")]
    [SerializeField, Range(0f, 1f)]         float               _screamChance           = 1f;
    [SerializeField, Range(0f, 50f)]        float               _screamRadius           = 20f;
    [SerializeField]                        AIScreamPosition    _screamPosition         = AIScreamPosition.Entity;
    [SerializeField]                        AISoundEmitter      _screamPrefab           = null;

    [Header("Reanimation System")]
    [SerializeField]                        float               _reanimationBlendTime   = 0.5f;
    [SerializeField]                        float               _reanimationWaitTime    = 3f;

    [SerializeField]                        LayerMask           _geometryLayers;

    //Private Data
    private int     _seeking        = 0;
    private bool    _feeding        = false;
    private bool    _crawling       = false;
    private int     _attackType     = 0;
    private float   _speed          = 0.0f;
    private float   _isScreaming    = 0f;

    // Animation Parameters Hashes
    private int _speedHash              = Animator.StringToHash("Speed");
    private int _seekingHash            = Animator.StringToHash("Seeking");
    private int _feedingHash            = Animator.StringToHash("Feeding");
    private int _attackHash             = Animator.StringToHash("Attack");
    private int _crawlingHash           = Animator.StringToHash("Crawling");
    private int _screamingHash          = Animator.StringToHash("Screaming");
    private int _screamHash             = Animator.StringToHash("Scream");
    private int _hitTriggerHash         = Animator.StringToHash("Hit");
    private int _hitTypeHash            = Animator.StringToHash("HitType");
    private int _reanimateFromBackHash  = Animator.StringToHash("Reanimate From Back");
    private int _reanimateFromFrontHash = Animator.StringToHash("Reanimate From Front");
    private int _upperBodyDamageHash    = Animator.StringToHash("Upper Body Damage");
    private int _lowerBodyDamageHash    = Animator.StringToHash("Lower Body Damage");
    private int _stateHash              = Animator.StringToHash("State");

    // Animation Layers Cast
    private int _upperBodyLayer = -1;
    private int _lowerBodyLayer = -1;

    // Private Ragdoll System Data
    private AIBoneControlType           _boneControllType       = AIBoneControlType.Animated;
    private List<BodyPartSnapshot>      _bodyPartSnapshots      = new List<BodyPartSnapshot>();
    private float                       _ragdollEndTime         = float.MinValue;
    private Vector3                     _ragdollHipPosition;
    private Vector3                     _ragdollFeetPosition;
    private Vector3                     _ragdoolHeadPosition;
    private IEnumerator                 _reanimationCoroutine   = null;
    private float                       _mecanimTransitionTime  = 0.1f;

    #region - Encapsulated Data -
    // Public Encapsulated Data
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
    public bool     isCrawling      { get => _lowerBodyDamage >= _crawlThreshold; }
    public bool     isScreaming     { get => _isScreaming > 0.1f; }
    public float    screamChance    { get => _screamChance; }
    #endregion

    // ------------------------------------------ Methods ------------------------------------------ //

    #region - BuiltIn Methods -
    // ----------------------------------------------------------------------
    // Name: Start
    // Desc: This method is called on the game start, the method is being
    //       overrided for another code parts implementation.
    // ----------------------------------------------------------------------
    protected override void Start()
    {
        base.Start();

        if (_animator != null)
        {
            //Cast Layer Indexes
            _lowerBodyLayer = _animator.GetLayerIndex("Lower Body");
            _upperBodyLayer = _animator.GetLayerIndex("Upper Body");
        }

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
        if (GameSceneManager.Instance._gameIsPaused) return;
        base.Update();

        if (_animator != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_stateHash, (int)_currentStateType);

            _isScreaming = IsLayerActive("Cinematic") ? 0f : _animator.GetFloat(_screamingHash);
        }

        _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime)/ 100f) * Mathf.Pow(_speed, 3f));
    }

    // ----------------------------------------------------------------------
    // Name: LateUpdate
    // Desc: This method is called after almost every frame actions
    //       calculations, mainly the system calculates the zombie instance
    //       body reanimation system.
    // ----------------------------------------------------------------------
    protected virtual void LateUpdate()
    {
        if (_boneControllType == AIBoneControlType.RagdollToAnim)
        {
            if (Time.time <= _ragdollEndTime + _mecanimTransitionTime)
            {
                Vector3 animatedToRagdoll = _ragdollHipPosition - _rootBone.position;
                Vector3 newRootPosition = transform.position + animatedToRagdoll;

                RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.50f), Vector3.down, float.MaxValue, _geometryLayers);
                newRootPosition.y = float.MinValue;

                foreach (RaycastHit hit in hits)
                    if (!hit.transform.IsChildOf(transform)) newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);

                NavMeshHit navMeshHit;

                Vector3 baseOffset = Vector3.zero;
                if (_navAgent) baseOffset.y = _navAgent.baseOffset;

                if (NavMesh.SamplePosition(newRootPosition, out navMeshHit, 25f, NavMesh.AllAreas)) transform.position = navMeshHit.position + baseOffset;
                else transform.position = newRootPosition + baseOffset;

                Vector3 ragdollDirection = _ragdoolHeadPosition - _ragdollFeetPosition;
                ragdollDirection.y = 0f;

                Vector3 meanFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                animatedDirection.y = 0f;

                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollDirection.normalized);
            }

            float blendAmount = Mathf.Clamp01((Time.time - _ragdollEndTime - _mecanimTransitionTime) / _reanimationBlendTime);

            foreach (BodyPartSnapshot snapshot in _bodyPartSnapshots)
            {
                if (snapshot.transform == _rootBone)
                    snapshot.transform.position = Vector3.Lerp(snapshot.position, snapshot.transform.position, blendAmount);

                snapshot.transform.rotation = Quaternion.Slerp(snapshot.rotation, snapshot.transform.rotation, blendAmount);
            }

            if (blendAmount == 1f)
            {
                _boneControllType = AIBoneControlType.Animated;
                if (_navAgent) _navAgent.enabled = true;
                if (_collider) _collider.enabled = true;

                AIState newState = null;

                if (_states.TryGetValue(AIStateType.Alerted, out newState))
                {
                    if (_currentState != null) _currentState.OnExitState();

                    newState.OnEnterState();
                    _currentState = newState;
                    _currentStateType = AIStateType.Alerted;
                }
            }
        }
    }
    #endregion

    #region - Zombie Body Damage System -
    // ----------------------------------------------------------------------
    // Name: UpdateAnimatorDamage
    // Desc: This method updates the zombie body damage, using the animator
    //       an its states.
    // ----------------------------------------------------------------------
    protected void UpdateAnimatorDamage()
    {
        if (_animator != null)
        {
            if (_lowerBodyLayer != -1) 
                _animator.SetLayerWeight(_lowerBodyLayer, (_lowerBodyDamage > _limpThreshold && _lowerBodyDamage < _crawlThreshold) ? 1f : 0f);
            if (_upperBodyLayer != -1) 
                _animator.SetLayerWeight(_upperBodyLayer, (_upperBodyDamage > _upperBodyThreshold && _lowerBodyDamage < _crawlThreshold) ? 1f : 0f);

            _animator.SetBool(_crawlingHash, isCrawling);
            _animator.SetInteger(_lowerBodyDamageHash, _lowerBodyDamage);
            _animator.SetInteger(_upperBodyDamageHash, _upperBodyDamage);

            SetLayerActive("Lower Body", _lowerBodyDamage > _limpThreshold && _lowerBodyDamage < _crawlThreshold);
            SetLayerActive("Upper Body", _upperBodyDamage > _upperBodyThreshold && _lowerBodyDamage < _crawlThreshold);

        }
    }
    // ----------------------------------------------------------------------
    // Name: TakeDamage
    // Desc: This method executes an damage action on the zombie, considering
    //       the attack force, damage value, direction and position.
    // ----------------------------------------------------------------------
    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection)
    {
        if (GameSceneManager.Instance != null && GameSceneManager.Instance.bloodParticles != null)
        {
            ParticleSystem blood = GameSceneManager.Instance.bloodParticles;
            blood.transform.position = position;

            var main = blood.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            blood.Emit(60);
        }

        float hitStrenght = force.magnitude;

        //Debug.Log($"AI_ZSM Hit Strenght: {hitStrenght}, Force: {force}");

        if (_boneControllType == AIBoneControlType.Ragdoll)
        {
            if (bodyPart != null)
            {
                if (hitStrenght > 20f) bodyPart.AddForce(force, ForceMode.Impulse);

                switch (bodyPart.tag)
                {
                    case "Head":
                        _health -= damage * 10;
                        HitMarkerManager.Instance.OnHit(new HitInfo((HitInfo.HitType)1, position));
                        break;
                    case "Upper Body":
                        _upperBodyDamage += damage * 10;
                        HitMarkerManager.Instance.OnHit(new HitInfo(0, position));
                        break;
                    case "Lower Body":
                        _lowerBodyDamage += damage * 10;
                        HitMarkerManager.Instance.OnHit(new HitInfo(0, position));
                        break;
                }

                if (bodyPart.CompareTag("Head")) _health -= damage * 10;
                else if (bodyPart.CompareTag("Upper Body")) _upperBodyDamage += damage * 10;
                else if (bodyPart.CompareTag("Lower Body")) _lowerBodyDamage += damage * 10;

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
       
        bool shouldRagdoll = (hitStrenght > 20f);

        if (bodyPart != null)
        {
            if (bodyPart.CompareTag("Head"))
            {
                _health -= damage * 10;
                if (_health <= 0) shouldRagdoll = true;
            }
            else if (bodyPart.CompareTag("Upper Body"))
            {
                _upperBodyDamage += damage * 10;
                UpdateAnimatorDamage();
            }
            else if (bodyPart.CompareTag("Lower Body"))
            {
                _lowerBodyDamage += damage * 10;
                UpdateAnimatorDamage();
                shouldRagdoll = true;
            }
        }

        if (_boneControllType != AIBoneControlType.Animated || isCrawling || IsLayerActive("Cinematic") || attackerLocPos.z < 0) shouldRagdoll = true;

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

            if (hitStrenght > 15f) if (bodyPart != null) bodyPart.AddForce(force, ForceMode.Impulse);

            _boneControllType = AIBoneControlType.Ragdoll;

            if (_health > 0)
            {
                if (_reanimationCoroutine != null) StopCoroutine(_reanimationCoroutine);

                _reanimationCoroutine = Reanimate();
                StartCoroutine(Reanimate());
            }
        }
    }
    #endregion

    #region - Zombie Behavior -
    // ----------------------------------------------------------------------
    // Name: Scream
    // Desc: This method represents an zombie scream action, basically the
    //       system makes the zombie scream when the method is called, the
    //       method consider the actual behavior (if the zombie is already
    //       screaming), also, the system returns an response to the scream
    //       action sucess.
    // ----------------------------------------------------------------------
    public bool Scream()
    {
        if (isScreaming) return true;
        if (_animator == null || IsLayerActive("Cinematic") || _screamPrefab == null || isCrawling) return false;

        _animator.SetTrigger(_screamHash);
        Vector3 spawnPos = _screamPosition == AIScreamPosition.Entity ? transform.position : VisualThreat.position;
        AISoundEmitter screamEmitter = Instantiate(_screamPrefab, spawnPos, Quaternion.identity);

        if (screamEmitter != null) screamEmitter.SetRadius(_screamRadius);
        return true;
    }
    #endregion

    #region - Ragdoll Reanimation -
    // ----------------------------------------------------------------------
    // Name: Reanimate (Coroutine)
    // Desc: This method reanimates the zombie body.
    // ----------------------------------------------------------------------
    protected IEnumerator Reanimate()
    {
        if (_health == 0) yield break;

        //Reanimate only if the zombie is in a ragdoll state
        if (_boneControllType != AIBoneControlType.Ragdoll || _animator == null) yield break;

        //Wait for the desired number of seconds before initiating the reanimation process
        yield return new WaitForSeconds(_reanimationWaitTime);

        //Record time at start of reanimation procedure
        _ragdollEndTime = Time.time;

        //Set rigidibodies back to begin kinematic
        foreach(Rigidbody body in _bodyParts) body.isKinematic = true;

        //Seting the zombie back in the reanimation mode
        _boneControllType = AIBoneControlType.RagdollToAnim;

        //Save positions and rotations of all bones prios to reanimation
        foreach(BodyPartSnapshot snapshot in _bodyPartSnapshots)
        {
            snapshot.position       = snapshot.transform.position;
            snapshot.rotation       = snapshot.transform.rotation;
        }

        //Record the ragdolls head and feet position
        _ragdoolHeadPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
        _ragdollFeetPosition = (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.2f;
        _ragdollHipPosition  = _rootBone.position;

        //Re-enables the animator
        _animator.enabled = true;

        if (_rootBone != null)
        {
            float forwardTest;
            switch (_rootBoneAligmentType)
            {
                case AIBoneAligmentType.ZAxis:          forwardTest = _rootBone.forward.y;      break;
                case AIBoneAligmentType.ZAxisInverted:  forwardTest = -_rootBone.forward.y;     break;
                case AIBoneAligmentType.YAxis:          forwardTest = _rootBone.up.y;           break;
                case AIBoneAligmentType.YAxisInverted:  forwardTest = -_rootBone.up.y;          break;
                case AIBoneAligmentType.XAxis:          forwardTest = _rootBone.right.y;        break;
                case AIBoneAligmentType.XAxisInverted:  forwardTest = -_rootBone.right.y;       break;
                default:                                forwardTest = _rootBone.forward.y;      break;
            }
            if (forwardTest > 0) _animator.SetTrigger(_reanimateFromBackHash);
            else _animator.SetTrigger(_reanimateFromFrontHash);
        }
        yield break;
    }
    #endregion 
}