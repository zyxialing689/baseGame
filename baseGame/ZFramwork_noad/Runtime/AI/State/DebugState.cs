using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugState : AIState
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

    public override AIStateData TryNextCond()
    {
        var value = base.TryNextCond();
        Debug.Log(stateData.logStr);
        return value;
    }

    public override bool TryRestCond()
    {
   
        return true;
    }
}
