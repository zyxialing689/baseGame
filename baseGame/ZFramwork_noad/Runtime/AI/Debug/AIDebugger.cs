using System.Collections.Generic;

public static class AIDebugger
{
    public static Dictionary<AIAgent, AIDebugData> debugData
        = new Dictionary<AIAgent, AIDebugData>();

    public static AIAgent currentAgent; // ⭐ 当前选中的AI

    public static System.Action onUpdate;
}