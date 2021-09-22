using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (Sight))]
public class SightEditor : Editor
{
    private void OnSceneGUI()
    {
        // provide visual view distance in editor
        Sight sight = (Sight)target;
        Handles.color = Color.green;

        // provide visual field of view in editor
        Vector3 viewAngleA = sight.AngleDirection(-sight.fieldOfView / 2, false);
        Vector3 viewAngleB = sight.AngleDirection(sight.fieldOfView / 2, false);

        Handles.DrawWireArc(sight.transform.position, Vector3.up, viewAngleA, sight.fieldOfView, sight.viewRadius);
        Handles.DrawLine(sight.transform.position, sight.transform.position + viewAngleA * sight.viewRadius);
        Handles.DrawLine(sight.transform.position, sight.transform.position + viewAngleB * sight.viewRadius);

        Handles.color = Color.magenta;
        foreach(GameObject seenTarget in sight.detectedTargets)
        {
            Handles.DrawLine(sight.transform.position, seenTarget.transform.position);
        }
    }
}
