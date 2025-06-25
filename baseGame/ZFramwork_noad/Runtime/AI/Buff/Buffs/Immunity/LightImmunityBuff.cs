using System;
using System.Collections;
using UnityEngine;
using UnityTimer;

public class LightImmunityBuff : BaseBuff
{
    private EffectData effectData;
    public override void InitEffect()
    {
        effectData = new EffectData(Vector3.zero, effect_path, false);
        agent.ShowEffect(effectData);
    }


    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        agent.HideEffect(effectData);
    }
}