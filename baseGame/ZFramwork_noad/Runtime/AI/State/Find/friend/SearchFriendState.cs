using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchFriendState : AIState
{
    public override void Awake()
    {

        agent.attackTarget = AIMgr.GetCloserTarget(agent, stateData.serchType, stateData.findCotainSky, false,true);

    }


    public override bool TryRestCond()
    {
        return true;
    }
}
