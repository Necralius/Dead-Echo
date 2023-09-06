using System;
using UnityEngine;
using Random = UnityEngine.Random;

// --------------------------------------------------------------------------
// Name: BulletBase (Class)
// Desc: This class manages an bullet system, with bullet drop and complex
//       hit interaction system.
// --------------------------------------------------------------------------
public class BulletBase : MonoBehaviour
{
    //Private Data
    private float       _bulletSpeed                = 0f;
    private float       _bulletGravity              = 0f;
    private float       _bulletDamage               = 10f;
    //private int         _bulletOriginCharManager    = 0;
    private float       _bulletImpactForce          = 30f;
    private Vector3     _startPosition              = Vector3.zero;
    private Vector3     _startForward               = Vector3.zero;

    private bool        _isInitialized              = false;
    private float       _startTime                  = -1;

    private float       _bulletLifeTime             = 15f;
    private float       _deactiveTimer              = 0;
    private Vector3     _direction                  = Vector3.zero;
    private LayerMask   _collisionLayerMask;

    private Transform   _bulletOrigin = null;

    Func<Vector3, string> _hitInteraction;

    // ----------------------------------------------------------------------
    // Name: Initialize
    // Desc: This method is called on the bullet activation, the method
    //       initialize the bullet, passing all needed data and setting it to
    //       the bullet instance.
    // ----------------------------------------------------------------------
    public void Initialize(Transform startPoint, 
        float spread, 
        float speed, 
        float gravity, 
        float bulletLifeTime, 
        LayerMask collisionLayerMask, 
        float bulletDamage, 
        float bulletImpactForce, 
        Transform playerAgent)
    {
        _direction              = Vector3.zero;
        _startPosition          = startPoint.position;

        float x                 = Random.Range(-spread, spread);
        float y                 = Random.Range(-spread, spread);
        _bulletOrigin           = playerAgent;

        _direction              = playerAgent.GetComponent<FPS_Controller>().cameraObject.transform.forward + new Vector3(x, y, 0);

        _startForward           = startPoint.forward + _direction;

        _bulletSpeed            = speed;
        _bulletGravity          = gravity;
        _bulletLifeTime         = bulletLifeTime;
        _collisionLayerMask     = collisionLayerMask;
        _bulletImpactForce      = bulletImpactForce;
        _bulletDamage           = bulletDamage;

        _isInitialized          = true;
    }
    // ----------------------------------------------------------------------
    // Name: ResetBullet
    // Desc: This method resets completly the bullet data, the bullet is
    //       spawned using an Object Pooler system that set data to the
    //       object, but in the future, the player will need the same bullet
    //       of the pooler system and this bullet will have data gargabe tha
    //       need to be clean, this method clean the complete bullet data.
    // ----------------------------------------------------------------------
    private void ResetBullet()
    {
        _startPosition          = Vector3.zero;
        _startForward           = Vector3.zero;
        
        _direction              = Vector3.zero;
        _bulletSpeed            = 0f;
        _bulletGravity          = 0f;
        _bulletLifeTime         = 0f;
        _collisionLayerMask     = LayerMask.NameToLayer("Default");
        _isInitialized          = false;
    }

    // ----------------------------------------------------------------------
    // Name: FindPointOnParabola
    // Desc: This method uses an time float to find an point in a parabola
    //       trajectory, also applying gravity to the parabola point generated.
    // ----------------------------------------------------------------------
    private Vector3 FindPointOnParabola(float time)
    {
        Vector3 point       = _startPosition + (_startForward * _bulletSpeed * time);
        Vector3 gravityVec  = Vector3.down * _bulletGravity * time * time;
        return point + gravityVec;
    }

    // ----------------------------------------------------------------------
    // Name: CastRayBetweenPoints
    // Desc: This method executes an raycast between two points and returns
    //       the hit result of this raycast.
    // ----------------------------------------------------------------------
    private bool CastRayBetweenPoints(Vector3 startPoint, Vector3 endPoint, out RaycastHit hit)
    {
        return Physics.Raycast(startPoint, endPoint - startPoint, out hit, (endPoint - startPoint).magnitude, _collisionLayerMask);
    }

    // ----------------------------------------------------------------------
    // Name : FixedUpdate
    // Desc : 
    // ----------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (!_isInitialized) return;
        if (_startTime < 0) _startTime = Time.time;

        RaycastHit hit;
        float currentTime       = Time.time - _startTime;
        float prevTime          = currentTime - Time.fixedDeltaTime;
        float nextTime          = currentTime + Time.fixedDeltaTime;

        Vector3 currentPoint    = FindPointOnParabola(currentTime);
        Vector3 nextPoint       = FindPointOnParabola(nextTime);

        if (prevTime > 0)
        {
            Vector3 prevPoint = FindPointOnParabola(prevTime);
            if (CastRayBetweenPoints(prevPoint, currentPoint, out hit)) OnHit(hit);
        }

        if (CastRayBetweenPoints(currentPoint, nextPoint, out hit)) OnHit(hit);
    }

    // ----------------------------------------------------------------------
    // Name : Update
    // Desc : Called every frame, this method manages the bullet movment
    //        and deactivation time.
    // ----------------------------------------------------------------------
    private void Update()
    {
        if (!_isInitialized || _startTime < 0) return;
        if (_deactiveTimer >= _bulletLifeTime) gameObject.SetActive(false);

        float currentTime       = Time.time - _startTime;
        Vector3 currentPoint    = FindPointOnParabola(currentTime);
        transform.position      = currentPoint;

        _deactiveTimer          += Time.deltaTime;
    }

    // ----------------------------------------------------------------------
    // Name : OnHit
    // Desc : This method notify the bullet hit, instatiate the impact and
    //       decal particle if valid, the method also verifies if the
    //       bullet has hit an AI Instance and finally the method has an
    //       costum function that is public and can be passed on the bullet
    //       shoot, this function will be called on the bullet hit.
    // ----------------------------------------------------------------------
    public virtual void OnHit(RaycastHit hit)
    {
        if (_hitInteraction != null) _hitInteraction(hit.point);

        ObjectPooler.Instance.SpawnFromPool(LayerMask.LayerToName(hit.collider.gameObject.layer) + "Hit", hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
        ObjectPooler.Instance.SpawnFromPool(LayerMask.LayerToName(hit.collider.gameObject.layer) + "Decal", hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));

        // In this code section the bullet verifies if the hit has finded an object of type AI Body Part and executes an aditional actions on the hit.
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("AI Body Part"))
        {
            // The method tries to get an valid AI Instance acessing the GameSceneManager and execute an hit action
            AiStateMachine stateMachine = GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID());
            if (stateMachine) 
                stateMachine.TakeDamage(hit.point, -hit.normal * _bulletImpactForce, (int)_bulletDamage, hit.rigidbody, CharacterManager.Instance, 0);
        }
        // This code section resets the bullet data after its impact interactions
        ResetBullet();

        //Finally, after all interactions, the bullet will self deactivate to return to the Object Pooler object library.
        gameObject.SetActive(false);
    }
}