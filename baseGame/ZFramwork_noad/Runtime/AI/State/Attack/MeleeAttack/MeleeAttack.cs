using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class MeleeAttack : AIState
{
    private AgentSkill agentSkill;
    private int curLoopTime;
    private AIAgent tempTarget;
    public override void Awake()
    {
        agent.agentTempData.isAttacking = true;
        curLoopTime = 0;
        Attack();
    }
    public override void Start()
    {
        if (agent.attackTarget != null)
        {
            if (agent.transform.position.x - agent.attackTarget.transform.position.x < 0)
            {
                agent.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                agent.transform.localScale = Vector3.one;
            }
        }
    }
    public override void FixedUpdateExecute()
    {
        if (stateData.followEnemyDir)
        {
            if (agent.attackTarget != null)
            {
                if (agent.transform.position.x - agent.attackTarget.transform.position.x < 0)
                {
                    agent.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    agent.transform.localScale = Vector3.one;
                }
            }
        }

 
    }
    private void Attack()
    {
        if (stateData.isFarSkill)
        {
            tempTarget = agent.attackTarget;
            agent.attackTarget = AIMgr.GetCloserTarget(agent, stateData.serchType, stateData.findCotainSky);
        }

        if (agent.attackTarget == null)
        {
            agent.agentTempData.isAttacking = false;
            return;
        }
        //agentSkill = new AgentSkill(agent.agentData.skill_ids[stateData.meleeAttackID], agent);
        agentSkill = new AgentSkill(agent);

        AIAttackMgr.CreateAttackCollider(agentSkill,
            AttackComplete, stateData.attackKeepTime
            );
        agentSkill.SetAnim(agent.animator);
    }
    public override void UpdateExecute()
    {
     
    }

    public override AIStateData TryNextCond()
    {
        AIStateData tempState = null;
        if (!agent.agentTempData.isAttacking)
        {
            if (stateData.condAIStateData != null)
            {
                tempState = stateData.condAIStateData[0];
            }
        }
        return tempState;
    }

    public override bool TryRestCond()
    {
        return !agent.agentTempData.isAttacking;
    }

    void AttackComplete(AgentSkill agentSkill,bool skillFULL)
    {
        if (skillFULL)
        {
            curLoopTime++;
        }
        else
        {
            agent.agentTempData.isAttacking = false;
        }

        if (stateData.attackCount < curLoopTime)
        {
            agent.agentTempData.isAttacking = false;
            if (stateData.isFarSkill&&stateData.isFarBackTarget)
            {
               agent.attackTarget = tempTarget;
            }
        }
        else
        {
            Attack();
        }

    }

}
