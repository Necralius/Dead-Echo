using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathDisplayMode { None, Connections, Path}
public class WaypointNetwork : MonoBehaviour
{
    [HideInInspector] public PathDisplayMode displayMode = PathDisplayMode.Connections;
    [HideInInspector] public int UIStart;
    [HideInInspector] public int UIEnd;

    public List<Transform> waypoints;





}