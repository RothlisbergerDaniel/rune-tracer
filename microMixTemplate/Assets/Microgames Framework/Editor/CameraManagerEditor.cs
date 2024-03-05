using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraManager))]
public class CameraManagerEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CameraManager script = (CameraManager)target;

        if (GUILayout.Button("Update Game View")) {
            if (script.firstScreenCamera != null)
                Undo.RecordObject(script.firstScreenCamera, "Update Game View");
            if (script.secondScreenCamera != null)
                Undo.RecordObject(script.secondScreenCamera, "Update Game View");
            script.AdjustCameraViewport();
        }

    }
}
