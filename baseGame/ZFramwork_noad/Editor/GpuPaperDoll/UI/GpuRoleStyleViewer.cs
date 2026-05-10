using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class GpuRoleStyleViewer : EditorWindow
{
    [SerializeField] private GpuRoleViewerCore _core;
    private GpuRolePreviewRenderer _renderer;
    private Vector2 _scrollPos;
    private readonly List<string> _messages = new List<string>();
    private bool _delayedPreviewRefresh;

    private const string PrefsKey_StyleAssetPath = "GpuRoleStyleViewer_StyleAssetPath";

    [MenuItem("ZFramework/Window/GPU Role")]
    public static void Open()
    {
        GetWindow<GpuRoleStyleViewer>("GPU Role Style Viewer");
    }

    private void OnEnable()
    {
        if (_core == null)
        {
            _core = ScriptableObject.CreateInstance<GpuRoleViewerCore>();
            _core.hideFlags = HideFlags.HideAndDontSave;
            _core.LoadFromEditorPrefs();
        }
        _core.EnsureManagers();

        _renderer = new GpuRolePreviewRenderer();
        if (_core.HasData)
            RebuildPreview();

        string lastAssetPath = EditorPrefs.GetString(PrefsKey_StyleAssetPath, "");
        if (!string.IsNullOrEmpty(lastAssetPath))
        {
            var lastAsset = AssetDatabase.LoadAssetAtPath<GpuRoleStyleData>(lastAssetPath);
            if (lastAsset != null)
                _sourceStyleAsset = lastAsset;
        }
    }

    private void OnDisable()
    {
        AutoSave();
        SaveCurrentStyleAssetPath();
        if (_renderer != null)
        {
            _renderer.CleanupAll();
            _renderer = null;
        }
    }

    private void OnDestroy()
    {
        AutoSave();
    }

    private void AutoSave()
    {
        if (_core != null)
            _core.SaveToEditorPrefs();
    }

    private void SaveCurrentStyleAssetPath()
    {
        if (_sourceStyleAsset == null)
            return;

        string path = AssetDatabase.GetAssetPath(_sourceStyleAsset);
        EditorPrefs.SetString(PrefsKey_StyleAssetPath, path);
    }

    private void OnGUI()
    {
        if (_core == null)
        {
            EditorGUILayout.LabelField("Core data lost. Reopen the window.");
            return;
        }

        if (_renderer == null)
        {
            _renderer = new GpuRolePreviewRenderer();
            if (_core.HasData)
                RebuildPreview();
        }

        if (_delayedPreviewRefresh)
        {
            _delayedPreviewRefresh = false;
            _renderer?.UpdateMainPreview(_core.SlotDefinitions, _core.StyleSlots);
            Repaint();
        }

        EditorGUILayout.BeginHorizontal();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawToolbar();
        DrawGroupManagement();
        DrawSlotList();
        DrawMessages();
        EditorGUILayout.EndScrollView();

        DrawPreviewArea();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawMessages()
    {
        foreach (var m in _messages)
            EditorGUILayout.HelpBox(m, MessageType.Info);
    }

    private void DrawFolderField(string label, string currentPath, System.Action<string> onPathChanged)
    {
        DefaultAsset folderAsset = null;
        if (!string.IsNullOrEmpty(currentPath))
            folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(currentPath);

        EditorGUI.BeginChangeCheck();
        var newAsset = (DefaultAsset)EditorGUILayout.ObjectField(label, folderAsset, typeof(DefaultAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            string path = newAsset != null ? AssetDatabase.GetAssetPath(newAsset) : "";
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
            {
                onPathChanged?.Invoke(path);
                GUI.changed = true;
            }
        }

        Rect dropRect = GUILayoutUtility.GetLastRect();
        Event evt = Event.current;
        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        if (!dropRect.Contains(evt.mousePosition))
            return;

        bool hasFolder = false;
        foreach (var obj in DragAndDrop.objectReferences)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(objPath) && AssetDatabase.IsValidFolder(objPath))
            {
                hasFolder = true;
                break;
            }
        }

        if (!hasFolder)
            return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                string objPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(objPath) && AssetDatabase.IsValidFolder(objPath))
                {
                    onPathChanged?.Invoke(objPath);
                    break;
                }
            }
            GUI.changed = true;
        }
        evt.Use();
    }
}
