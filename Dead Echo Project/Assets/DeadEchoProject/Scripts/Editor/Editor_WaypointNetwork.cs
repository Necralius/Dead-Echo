using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(WaypointNetwork))]
public class Editor_WaypointNetwork : Editor
{

    public override void OnInspectorGUI()
    {
        WaypointNetwork network = (WaypointNetwork)target;

        network.displayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode" , network.displayMode);
        if (network.displayMode == PathDisplayMode.Path)
        {
            network.UIStart = EditorGUILayout.IntSlider("Waypoint Start", network.UIStart, 0, network.waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("Waypoint End", network.UIEnd, 0, network.waypoints.Count - 1);
        }

        DrawDefaultInspector();
    }
    private void OnSceneGUI()
    {
        WaypointNetwork network = (WaypointNetwork)target;

        for (int i = 0; i < network.waypoints.Count; i++)
        {
            if (network.waypoints[i] != null) Handles.Label(network.waypoints[i].position, "Waypoint " + i);
        }

        if (network.displayMode == PathDisplayMode.Connections)
        {
            Vector3[] linePoints = new Vector3[network.waypoints.Count + 1];

            for (int i = 0; i <= network.waypoints.Count; i++)
            {
                int index = i != network.waypoints.Count ? i : 0;

                if (network.waypoints[index] != null) linePoints[i] = network.waypoints[index].position;
                else linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
            Handles.color = Color.red;
            Handles.DrawPolyLine(linePoints);
        }
        else if (network.displayMode == PathDisplayMode.Path)
        {
            NavMeshPath path = new NavMeshPath();
            Vector3 from = network.waypoints[network.UIStart].position;
            Vector3 to = network.waypoints[network.UIEnd].position;

            NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
            Handles.color = Color.yellow;
            Handles.DrawPolyLine(path.corners);

        }
    }
}