using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDState : AIState
{
    public override void Awake()
    {
        if (agent.agentTempData.cdMap.ContainsKey(stateData.id))
        {
            agent.agentTempData.cdMap[stateData.id] =  agent.curTime + stateData.cd;
        }
        else
        {
            agent.agentTempData.cdMap.Add(stateData.id,agent.curTime+stateData.cd);
        }
    }

    public override AIStateData TryNextCond()
    {
        return stateData.condAIStateData[0];
    }

}
