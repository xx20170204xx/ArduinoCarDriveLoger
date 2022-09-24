using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MeterUnitController))]//Šg’£‚·‚éƒNƒ‰ƒX‚ðŽw’è
public class MeterUnitControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if( GUILayout.Button("Demo") )
        {
            Debug.Log("Pushed");
            MeterUnitController targetScript = target as MeterUnitController;
            if (targetScript != null)
            {
                targetScript.StartDemoPlay();
            }
        }
    } /* OnInspectorGUI */
} /* class */
