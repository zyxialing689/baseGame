using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDResetGlobal: AIState
{
    public override void Awake()
    {
        if (agent.agentTempData.globalCdMap.ContainsKey(stateData.resetID))
        {
            agent.agentTempData.globalCdMap[stateData.resetID].time =  agent.curTime + agent.agentTempData.globalCdMap[stateData.resetID].cd;
        }
    }

    public override bool TryRestCond()
    {
        return true;
    }

}
