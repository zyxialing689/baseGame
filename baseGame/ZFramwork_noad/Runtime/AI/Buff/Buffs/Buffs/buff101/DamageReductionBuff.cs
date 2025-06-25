using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class DamageReductionBuff: BaseBuff
{
    EffectData effectData;
    public override void InitEffect()
    {
        effectData = new EffectData(Vector3.up*agent.aICollider.bodyBox.size.y*0.5f, effect_path);
        agent.ShowEffect(effectData);
    }
    public override int TriggerBeHurt(int chp,AgentSkill agentSkill, HurtType hurtType)
    {
        if(chp<0)
        chp = Mathf.CeilToInt(chp * 0.1f);
        return chp;
    }

    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        agent.HideEffect(effectData);
    }
}