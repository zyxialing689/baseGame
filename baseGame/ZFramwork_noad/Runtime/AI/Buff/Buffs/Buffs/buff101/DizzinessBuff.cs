using System;
using System.Collections;
using UnityEngine;
using UnityTimer;

public class DizzinessBuff : BaseBuff
{
    private EffectData effectData;
    public override void InitEffect()
    {
        effectData = new EffectData(new Vector3(0, agent.aICollider.bodyBox.size.y, 0), effect_path);
 
        agent.animator.SetBool("dizz", true);
        agent.isDizz = true;
        agent.ShowEffect(effectData);
        agent.ResetState();
    }



    public override void RemoveBuffEffect()
    {
        base.RemoveBuffEffect();
        agent.agentTempData.stateMoveSpeed = 1;
        agent.animator.SetBool("dizz", false);
        agent.HideEffect(effectData);
        agent.isDizz = false;
    }
}