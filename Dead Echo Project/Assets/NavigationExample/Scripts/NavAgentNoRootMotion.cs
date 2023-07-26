using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{
    NavMeshAgent agent => GetComponent<NavMeshAgent>();
    Animator anim => GetComponent<Animator>();

    public WaypointNetwork network;

    public int currentIndex;

    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    private float originalMaxSpeed = 0;

    //public AnimationCurve jumpCurve = new AnimationCurve();

    private void Start()
    {
        if (agent == null) return;
        if (network == null) return;

        originalMaxSpeed = agent.speed;

        SetNextDestination(false);
    }
    void SetNextDestination(bool increment)
    {
        if (!network) return;

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        while (nextWaypointTransform == null)
        {
            int nextWaypoint = (currentIndex + incStep >= network.waypoints.Count) ? 0 : currentIndex + incStep;
            nextWaypointTransform = network.waypoints[nextWaypoint];

            if (nextWaypointTransform != null)
            {
                currentIndex = nextWaypoint;
                agent.destination = nextWaypointTransform.position;
                return;
            }
        }
        currentIndex++;
    }

    //IEnumerator Jump(float duration)
    //{
    //    OffMeshLinkData data = agent.currentOffMeshLinkData;
    //    Vector3 startPos = agent.transform.position;
    //    Vector3 endPos = data.endPos + (agent.baseOffset * Vector3.up);

    //    float time = 0.0f;

    //    while (time <= duration)
    //    {
    //        float t = time / duration;
    //        agent.transform.position = Vector3.Lerp(startPos, endPos, t) + (jumpCurve.Evaluate(t) * Vector3.up);
    //        time += Time.deltaTime;

    //        yield return null;
    //    }
    //    agent.CompleteOffMeshLink();
    //}

    private void Update()
    {
        int turnOnSpot;

        Vector3 cross = Vector3.Cross(transform.forward, agent.desiredVelocity.normalized);
        float horizontal = (cross.y < 0) ? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);

        if (agent.velocity.magnitude < 1.0f && Vector3.Angle(transform.forward, agent.desiredVelocity) > 10.0f)
        {
            agent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(horizontal);
        }
        else
        {
            agent.speed = originalMaxSpeed;
            turnOnSpot = 0;
        }

        anim.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        anim.SetFloat("Vertical", agent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        anim.SetInteger("TurnOnSpot", turnOnSpot);


        //if (agent.isOnOffMeshLink)
        //{
        //    StartCoroutine(Jump(1.0f));
        //    return;
        //}

        if ((agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending) || pathStatus == NavMeshPathStatus.PathInvalid) SetNextDestination(true);
        else if (agent.isPathStale) SetNextDestination(false);
    }
}