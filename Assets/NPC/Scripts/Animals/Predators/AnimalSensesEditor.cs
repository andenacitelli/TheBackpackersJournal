#if (UNITY_EDITOR) 
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimalSenses))]
public class AnimalSensesEditor : Editor
{
    AnimalSenses senses;
    private void OnSceneGUI()
    {
        senses = (AnimalSenses)target;
        VisualizeVision();
        VisualizeHearing();
    }

    // provide visual view distance in editor
    private void VisualizeVision()
    {
        Handles.color = Color.green;

        // provide visual field of view in editor
        Vector3 viewAngleA = senses.AngleDirection(-senses.ViewingAngle / 2, false);
        Vector3 viewAngleB = senses.AngleDirection(senses.ViewingAngle / 2, false);

        Handles.DrawWireArc(senses.transform.position, Vector3.up, viewAngleA, senses.ViewingAngle, senses.VisionRadius);
        Handles.DrawLine(senses.transform.position, senses.transform.position + viewAngleA * senses.VisionRadius);
        Handles.DrawLine(senses.transform.position, senses.transform.position + viewAngleB * senses.VisionRadius);

        Handles.color = Color.magenta;
        foreach (Creature seenCreature in senses.SeenCreatures)
        {
            if (seenCreature != null) Handles.DrawLine(senses.transform.position, seenCreature.transform.position);
        }
    }

    // provide visual view distance in editor
    private void VisualizeHearing()
    {
        Handles.color = Color.blue;
        Handles.DrawWireArc(senses.transform.position, Vector3.up, Vector3.forward, senses.HearingAngle, senses.HearingRadius);

        Handles.color = Color.cyan;
        foreach (Creature heardCreature in senses.HeardCreatures)
        {
            if (heardCreature != null) Handles.DrawLine(senses.transform.position, heardCreature.transform.position);
        }
    }
}
#endif