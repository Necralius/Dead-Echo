using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletBase : MonoBehaviour
{
    private float _bulletSpeed;
    private float _bulletGravity;
    private Vector3 _startPosition;
    private Vector3 _startForward;

    private bool _isInitialized = false;
    private float _startTime = -1;

    private float _bulletLifeTime = 15f;
    private float _deactiveTimer = 0;
    private LayerMask _collisionLayerMask;
    private Vector3 _direction = Vector3.zero;

    Func<Vector3, string> hitInteraction;

    public void Initialize(Transform startPoint, float spread, float speed, float gravity, float bulletLifeTime, LayerMask collisionLayerMask)
    {
        _direction = Vector3.zero;
        _startPosition = startPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        _direction = FPS_Controller.Instance.cameraObject.transform.forward + new Vector3(x, y, 0);

        _startForward = startPoint.forward + _direction;

        _bulletSpeed = speed;
        _bulletGravity = gravity;
        _bulletLifeTime = bulletLifeTime;
        _collisionLayerMask = collisionLayerMask;
        _isInitialized = true;
    }
    private void ResetBullet()
    {
        _startPosition = Vector3.zero;
        _startForward = Vector3.zero;
        
        _direction = Vector3.zero;
        _bulletSpeed = 0f;
        _bulletGravity = 0f;
        _bulletLifeTime = 0f;
        _collisionLayerMask = LayerMask.NameToLayer("Default");
        _isInitialized = false;
    }

    private Vector3 FindPointOnParabola(float time)
    {
        Vector3 point = _startPosition + (_startForward * _bulletSpeed * time);
        Vector3 gravityVec = Vector3.down * _bulletGravity * time * time;
        return point + gravityVec;
    }
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
        float currentTime = Time.time - _startTime;
        float prevTime = currentTime - Time.fixedDeltaTime;
        float nextTime = currentTime + Time.fixedDeltaTime;

        Vector3 currentPoint = FindPointOnParabola(currentTime);
        Vector3 nextPoint = FindPointOnParabola(nextTime);

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

        float currentTime = Time.time - _startTime;
        Vector3 currentPoint = FindPointOnParabola(currentTime);
        transform.position = currentPoint;

        _deactiveTimer += Time.deltaTime;
    }

    // ----------------------------------------------------------------------
    // Name : OnHit
    // Desc : This method notify the bullet hit an executes the storaged
    //        function to represent an bullet impact interaction.
    // ----------------------------------------------------------------------
    public virtual void OnHit(RaycastHit hit)
    {
        if (hitInteraction != null) hitInteraction(hit.point);

        ObjectPooler.Instance.SpawnFromPool(LayerMask.LayerToName(hit.collider.gameObject.layer) + "Hit", hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
        ObjectPooler.Instance.SpawnFromPool(LayerMask.LayerToName(hit.collider.gameObject.layer) + "Decal", hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
        ResetBullet();
        
        gameObject.SetActive(false);
    }
}