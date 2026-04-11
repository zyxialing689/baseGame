using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AIEditorWindow : EditorWindow
{
    private List<AINodeView> nodes = new List<AINodeView>();
    private List<AIConnectionView> connections = new List<AIConnectionView>();

    private AINodeView selectedOutNode;
    private AINodeView selectedInNode;
    private Vector2 offset;
    private Vector2 drag;

    private int idCounter = 0;
    private const string LastDirKey = "AIEditor_LastDirectory";

    [MenuItem("ZFramework/Window/AI Editor")]
    public static void Open()
    {
        GetWindow<AIEditorWindow>("AI Editor");
    }

    private void OnGUI()
    {
        UpdateNodeConnectionState();
        DrawToolbar();
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawNodes();
        DrawConnections();
        DrawConnectionLine(Event.current);
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);
        if (GUI.changed)
            Repaint();
    }

    #region 绘制

    private void UpdateNodeConnectionState()
    {
        // 先全部清空
        foreach (var node in nodes)
        {
            node.hasInConnection = false;
            node.hasOutConnection = false;
        }

        // 遍历所有连接
        foreach (var conn in connections)
        {
            if (conn.from != null)
            {
                conn.from.hasOutConnection = true;
            }

            if (conn.to != null)
            {
                conn.to.hasInConnection = true;
            }
        }
    }
    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("保存", EditorStyles.toolbarButton))
        {
            Save();
        }

        if (GUILayout.Button("加载", EditorStyles.toolbarButton))
        {
            Load();
        }

        GUILayout.EndHorizontal();
    }
    private void DrawNodes()
    {
        foreach (var node in nodes)
        {
            node.Draw();
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
    private void DrawConnections()
    {
        foreach (var conn in connections)
        {
            conn.Draw();
        }
    }
    private void DrawConnectionLine(Event e)
    {
        if (selectedOutNode != null)
        {
            Handles.DrawBezier(
                selectedOutNode.outPoint.center,
                e.mousePosition,
                selectedOutNode.outPoint.center + Vector2.right * 80,
                e.mousePosition + Vector2.left * 80,
                Color.yellow,
                null,
                3f
            );

            GUI.changed = true;
        }
    }
    #endregion

    #region 事件

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            ShowContextMenu(e.mousePosition);
        }

        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            OnDrag(e.delta);
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            var node = nodes[i];

            // 点击输出点
            if (e.type == EventType.MouseDown && node.outPoint.Contains(e.mousePosition))
            {
                selectedOutNode = node;
                e.Use();
            }

            // 点击输入点
            if (e.type == EventType.MouseDown && node.inPoint.Contains(e.mousePosition))
            {
                selectedInNode = node;
                TryCreateConnection();
                e.Use();
            }

            if (node.ProcessEvents(e))
            {
                GUI.changed = true;
            }
        }
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        foreach (var node in nodes)
        {
            node.Drag(delta);
        }

        GUI.changed = true;
    }
    private void TryCreateConnection()
    {
        if (selectedOutNode != null && selectedInNode != null)
        {
            connections.Add(new AIConnectionView(selectedOutNode, selectedInNode));
        }

        selectedOutNode = null;
        selectedInNode = null;
    }
    #endregion
    private void Save()
    {
        string dir = GetLastDir();

        string path = EditorUtility.SaveFilePanel(
            "保存AI图",
            dir, // ⭐ 用上次目录
            "AIEditorData.json",
            "json"
        );

        if (string.IsNullOrEmpty(path)) return;

        SetLastDir(path); // ⭐ 记住目录

        AIEditorData data = new AIEditorData();

        foreach (var node in nodes)
        {
            data.nodes.Add(new AIEditorNode
            {
                id = node.id,
                stateType = (int)node.stateType,
                x = node.rect.x,
                y = node.rect.y
            });
        }

        foreach (var conn in connections)
        {
            data.connections.Add(new AIEditorConnection
            {
                fromNodeId = conn.from.id,
                toNodeId = conn.to.id,
                conditionType = (int)conn.conditionType   // ⭐ 就是这行
            });
        }

        string json = JsonUtility.ToJson(data, true);

        System.IO.File.WriteAllText(path, json);

        Debug.Log("保存成功: " + path);
    }
    private void Load()
    {
        string dir = GetLastDir();

        string path = EditorUtility.OpenFilePanel(
            "加载AI图",
            dir, // ⭐ 用上次目录
            "json"
        );

        if (string.IsNullOrEmpty(path)) return;

        SetLastDir(path); // ⭐ 记住目录

        string json = System.IO.File.ReadAllText(path);

        AIEditorData data = JsonUtility.FromJson<AIEditorData>(json);

        nodes.Clear();
        connections.Clear();

        Dictionary<int, AINodeView> map = new Dictionary<int, AINodeView>();

        foreach (var n in data.nodes)
        {
            var node = new AINodeView(n.id, new Vector2(n.x, n.y));
            node.stateType = (AIStateType)n.stateType;

            nodes.Add(node);
            map[n.id] = node;

            idCounter = Mathf.Max(idCounter, n.id + 1);
        }

        foreach (var c in data.connections)
        {
            if (map.TryGetValue(c.fromNodeId, out var from) &&
                map.TryGetValue(c.toNodeId, out var to))
            {
                connections.Add(new AIConnectionView(from, to));
            }
        }

        Debug.Log("加载成功: " + path);
    }
    #region 右键菜单

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Add Node"), false, () => AddNode(mousePosition));

        menu.ShowAsContext();
    }

    private void AddNode(Vector2 position)
    {
        nodes.Add(new AINodeView(idCounter++, position));
    }

    #endregion

    ///////////////////////////

    private string GetLastDir()
    {
        return EditorPrefs.GetString(LastDirKey, Application.dataPath);
    }
    private void SetLastDir(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        string dir = System.IO.Path.GetDirectoryName(path);
        EditorPrefs.SetString(LastDirKey, dir);
    }
}