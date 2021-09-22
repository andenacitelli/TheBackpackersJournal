using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Hearing))]
public class HearingEditor : Editor
{
    private void OnSceneGUI()
    {
        // provide visual view distance in editor
        Hearing hearing = (Hearing)target;
        Handles.color = Color.blue;
        Handles.DrawWireArc(hearing.transform.position, Vector3.up, Vector3.forward, 360.0f, hearing.hearingRadius);

        Handles.color = Color.cyan;
        foreach (GameObject seenTarget in hearing.detectedTargets)
        {
            Handles.DrawLine(hearing.transform.position, seenTarget.transform.position);
        }
    }
}
