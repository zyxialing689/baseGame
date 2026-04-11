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
                stateType = ConvertType(node.stateType)
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
                    conditionType = (AIConditionType)conn.conditionType
                });
            }
        }

        return result;
    }

    private static string ConvertType(int type)
    {
        switch (type)
        {
            case 2000: return "Idle";
            case 3000: return "Move";
            default: return "Idle";
        }
    }
}