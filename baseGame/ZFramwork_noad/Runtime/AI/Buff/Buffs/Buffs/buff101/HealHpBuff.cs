using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class HealHpBuff : BaseBuff
{
    EffectData effectData;
    public override void InitEffect()
    {
        effectData = new EffectData(Vector3.up * agent.aICollider.bodyBox.size.y * 0.5f, effect_path);
        agent.ShowEffect(effectData);
    }

    public override void TriggerHurt()
    {
        agent.attrData.ChangeHp(3, null, agent.aICollider.GetHurtPos(), HurtType.BUFF, false);
    }

    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        agent.HideEffect(effectData);
    }
}