using System.Collections.Generic;

public static class AIEditorToRuntimeConverter
{
    public static List<AIStateConfig> Convert(AIEditorData data)
    {
        var result = new List<AIStateConfig>();

        Dictionary<int, AIStateConfig> map = new Dictionary<int, AIStateConfig>();

        // 创建状态
        foreach (var node in data.nodes)
        {
            var cfg = new AIStateConfig
            {
                id = node.id,
                stateType = node.stateType
            };

            map[node.id] = cfg;
            result.Add(cfg);
        }

        // 处理连接
        foreach (var conn in data.connections)
        {
            if (map.TryGetValue(conn.fromNodeId, out var from))
            {
                from.transitions.Add(new AITransition
                {
                    targetStateId = conn.toNodeId,
                    conditionType = (AIConditionType)conn.conditionType,
                    param = conn.param // ⭐
                });
            }
        }

        return result;
    }

    private static string ConvertType(AIStateType type)
    {
        switch (type)
        {
            case AIStateType.Idle: return "Idle";
            case AIStateType.Move: return "Move";
            case AIStateType.Attack: return "Attack";
            default: return "Idle";
        }
    }
}