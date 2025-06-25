using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchEnemyState : AIState
{
    public override void Awake()
    {
        if (agent.attackTarget == null)
        {
            agent.attackTarget = AIMgr.GetCloserTarget(agent, stateData.serchType, stateData.findCotainSky);
        }

    }


    public override bool TryRestCond()
    {
        return true;
    }
}
