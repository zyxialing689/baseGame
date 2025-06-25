using System;
using System.Collections;
using UnityEngine;
using UnityTimer;

public class BleedingBuff : BaseBuff
{
    int hurt = -1;
    private EffectData effectData;

    public override void InitEffect()
    {
        base.InitEffect();
        effectData = new EffectData(new Vector3(0, agent.aICollider.bodyBox.size.y / 3f, 0), effect_path);
        agent.ShowEffect(effectData);
        hurt = -1;
    }
    public override void TriggerHurt()
    {
 
        agent.attrData.ChangeHp(hurt,null, agent.aICollider.GetHurtPos(),HurtType.BUFF);

    }

    public override void UpdateEffect()
    {
        if (hurt > -9)
        {
            hurt--;
        }
        base.UpdateEffect();
    }

    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        agent.HideEffect(effectData);
    }

}