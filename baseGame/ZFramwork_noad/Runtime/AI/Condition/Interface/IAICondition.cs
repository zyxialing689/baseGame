using UnityEngine;

public interface IAICondition
{
    bool Check(AIAgent agent, Vector4 param);
    
}