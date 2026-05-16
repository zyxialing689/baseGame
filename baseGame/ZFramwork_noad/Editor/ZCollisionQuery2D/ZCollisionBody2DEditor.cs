#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ZGame.Collision2D;

[CustomEditor(typeof(ZCollisionBody2D))]
public class ZCollisionBody2DEditor : Editor
{
    private static bool identityFoldout = true;
    private static bool flagsFoldout = true;
    private static bool hitFoldout = true;
    private static bool selectFoldout = true;
    private static bool interactFoldout = true;
    private static bool attackFoldout = true;
    private static bool debugFoldout = true;

    private static readonly Color GroundColor   = new Color(0.00f, 1.00f, 0.10f, 1f);
    private static readonly Color BodyColor     = new Color(0.10f, 0.30f, 1.00f, 1f);
    private static readonly Color SkyColor      = new Color(0.80f, 0.10f, 1.00f, 1f);
    private static readonly Color SelectColor   = new Color(1.00f, 0.95f, 0.05f, 1f);
    private static readonly Color InteractColor = new Color(1.00f, 0.45f, 0.05f, 1f);
    private static readonly Color AttackColor   = new Color(1.00f, 0.15f, 0.15f, 1f);

    private SerializedProperty teamId;
    private SerializedProperty layer;
    private SerializedProperty canBeHit;
    private SerializedProperty canBeSelected;
    private SerializedProperty canBeInteracted;
    private SerializedProperty groundBox;
    private SerializedProperty bodyBox;
    private SerializedProperty skyBox;
    private SerializedProperty selectBox;
    private SerializedProperty interactBox;
    private SerializedProperty drawMode;
    private SerializedProperty drawOccupiedCells;
    private SerializedProperty presetProp;
    private SerializedProperty attackBox;

    private void OnEnable()
    {
        teamId = serializedObject.FindProperty("teamId");
        layer = serializedObject.FindProperty("layer");
        canBeHit = serializedObject.FindProperty("canBeHit");
        canBeSelected = serializedObject.FindProperty("canBeSelected");
        canBeInteracted = serializedObject.FindProperty("canBeInteracted");
        groundBox = serializedObject.FindProperty("groundBox");
        bodyBox = serializedObject.FindProperty("bodyBox");
        skyBox = serializedObject.FindProperty("skyBox");
        selectBox = serializedObject.FindProperty("selectBox");
        interactBox = serializedObject.FindProperty("interactBox");
        drawMode = serializedObject.FindProperty("drawMode");
        drawOccupiedCells = serializedObject.FindProperty("drawOccupiedCells");
        presetProp = serializedObject.FindProperty("preset");
        attackBox = serializedObject.FindProperty("attackBox");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ZCollisionBody2D body = (ZCollisionBody2D)target;

        EditorGUILayout.Space(4);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(presetProp);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            var newPreset = (ZCollisionPreset)presetProp.enumValueIndex;
            if (newPreset != ZCollisionPreset.Custom)
            {
                Undo.RecordObject(body, "Apply Collision Preset");
                body.preset = newPreset;
                body.ApplyPreset(newPreset);
                EditorUtility.SetDirty(body);
                serializedObject.Update();
            }
        }

        EditorGUILayout.Space(4);
        identityFoldout = EditorGUILayout.Foldout(identityFoldout, "Identity", true);
        if (identityFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(teamId);
            EditorGUILayout.PropertyField(layer);
            EditorGUI.indentLevel--;
        }

        flagsFoldout = EditorGUILayout.Foldout(flagsFoldout, "Query Flags", true);
        if (flagsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(canBeHit, new GUIContent("Can Be Hit"));
            EditorGUILayout.PropertyField(canBeSelected, new GUIContent("Can Be Selected"));
            EditorGUILayout.PropertyField(canBeInteracted, new GUIContent("Can Be Interacted"));
            EditorGUI.indentLevel--;
        }

