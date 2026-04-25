using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildSystem))]
public class BuildSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildSystem t = (BuildSystem)target;

        // 先画开关
        t.useGrassSystem = EditorGUILayout.Toggle("Use Grass System", t.useGrassSystem);

        // ⭐ 条件显示
        if (t.useGrassSystem)
        {
            EditorGUI.indentLevel++;

            t.distribution = (DistributionType)EditorGUILayout.EnumPopup("Distribution", t.distribution);
            t.mapSeed = EditorGUILayout.IntField("Map Seed", t.mapSeed);

            EditorGUILayout.Space();

            t.material = (Material)EditorGUILayout.ObjectField("Material", t.material, typeof(Material), false);
            t.atlasData = (GrassAtlasData)EditorGUILayout.ObjectField("Atlas Data", t.atlasData, typeof(GrassAtlasData), false);
            t.targetCount = EditorGUILayout.IntField("targetCount", t.targetCount);
            t.density = EditorGUILayout.FloatField("density", t.density);
            EditorGUI.indentLevel--;
        }

        // 保存修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(t);
        }
    }
}