using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDGlobalState : AIState
{
    public override void Awake()
    {
        if (agent.agentTempData.globalCdMap.ContainsKey(stateData.shadreCDID))
        {
            if (agent.agentTempData.globalCdMap.ContainsKey(stateData.shadreCDID))
            {
                agent.agentTempData.globalCdMap[stateData.shadreCDID].time = agent.curTime + stateData.cd;
            }
            else
            {
                agent.agentTempData.globalCdMap.Add(stateData.shadreCDID, new GlobalCD(agent.curTime + stateData.cd, stateData.cd));
            }

        }
        else
        {
            agent.agentTempData.globalCdMap.Add(stateData.shadreCDID, new GlobalCD(agent.curTime + stateData.cd, stateData.cd));
        }
    }

    public override AIStateData TryNextCond()
    {
        return stateData.condAIStateData[0];
    }

}
