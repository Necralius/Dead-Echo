using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentRootMotion : MonoBehaviour
{
    #region - Dependencies -
    NavMeshAgent currentAgent => GetComponent<NavMeshAgent>();
    Animator anim => GetComponent<Animator>();
    public WaypointNetwork network;
    #endregion

    [Header("AI Status")]
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathComplete;

    public int currentIndex;

    [Header("Animation Hashes")]
    private int AngleHash;
    private int SpeedHash;

    public float smoothAngle = 0;

    public bool mixedMode = true;

    public AnimationCurve jumpCurve = new AnimationCurve();

    private void Start()
    {
        if (currentAgent == null) return;
        if (network == null) return;
        if (anim == null) return;

        AngleHash = Animator.StringToHash("Angle");
        SpeedHash = Animator.StringToHash("Speed");

        currentAgent.updateRotation = false;

        SetNextDestination(false);
    }
    private void Update()
    {

        //Jump from off mesh link example
        if (currentAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }

        LocomotionSystem();
    }

    #region - Waypoint System -
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
                currentAgent.destination = nextWaypointTransform.position;
                return;
            }
        }
        currentIndex++;
    }
    #endregion

    #region - Off Mesh Link System -
    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = currentAgent.currentOffMeshLinkData;
        Vector3 startPos = currentAgent.transform.position;
        Vector3 endPos = data.endPos + (currentAgent.baseOffset * Vector3.up);

        float time = 0.0f;

        while (time <= duration)
        {
            float t = time / duration;
            currentAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (jumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;

            yield return null;
        }
        currentAgent.CompleteOffMeshLink();
    }
    #endregion

    #region - Animation and Locomotion System -
    private void LocomotionSystem()
    {
        Vector3 localDesiredVelocity = transform.InverseTransformVector(currentAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;

        smoothAngle = Mathf.MoveTowardsAngle(smoothAngle, angle, 80.0f * Time.deltaTime);

        float speed = localDesiredVelocity.z;

        anim.SetFloat(AngleHash, smoothAngle);
        anim.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);

        if (currentAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            if (!mixedMode || (mixedMode && Mathf.Abs(angle) < 80.0f && anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")))
            {
                Quaternion lookRotation = Quaternion.LookRotation(currentAgent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
            }
        }

        if ((currentAgent.remainingDistance <= currentAgent.stoppingDistance && !currentAgent.pathPending) || PathStatus == NavMeshPathStatus.PathInvalid) SetNextDestination(true);
        else if (currentAgent.isPathStale) SetNextDestination(false);
    }
    private void OnAnimatorMove()
    {
        if (mixedMode && !anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")) transform.rotation = anim.rootRotation;

        currentAgent.velocity = anim.deltaPosition / Time.deltaTime;
    }
    #endregion
}