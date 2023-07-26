using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AI_Example : MonoBehaviour
{
    NavMeshAgent agent => GetComponent<NavMeshAgent>();

    public WaypointNetwork network;

    public int currentIndex;

    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve jumpCurve = new AnimationCurve();

    private void Start()
    {
        if (network == null) return;

        SetNextDestination(false);
    }
    void SetNextDestination(bool increment)
    {
        if (!network) return;

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        while(nextWaypointTransform == null)
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
    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + (agent.baseOffset * Vector3.up);

        float time = 0.0f;

        while(time <= duration)
        {
            float t = time / duration;
            agent.transform.position = Vector3.Lerp(startPos, endPos, t) + (jumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;

            yield return null;
        }
        agent.CompleteOffMeshLink();
    }
    private void Update()
    {

        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }

        if ((agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending) || pathStatus == NavMeshPathStatus.PathInvalid) SetNextDestination(true);
        else if (agent.isPathStale) SetNextDestination(false);

    }
}