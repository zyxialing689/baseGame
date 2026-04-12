using UnityEngine;

public class TimeCondition : IAICondition
{
    public bool Check(AIAgent agent, Vector4 param)
    {
        return agent.StateTime >= param.x;
    }
}