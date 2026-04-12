using UnityEditor;
using UnityEngine;

public static class AIConditionDrawer
{
    //专属绘制
    public static void Draw(AIConditionType type, ref Vector4 param, Rect rect)
    {
        GUIStyle style = new GUIStyle(EditorStyles.numberField);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;

        switch (type)
        {
            case AIConditionType.True:
                EditorGUI.LabelField(rect, "True");
                break;

            case AIConditionType.WaitTime:
                param.x = EditorGUI.FloatField(rect, param.x, style);
                break;

            case AIConditionType.Distance:
                param.x = EditorGUI.FloatField(rect, param.x, style);
                break;
        }
    }
    //有没有参数
    public static bool HasParam(AIConditionType type)
    {
        switch (type)
        {
            case AIConditionType.True:
                return false;

            case AIConditionType.WaitTime:
            case AIConditionType.Distance:
                return true;
        }

        return false;
    }
}