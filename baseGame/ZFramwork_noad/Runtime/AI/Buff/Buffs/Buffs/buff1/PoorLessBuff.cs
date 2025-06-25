using System;
using System.Collections;
using UnityEngine;
using UnityTimer;

public class PoorLessBuff : BaseBuff
{
    public override void InitEffect()
    {
        agent.agentTempData.stateMoveSpeed = 0.3f;
    }


    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        agent.agentTempData.stateMoveSpeed = 1;
    }

}