using UnityEditor;
using UnityEngine;

public class AINodeView
{
    public Rect rect;
    public int id;
    public AIStateType stateType;

    public Rect outPoint;
    public Rect inPoint;

    private bool isDragging;
    private bool isSelected;
    public bool hasInConnection;
    public bool hasOutConnection;
    public bool isActive;
    public AINodeView(int id, Vector2 position)
    {
        this.id = id;
        rect = new Rect(position.x, position.y, 180, 80);

        stateType = AIStateType.Idle;
    }

    public void Draw()
    {
        DrawBackground();
        DrawHeader();
        DrawContent();
        DrawConnectionPoints();
        DrawSelection();
    }

    #region 绘制

    private void DrawBackground()
    {
        // 主背景（深色卡片）
        EditorGUI.DrawRect(rect, new Color(0.22f, 0.22f, 0.22f));

        // 边框
        Handles.color = new Color(0, 0, 0, 0.6f);
        Handles.DrawAAPolyLine(2,
            new Vector3(rect.x, rect.y),
            new Vector3(rect.x + rect.width, rect.y),
            new Vector3(rect.x + rect.width, rect.y + rect.height),
            new Vector3(rect.x, rect.y + rect.height),
            new Vector3(rect.x, rect.y)
        );
        Handles.color = Color.white;
    }

    private void DrawHeader()
    {
        Rect header = new Rect(rect.x, rect.y, rect.width, 22);

        // ⭐ 只看运行状态
        Color color = isActive
            ? new Color(0.2f, 0.8f, 0.2f)   // 绿色
            : new Color(0.3f, 0.3f, 0.3f);  // 灰色

        EditorGUI.DrawRect(header, color);

        GUI.Label(
            new Rect(rect.x + 6, rect.y + 2, rect.width, 20),
            $"State {id}",
            EditorStyles.boldLabel
        );
    }

    private void DrawContent()
    {
        // 状态下拉框
        stateType = (AIStateType)EditorGUI.EnumPopup(
            new Rect(rect.x + 10, rect.y + 30, rect.width - 20, 20),
            stateType
        );
    }

    private void DrawConnectionPoints()
    {
        float size = 6f;

        // 输入点（左）
        Vector2 inCenter = new Vector2(rect.x, rect.center.y);
        inPoint = new Rect(inCenter.x - size, inCenter.y - size, size * 2, size * 2);

        Handles.color = hasInConnection ? Color.green : Color.red;
        Handles.DrawSolidDisc(inCenter, Vector3.forward, size);

        // 输出点（右）
        Vector2 outCenter = new Vector2(rect.x + rect.width, rect.center.y);
        outPoint = new Rect(outCenter.x - size, outCenter.y - size, size * 2, size * 2);

        Handles.color = hasOutConnection ? Color.green : Color.red;
        Handles.DrawSolidDisc(outCenter, Vector3.forward, size);

        Handles.color = Color.white;
    }

    private void DrawSelection()
    {
        if (!isSelected) return;

        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(3,
            new Vector3(rect.x, rect.y),
            new Vector3(rect.x + rect.width, rect.y),
            new Vector3(rect.x + rect.width, rect.y + rect.height),
            new Vector3(rect.x, rect.y + rect.height),
            new Vector3(rect.x, rect.y)
        );
        Handles.color = Color.white;
    }

    #endregion

    #region 状态颜色


    #endregion

    #region 交互

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:

                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isSelected = true;
                        isDragging = true;
                        GUI.changed = true;
                    }
                    else
                    {
                        isSelected = false;
                    }
                }

                break;

            case EventType.MouseUp:
                isDragging = false;
                break;

            case EventType.MouseDrag:

                if (e.button == 0 && isDragging)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }

                break;
        }

        return false;
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    #endregion
}