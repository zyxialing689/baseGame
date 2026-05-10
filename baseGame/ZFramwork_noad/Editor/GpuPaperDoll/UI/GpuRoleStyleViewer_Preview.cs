using UnityEditor;
using UnityEngine;

public partial class GpuRoleStyleViewer
{
    private Vector2 _previewDrag;

    private void DrawPreviewArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(400), GUILayout.ExpandHeight(true));

        GUILayout.Label("Preview", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Shadow", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _core.UseShadow = EditorGUILayout.Toggle("Use Shadow", _core.UseShadow);
        using (new EditorGUI.DisabledScope(!_core.UseShadow))
        {
            _core.ShadowOffset = EditorGUILayout.Vector2Field("Offset", _core.ShadowOffset);
            _core.ShadowSize = EditorGUILayout.Vector2Field("Size", _core.ShadowSize);
            _core.ShadowColor = EditorGUILayout.ColorField("Color", _core.ShadowColor);
        }
        if (EditorGUI.EndChangeCheck())
        {
            AutoSave();
            RebuildPreview();
            Repaint();
        }

        if (GUILayout.Button("Open GPU Export Inspector", GUILayout.Height(24)))
        {
            GpuRoleExportInspectorWindow.Open(_core);
        }

        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (rect.width < 10) rect.width = 380;
        if (rect.height < 10) rect.height = 400;
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

        if (!_core.HasData)
        {
            GUI.Label(rect, "Assign a prefab and load slots.");
        }
        else if (_renderer == null || !_renderer.HasMainPreview)
        {
            GUI.Label(rect, "Click 'Load From Prefab'.");
        }
        else
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition) && e.button == 0)
            {
                _previewDrag += e.delta * 0.01f;
                e.Use();
                Repaint();
            }

            Texture tex = _renderer.RenderMainPreview(rect, ref _previewDrag);
            if (tex != null)
                GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);
        }

        EditorGUILayout.EndVertical();
    }

    private void RebuildPreview()
    {
        _renderer?.CleanupAll();
        if (_core.HasData)
            _renderer?.BuildMainPreview(
                _core.SlotDefinitions,
                _core.StyleSlots,
                _core.RootPosition,
                _core.RootRotation,
                _core.RootScale,
                _core.UseShadow,
                _core.ShadowOffset,
                _core.ShadowSize,
                _core.ShadowColor
            );
    }
}
