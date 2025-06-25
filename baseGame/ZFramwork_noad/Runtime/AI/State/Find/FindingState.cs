using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindingState : AIState
{
    float nextFindTime = 0;
    private int curFindIndex;
    private Vector3 targetPos;
    private AIAgent[] targets;
    public override void Start()
    {
        curFindIndex = -1;
        agent.animator.SetInteger("type", 0);

    }
    public override void FixedUpdateExecute()
    {
        if (agent.attackTarget!=null&& curFindIndex>=0)
        {

            //agent.transform.position = Vector3.Lerp(agent.transform.position, curPath.Peek(), Time.fixedDeltaTime * 1f);
            if (agent.transform.position.x > targetPos.x)
            {
                agent.transform.localScale = new Vector3(1, 1, 1);
            }else
            {
                agent.transform.localScale = new Vector3(-1, 1, 1);
            }
            agent.animator.SetFloat("speed", agent.moveSpeed);
            if(agent.rovMovement != null&& agent.rovMovement._FixedUpdate(agent, targets, targetPos))
            {
                targetPos = agent.attackTarget.aICollider.groundBox.GetFindRandomPos3OutIndex(GetPointDir(true), out curFindIndex);
                return;
            }
            //agent.transform.position = Vector3.MoveTowards(agent.transform.position, targetPos, Time.fixedDeltaTime * agent.moveSpeed);
            if(Vector2.Distance(agent.transform.position,targetPos)<0.1f)
            {
                curFindIndex = -1;
            }
        }

    }

    public override void UpdateExecute()
    {
        if (agent.attackTarget != null)
        {
            if (curFindIndex < 0)
            {
                targetPos = agent.attackTarget.aICollider.groundBox.GetFindRandomPos3OutIndex(GetPointDir(), out curFindIndex);
            }
            else
            {
                targetPos = agent.attackTarget.aICollider.groundBox.GetFindPos(GetPointDir(), curFindIndex);
            }
            if (nextFindTime < agent.curTime)
            {
                targets = AIMgr.GetRandomAIAgents(agent, 5, ClosestType.DIS, true, false);
                UpdateNextFindTime();
            }
        }
    }
    private void UpdateNextFindTime()
    {
        nextFindTime = RandomMgr.GetValue() * stateData.findInterval + agent.curTime;
    }
    public override bool TryRestCond()
    {
        return agent.attackTarget == null;
    }
    public override void Exit()
    {
        agent.animator.SetFloat("speed", 0);
    }

    private PointDir GetPointDir(bool reverse = false)
    {
        if (reverse)
        {
            if (agent.transform.position.x > agent.attackTarget.transform.position.x)
            {
                return PointDir.Left;
            }
            else
            {
                return PointDir.Right;
            }
        }
        else
        {
            if (agent.transform.position.x > agent.attackTarget.transform.position.x)
            {
                return PointDir.Right;
            }
            else
            {
                return PointDir.Left;
            }
        }

    }

}
