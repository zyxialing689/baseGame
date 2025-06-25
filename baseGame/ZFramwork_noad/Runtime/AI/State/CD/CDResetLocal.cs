using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDResetLocal : AIState
{
    public override void Awake()
    {
        if (agent.agentTempData.cdMap.ContainsKey(stateData.resetID))
        {
            agent.agentTempData.cdMap[stateData.resetID] =  agent.curTime + stateData.cd;
        }
        else
        {
            agent.agentTempData.cdMap.Add(stateData.resetID, agent.curTime + stateData.cd);
        }
    }

    public override bool TryRestCond()
    {
        return true;
    }

}