        if (canBeHit.boolValue)
        {
            hitFoldout = EditorGUILayout.Foldout(hitFoldout, "Hit Boxes: Ground / Body / Sky", true);
            if (hitFoldout)
            {
                EditorGUI.indentLevel++;
                bool isCustom = body.preset == ZCollisionPreset.Custom;
                if (isCustom)
                {
                    DrawBoxWithToggle("Ground Box", groundBox, GroundColor);
                    DrawBoxWithToggle("Body Box", bodyBox, BodyColor);
                    DrawBoxWithToggle("Sky Box", skyBox, SkyColor);
                }
                else
                {
                    DrawBoxIfEnabled("Ground Box", groundBox, GroundColor);
                    DrawBoxIfEnabled("Body Box", bodyBox, BodyColor);
                    DrawBoxIfEnabled("Sky Box", skyBox, SkyColor);
                }
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Can Be Hit is off. Ground / Body / Sky hit boxes are hidden in Inspector and ignored by damage queries.", MessageType.Info);
        }

        // Attack Box
        {
            bool isCustomAttack = body.preset == ZCollisionPreset.Custom;
            bool showAttack = isCustomAttack || attackBox.FindPropertyRelative("enabled").boolValue;
            if (showAttack)
            {
                attackFoldout = EditorGUILayout.Foldout(attackFoldout, "Attack Box", true);
                if (attackFoldout)
                {
                    EditorGUI.indentLevel++;
                    if (isCustomAttack)
                        DrawBoxWithToggle("Attack Box", attackBox, AttackColor);
                    else
                        DrawBoxIfEnabled("Attack Box", attackBox, AttackColor);
                    EditorGUI.indentLevel--;
                }
            }
        }

        if (canBeSelected.boolValue)
        {
            selectFoldout = EditorGUILayout.Foldout(selectFoldout, "Select Box", true);
            if (selectFoldout)
            {
                EditorGUI.indentLevel++;
                DrawBoxWithToggle("Select Box", selectBox, SelectColor);
                EditorGUILayout.HelpBox("Select fallback: selectBox -> bodyBox -> groundBox -> skyBox.", MessageType.None);
                EditorGUI.indentLevel--;
            }
        }

        if (canBeInteracted.boolValue)
        {
            interactFoldout = EditorGUILayout.Foldout(interactFoldout, "Interact Box", true);
            if (interactFoldout)
            {
                EditorGUI.indentLevel++;
                DrawBoxWithToggle("Interact Box", interactBox, InteractColor);
                EditorGUILayout.HelpBox("Interact fallback: interactBox -> bodyBox -> groundBox -> skyBox. Interact uses box overlap.", MessageType.None);
                EditorGUI.indentLevel--;
            }
        }

        debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug", true);
        if (debugFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(drawMode, new GUIContent("Draw Mode"));
            EditorGUILayout.PropertyField(drawOccupiedCells, new GUIContent("Draw Occupied Cells"));
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Registered", body.registered);
                EditorGUILayout.ObjectField("World", body.World, typeof(ZCollisionWorld2D), true);
                EditorGUILayout.RectField("Cached Bounds", body.CachedBounds);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Sync To World"))
            {
                Undo.RecordObject(body, "Sync Collision Body");
                body.SyncToWorld();
                EditorUtility.SetDirty(body);
            }
            if (GUILayout.Button("Register"))
            {
                if (ZCollisionWorld2D.Instance != null)
                    ZCollisionWorld2D.Instance.Register(body);
            }
            if (GUILayout.Button("Unregister"))
            {
                if (body.World != null)
                    body.World.Unregister(body);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void BeginColorBox(Color color)
    {
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = oldColor;
    }

    private void EndColorBox()
    {
        EditorGUILayout.EndVertical();
    }

    private void DrawBoxWithToggle(string label, SerializedProperty boxProp, Color color)
    {
        BeginColorBox(color);

        SerializedProperty enabledProp = boxProp.FindPropertyRelative("enabled");
        EditorGUILayout.PropertyField(enabledProp, new GUIContent(label));

        if (enabledProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(boxProp.FindPropertyRelative("offset"), new GUIContent("Offset"));
            EditorGUILayout.PropertyField(boxProp.FindPropertyRelative("size"), new GUIContent("Size"));
            EditorGUI.indentLevel--;
        }

        EndColorBox();
    }

    private void DrawBoxIfEnabled(string label, SerializedProperty boxProp, Color color)
    {
        SerializedProperty enabledProp = boxProp.FindPropertyRelative("enabled");

        if (!enabledProp.boolValue)
            return;

        BeginColorBox(color);

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(boxProp.FindPropertyRelative("offset"), new GUIContent("Offset"));
        EditorGUILayout.PropertyField(boxProp.FindPropertyRelative("size"), new GUIContent("Size"));
        EditorGUI.indentLevel--;

        EndColorBox();
    }

    private void OnSceneGUI()
    {
        ZCollisionBody2D body = (ZCollisionBody2D)target;
        if (body == null || body.drawMode == ZBodyDrawMode.None)
            return;

        bool selectedOnly = body.drawMode == ZBodyDrawMode.SelectedOnly;
        if (selectedOnly && Selection.activeGameObject != body.gameObject)
            return;

        serializedObject.Update();

        if (body.canBeHit)
        {
            DrawBoxHandle(body, groundBox, ZBoxType.Ground, new Color(0.2f, 1f, 0.2f, 0.85f));
            DrawBoxHandle(body, bodyBox, ZBoxType.Body, new Color(0.25f, 0.65f, 1f, 0.85f));
            DrawBoxHandle(body, skyBox, ZBoxType.Sky, new Color(0.75f, 0.25f, 1f, 0.85f));
        }

        // Attack box
        DrawBoxHandle(body, attackBox, ZBoxType.Attack, new Color(1f, 0.15f, 0.15f, 0.85f));

        if (body.canBeSelected)
            DrawBoxHandle(body, selectBox, ZBoxType.Select, new Color(1f, 0.9f, 0.2f, 0.85f));

        if (body.canBeInteracted)
            DrawBoxHandle(body, interactBox, ZBoxType.Interact, new Color(1f, 0.45f, 0.1f, 0.85f));

        if (body.drawOccupiedCells && body.World != null && body.World.TryGetOccupiedCells(body, out var cells))
        {
            Handles.color = new Color(1f, 1f, 1f, 0.25f);
            for (int i = 0; i < cells.Count; i++)
            {
                Rect r = body.World.GetCellRect(cells[i]);
                Handles.DrawWireCube(r.center, r.size);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBoxHandle(ZCollisionBody2D body, SerializedProperty boxProp, ZBoxType type, Color color)
    {
        SerializedProperty enabled = boxProp.FindPropertyRelative("enabled");
        if (!enabled.boolValue)
            return;

        if (!body.TryGetWorldBox(type, out Rect rect))
            return;

        SerializedProperty offsetProp = boxProp.FindPropertyRelative("offset");
        SerializedProperty sizeProp = boxProp.FindPropertyRelative("size");

        Handles.color = color;
        Handles.DrawSolidRectangleWithOutline(rect, new Color(color.r, color.g, color.b, 0.06f), color);

        Vector3 center = rect.center;
        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.FreeMoveHandle(center, Quaternion.identity, HandleUtility.GetHandleSize(center) * 0.06f, Vector3.zero, Handles.DotHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(body, "Move Collision Box");
            Vector3 delta = newCenter - center;
            Vector3 scale3 = body.transform.lossyScale;
            Vector2 offset = offsetProp.vector2Value;
            if (Mathf.Abs(scale3.x) > 0.0001f) offset.x += delta.x / Mathf.Abs(scale3.x);
            if (Mathf.Abs(scale3.y) > 0.0001f) offset.y += delta.y / Mathf.Abs(scale3.y);
            offsetProp.vector2Value = offset;
            serializedObject.ApplyModifiedProperties();
            body.SyncToWorld();
            serializedObject.Update();
            MarkCustom(body);
        }

        Vector3 right = new Vector3(rect.xMax, rect.center.y, 0f);
        Vector3 top = new Vector3(rect.center.x, rect.yMax, 0f);

        EditorGUI.BeginChangeCheck();
        Vector3 newRight = Handles.Slider(right, Vector3.right, HandleUtility.GetHandleSize(right) * 0.05f, Handles.CubeHandleCap, 0f);
        Vector3 newTop = Handles.Slider(top, Vector3.up, HandleUtility.GetHandleSize(top) * 0.05f, Handles.CubeHandleCap, 0f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(body, "Resize Collision Box");
            Vector2 size = sizeProp.vector2Value;
            Vector3 scale3 = body.transform.lossyScale;
            float sx = Mathf.Abs(scale3.x) > 0.0001f ? Mathf.Abs(scale3.x) : 1f;
            float sy = Mathf.Abs(scale3.y) > 0.0001f ? Mathf.Abs(scale3.y) : 1f;
            size.x = Mathf.Max(0.001f, (newRight.x - rect.xMin) / sx);
            size.y = Mathf.Max(0.001f, (newTop.y - rect.yMin) / sy);
            sizeProp.vector2Value = size;
            serializedObject.ApplyModifiedProperties();
            body.SyncToWorld();
            serializedObject.Update();
            MarkCustom(body);
        }
    }

    private static void MarkCustom(ZCollisionBody2D body)
    {
        if (body.preset != ZCollisionPreset.Custom)
        {
            body.preset = ZCollisionPreset.Custom;
            EditorUtility.SetDirty(body);
        }
    }
}
#endif
