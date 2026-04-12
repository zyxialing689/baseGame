using UnityEditor;
using UnityEngine;

public class AIConnectionView
{
    public bool isSelected;
    public Vector4 param;
    public AINodeView from;
    public AINodeView to;
    public AIConditionType conditionType = AIConditionType.True;
    public bool isActive;
    public AIConnectionView(AINodeView from, AINodeView to)
    {
        this.from = from;
        this.to = to;
    }

    public void Draw()
    {
        if (from == null || to == null) return;

        Color lineColor = isActive ? Color.green : new Color(1f, 1f, 1f, 0.6f);

        Handles.DrawBezier(
            from.outPoint.center,
            to.inPoint.center,
            from.outPoint.center + Vector2.right * 80,
            to.inPoint.center + Vector2.left * 80,
            lineColor,
            null,
            isActive ? 6f : 3f
        );

        // 中点
        Vector2 mid = (from.outPoint.center + to.inPoint.center) / 2;

        // 往上偏一点
        mid.y -= 10;

        // 主区域
        Rect rect = new Rect(mid.x - 55, mid.y - 10, 110, 20);

        // 背景
        EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.4f));

        // 样式
        GUIStyle style = new GUIStyle(EditorStyles.popup);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;

        // Condition选择
        conditionType = (AIConditionType)EditorGUI.EnumPopup(rect, conditionType, style);
        // ⭐ 参数区域（只在需要时显示）
        if (AIConditionDrawer.HasParam(conditionType))
        {
            Rect paramRect = new Rect(mid.x - 40, mid.y + 12, 80, 18);

            EditorGUI.DrawRect(paramRect, new Color(0, 0, 0, 0.4f));

            AIConditionDrawer.Draw(conditionType, ref param, paramRect);
        }
        if (Event.current.type == EventType.MouseDown)
        {
            float dist = HandleUtility.DistancePointBezier(
                Event.current.mousePosition,
                from.outPoint.center,
                to.inPoint.center,
                from.outPoint.center + Vector2.right * 80,
                to.inPoint.center + Vector2.left * 80
            );

            if (dist < 10f)
            {
                isSelected = true;
                GUI.changed = true;
            }
            else
            {
                isSelected = false;
            }
        }
    }
}