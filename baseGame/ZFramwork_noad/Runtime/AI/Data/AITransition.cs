using System;
using UnityEngine;

[Serializable]
public class AITransition
{
    public int targetStateId;
    public AIConditionType conditionType;
    public IAICondition condition;

    public Vector4 param; // ⭐ 参数（核心）
}