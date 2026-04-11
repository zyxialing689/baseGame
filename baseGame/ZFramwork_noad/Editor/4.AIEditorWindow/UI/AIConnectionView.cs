using UnityEditor;
using UnityEngine;

public class AIConnectionView
{
    public AINodeView from;
    public AINodeView to;
    public AIConditionType conditionType = AIConditionType.True;
    public AIConnectionView(AINodeView from, AINodeView to)
    {
        this.from = from;
        this.to = to;
    }

    public void Draw()
    {
        if (from == null || to == null) return;

        // 画线
        Handles.DrawBezier(
            from.outPoint.center,
            to.inPoint.center,
            from.outPoint.center + Vector2.right * 80,
            to.inPoint.center + Vector2.left * 80,
            Color.white,
            null,
            4f
        );

        // 中点
        Vector2 mid = (from.outPoint.center + to.inPoint.center) / 2;

        Rect rect = new Rect(mid.x - 50, mid.y - 10, 100, 20);

        // 🔥 关键：可编辑的下拉框
        conditionType = (AIConditionType)EditorGUI.EnumPopup(rect, conditionType);
    }
}