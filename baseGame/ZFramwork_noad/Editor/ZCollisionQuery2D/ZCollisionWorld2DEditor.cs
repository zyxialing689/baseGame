#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ZGame.Collision2D;

[CustomEditor(typeof(ZCollisionWorld2D))]
public class ZCollisionWorld2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        ZCollisionWorld2D world = (ZCollisionWorld2D)target;
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Runtime Stats", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("Body Count", world.BodyCount);
            EditorGUILayout.IntField("Cell Count", world.CellCount);
            EditorGUILayout.IntField("Queries This Frame", world.QueryCountThisFrame);
            EditorGUILayout.IntField("Candidates This Frame", world.CandidateCountThisFrame);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rebuild All"))
            world.RebuildAll();
        if (GUILayout.Button("Clear"))
            world.ClearAll();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
