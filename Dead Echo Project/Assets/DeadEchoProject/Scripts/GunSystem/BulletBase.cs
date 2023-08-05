using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    private float speed;
    private float gravity;
    private Vector3 startPosition;
    private Vector3 startForward;

    private bool isInitialized = false;
    private float startTime = -1;

    public void Initialize(Transform startPoint, float speed, float gravity)
    {
        startPosition = startPoint.position;
        startForward = startPoint.forward;

        this.speed = speed;
        this.gravity = gravity;
        isInitialized = true;
    }

    private Vector3 FindPointOnParabola(float time)
    {
        Vector3 point = startPosition + (startForward * speed * time);
        Vector3 gravityVec = Vector3.down * gravity * time * time;
        return point + gravityVec;
    }
    private bool CastRayBetweenPoints(Vector3 startPoint, Vector3 endPoint, out RaycastHit hit)
    {
        return Physics.Raycast(startPoint, endPoint - startPoint, out hit, (endPoint - startPoint).magnitude);
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;
        if (startTime < 0) startTime = Time.time;

        RaycastHit hit;
        float currentTime = Time.time - startTime;
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

    private void Update()
    {
        if (!isInitialized || startTime < 0) return;

        float currentTime = Time.time - startTime;
        Vector3 currentPoint = FindPointOnParabola(currentTime);
        transform.position = currentPoint;
    }

    public virtual void OnHit(RaycastHit hit)
    {
        //Debug.Log("Hit!!");
        gameObject.SetActive(false);
    }

}