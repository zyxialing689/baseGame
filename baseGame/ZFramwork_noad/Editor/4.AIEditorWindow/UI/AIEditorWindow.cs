using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AIEditorWindow : EditorWindow
{
    private List<AINodeView> nodes = new List<AINodeView>();
    private List<AIConnectionView> connections = new List<AIConnectionView>();
    [SerializeField]
    private TextAsset currentText;
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
    private void OnDisable()
    {
        AIDebugger.onUpdate -= OnAIDebugUpdate;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }
    private void OnEnable()
    {
        AIDebugger.onUpdate += OnAIDebugUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        // ⭐ 优先恢复之前的
        if (currentText != null)
        {
            LoadFromTextAsset(currentText);
        }
        else
        {
            TryLoadFromSelection();
        }
    }
    private void OnSelectionChange()
    {
        TryLoadFromSelection(); // 有就加载，没有就保持
        Repaint();
    }
    private void OnAIDebugUpdate()
    {
        var agent = AIDebugger.currentAgent;

        if (agent == null)
            return;

        if (!AIDebugger.debugData.TryGetValue(agent, out var data))
            return;

        int current = data.currentStateId;
        int next = data.nextStateId;

        // 清空
        foreach (var n in nodes)
            n.isActive = false;

        foreach (var c in connections)
            c.isActive = false;

        // 节点高亮
        foreach (var n in nodes)
        {
            if (n.id == current)
                n.isActive = true;
        }

        // 连线高亮
        foreach (var c in connections)
        {
            if (next != -1 && c.from.id == current && c.to.id == next)
            {
                c.isActive = true;
            }
        }

        Repaint();
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
            // 画线
            Handles.DrawBezier(
                selectedOutNode.outPoint.center,
                e.mousePosition,
                selectedOutNode.outPoint.center + Vector2.right * 80,
                e.mousePosition + Vector2.left * 80,
                Color.yellow,
                null,
                3f
            );

            // ⭐ 提示文字（放这里）
            GUI.Label(
                new Rect(e.mousePosition.x + 15, e.mousePosition.y, 100, 20),
                "ESC取消"
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

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
        {
            DeleteSelection();
            e.Use();
        }
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            selectedOutNode = null;
            selectedInNode = null;
            GUI.changed = true;
        }
    }
    private void DeleteSelection()
    {
        // 删除选中节点
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (nodes[i].IsSelected())
            {
                var node = nodes[i];

                // 删除相关连线
                connections.RemoveAll(c => c.from == node || c.to == node);

                nodes.RemoveAt(i);
            }
        }

        // 2️⃣ 删除选中连线（⭐ 就放这里）
        connections.RemoveAll(c => c.isSelected);
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
                stateType = node.stateType,
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
                conditionType = (int)conn.conditionType, // ⭐
                param = conn.param                       // ⭐
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
            node.stateType = n.stateType;

            nodes.Add(node);
            map[n.id] = node;

            idCounter = Mathf.Max(idCounter, n.id + 1);
        }

        foreach (var c in data.connections)
        {
            if (map.TryGetValue(c.fromNodeId, out var from) &&
                map.TryGetValue(c.toNodeId, out var to))
            {
                var view = new AIConnectionView(from, to);

                view.conditionType = (AIConditionType)c.conditionType; // ⭐
                view.param = c.param;                                  // ⭐

                connections.Add(view);
            }
        }

        Debug.Log("加载成功: " + path);
    }
    #region 右键菜单

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        if (selectedOutNode != null)
        {
            menu.AddItem(new GUIContent("Create Node (Connect)"), false, () =>
            {
                var newNode = new AINodeView(idCounter++, mousePosition);
                nodes.Add(newNode);

                connections.Add(new AIConnectionView(selectedOutNode, newNode));

                selectedOutNode = null;
            });
        }
        else
        {
            menu.AddItem(new GUIContent("Add Node"), false, () => AddNode(mousePosition));
        }

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
    private void TryLoadFromSelection()
    {
        var go = Selection.activeGameObject;

        if (go == null)
            return; // ⭐ 不清空，不动现有UI

        var provider = go.GetComponent<IAIProvider>();

        if (provider == null)
            return; // ⭐ 同样不清空

        var text = provider.GetAIAsset();
        var agent = provider.GetAgent();

        if (text == null)
            return;

        // ⭐⭐⭐ 只有在“有效数据”时才更新
        AIDebugger.currentAgent = agent;

        if (text != currentText)
        {
            currentText = text;
            LoadFromTextAsset(text);
        }
    }
    public void LoadFromTextAsset(TextAsset text)
    {
        if (text == null) return;

        AIEditorData data = JsonUtility.FromJson<AIEditorData>(text.text);

        nodes.Clear();
        connections.Clear();

        Dictionary<int, AINodeView> map = new Dictionary<int, AINodeView>();

        foreach (var n in data.nodes)
        {
            var node = new AINodeView(n.id, new Vector2(n.x, n.y));
            node.stateType = (AIStateType)n.stateType;

            nodes.Add(node);
            map[n.id] = node;
        }

        foreach (var c in data.connections)
        {
            if (map.TryGetValue(c.fromNodeId, out var from) &&
                map.TryGetValue(c.toNodeId, out var to))
            {
                var view = new AIConnectionView(from, to);
                view.conditionType = (AIConditionType)c.conditionType;
                view.param = c.param;

                connections.Add(view);
            }
        }

        Repaint();
    }
    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // ⭐ Play后再尝试加载一次
            TryLoadFromSelection();
        }
    }

}