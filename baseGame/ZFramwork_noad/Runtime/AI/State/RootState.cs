using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootState : AIState
{
    public override void Awake()
    {
        if (agent.team != null && agent.team.leader == null)
        {
            agent.team.ChangeLeader();
        }
    }



}
