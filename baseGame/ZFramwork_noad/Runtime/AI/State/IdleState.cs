using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : AIState
{
    public override void Awake()
    {
        agent.animator.SetInteger("type", 0);
        agent.animator.SetFloat("speed", 0);
    }

    public override void FixedUpdateExecute()
    {
        //ZLogUtil.Log("IdleState");

    }

    public override void UpdateExecute()
    {
        
    }


    public override bool TryRestCond()
    {
        return ISOverTime();
    }
}
