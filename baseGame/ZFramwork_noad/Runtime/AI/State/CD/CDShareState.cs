using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDShareState : AIState
{
    public override void Awake()
    {
        if (agent.agentTempData.cdMap.ContainsKey(stateData.shadreCDID))
        {
            agent.agentTempData.cdMap[stateData.shadreCDID] =  agent.curTime + stateData.cd;
        }
        else
        {
            agent.agentTempData.cdMap.Add(stateData.shadreCDID, agent.curTime+stateData.cd);
        }
    }

    public override AIStateData TryNextCond()
    {
        return stateData.condAIStateData[0];
    }

}
