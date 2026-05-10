using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class GpuRoleStyleViewer
{
    [SerializeField] private GpuRoleStyleData _sourceStyleAsset;

    private void DrawToolbar()
    {
        GUILayout.Label("GPU Role Style Viewer", EditorStyles.boldLabel);

        if (GUILayout.Button("Open GPU Export Inspector", GUILayout.Height(30)))
        {
            GpuRoleExportInspectorWindow.Open(_core);
        }

        EditorGUI.BeginChangeCheck();
        var newPrefab = (GameObject)EditorGUILayout.ObjectField("Source Prefab", _core.SourcePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            if (newPrefab != null)
            {
                _core.LoadFromPrefab(newPrefab);
                AutoSave();
                RebuildPreview();
            }
            else
            {
                _core.SourcePrefab = null;
                _messages.Add("Source Prefab cleared.");
            }
            Repaint();
        }

        EditorGUI.BeginChangeCheck();
        _sourceStyleAsset = (GpuRoleStyleData)EditorGUILayout.ObjectField("Source Style Asset", _sourceStyleAsset, typeof(GpuRoleStyleData), false);
        if (EditorGUI.EndChangeCheck())
        {
            if (_sourceStyleAsset != null)
            {
                LoadFromStyleAsset(_sourceStyleAsset);
            }
            else
            {
                _messages.Add("Source Style Asset cleared.");
            }
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load From Prefab"))
        {
            _core.LoadFromPrefab(_core.SourcePrefab);
            AutoSave();
            RebuildPreview();
            Repaint();
        }

        if (GUILayout.Button("Random All Groups"))
        {
            RandomizeAllGroups();
            AutoSave();
            Repaint();
        }

        if (GUILayout.Button("Clear All Slots"))
        {
            _core.ClearAllSprites();
            AutoSave();
            _delayedPreviewRefresh = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Style Asset"))
        {
            SaveStyleAsset();
        }

        if (GUILayout.Button("Load Style Asset"))
        {
            if (_sourceStyleAsset == null)
            {
                _messages.Add("No Source Style Asset assigned. Drag a style asset to the field above or use the file picker.");
                return;
            }
            LoadFromStyleAsset(_sourceStyleAsset);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RandomizeAllGroups()
    {
        if (!_core.HasData)
        {
            _messages.Add("No slots loaded.");
            return;
        }

        HashSet<int> done = new HashSet<int>();
        int count = 0;

        for (int i = 0; i < _core.StyleSlots.Count; i++)
        {
            var slot = _core.StyleSlots[i];
            if (slot.linkedGroupId >= 0)
            {
                if (done.Add(slot.linkedGroupId))
                {
                    if (_core.RandomizeLinkedGroup(slot.linkedGroupId))
                    {
                        _core.ApplyGroupExclusive(slot.linkedGroupId);
                        count++;
                    }
                }
            }
            else
            {
                var s = _core.PickRandomSpriteFromFolder(slot.spriteFolder);
                if (s != null)
                {
                    slot.sprite = s;
                    slot.color = Color.white;
                    _core.ApplySlotExclusive(i);
                    count++;
                }
            }
        }

        _messages.Add($"Randomized {count} groups/slots.");
        _delayedPreviewRefresh = true;
    }
}
