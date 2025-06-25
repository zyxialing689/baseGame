using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICommonFunc
{
    public static NodeType GetNodeTypeByAIState(AIStateType aIState)
    {
        int index = (int)aIState;

        if (index >= 1000 && index < 2000)
        {
            return NodeType.compose;
        }
        if (index >= 2000 && index < 3000)
        {
            return NodeType.idle;
        }
        if (index >= 3000 && index < 4000)
        {
            return NodeType.finding;
        }
        if (index >= 4000 && index < 5000)
        {
            return NodeType.attack;
        }
        if (index >= 5000 && index < 6000)
        {
            return NodeType.condition;
        }

        return NodeType.root;
    }
}
