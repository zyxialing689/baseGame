using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgentNew : AIAgent
{

    public override void InitAgentData(int roleId, PlayerCamp playerCamp)
    {
        agentData = new AgentDataNew(roleId);
        agentData.playerCamp = playerCamp;
        Init();
        GetComponent<AICollider>().Init();
        gameObject.SetActive(true);
    }


    protected override AIState CreateRemoteAttackState(AIStateData value)
    {
        AIState aIState =  new RemoteAttackNew().Init(this, value);
        return aIState;
    }

}
