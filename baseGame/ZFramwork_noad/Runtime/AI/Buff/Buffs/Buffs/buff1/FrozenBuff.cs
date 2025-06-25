using System;
using System.Collections;
using UnityEngine;
using UnityTimer;

public class FrozenBuff : BaseBuff
{
    private float curSpeed;
    public override void InitEffect()
    {
        curSpeed = 0.5f;
        agent.agentTempData.iceSpeed = curSpeed;
        //agent.isFrozen = true;
        //agent.animator.enabled = false;
    }

    public override void UpdateEffect()
    {
        curSpeed -= 0.05f;
        if (curSpeed < 0)
        {
            curSpeed = 0;
        }
        agent.agentTempData.iceSpeed = curSpeed;
    }

    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        //agent.agentTempData.stateMoveSpeed = 1;
        //agent.animator.enabled = true;
        //agent.isFrozen = false;
        agent.agentTempData.iceSpeed =1f;
        agent.ResetSpeed();
    }

}