using System;
using System.Collections;
using UnityEngine;
using UnityTimer;

public class LightBuff : BaseBuff
{
    public override void TriggerHurt()
    {
        int hurt = -1;
        agent.attrData.ChangeHp(hurt,null, agent.aICollider.GetHurtPos(), HurtType.BUFF);
    }


}