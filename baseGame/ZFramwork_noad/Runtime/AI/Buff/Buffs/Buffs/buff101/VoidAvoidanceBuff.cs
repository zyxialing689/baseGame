using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class VoidAvoidanceBuff : BaseBuff
{

    private float lateTime;
    private bool onceTrigger;
    public override void InitEffect()
    {
        onceTrigger = false;

        lateTime = 0;
    }
    public override int TriggerBeHurt(int chp,AgentSkill agentSkill,HurtType hurtType)
    {
        if (chp < 0&&RandomMgr.GetValue() < 0.2f&& hurtType == HurtType.SKILL)
        {
            //DamageNumberMgr._instance.Spawn(agent.aICollider.GetHurtPos(), "miss");
            AudioManager._instance.PlayNewSound("__Sounds/dagger/dagger_flash", agent.aICollider.GetHurtPos());
            agent.agentTempData.isAttacking = false;
            agent.SetInvisible(false);
            lateTime = 0.2f;
            onceTrigger = true;
            return 0;
  
        }
        return chp;
    }


    public override void _Update()
    {
        base._Update();
        if (lateTime > 0)
        {
            lateTime -= Time.deltaTime;
        }
        else
        {
            if (onceTrigger)
            {
                agent.SetInvisible(true);

            }
            onceTrigger = false;
        }
  
    }
}