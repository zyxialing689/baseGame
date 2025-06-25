using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finding_FarState : AIState
{
    float nextFindTime = 0;
    private int curFindIndex;
    private Vector3 targetPos;
    public override void Start()
    {
        curFindIndex = -1;
        agent.animator.SetInteger("type", 0);

    }
    public override void FixedUpdateExecute()
    {
        if (agent.attackTarget != null && curFindIndex >= 0)
        {

            //agent.transform.position = Vector3.Lerp(agent.transform.position, curPath.Peek(), Time.fixedDeltaTime * 1f);
            if (agent.transform.position.x > targetPos.x)
            {
                agent.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                agent.transform.localScale = new Vector3(-1, 1, 1);
            }
            agent.animator.SetFloat("speed", agent.moveSpeed);
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, targetPos, Time.fixedDeltaTime * agent.moveSpeed);
            if (agent.transform.position==targetPos)
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
                targetPos = PathFindMgr._instance.GetCloserPos(agent.attackTarget.aICollider.groundBox.GetFindFarRandomPos3OutIndex(GetPointDir(), out curFindIndex));
            }
            else
            {
                targetPos = PathFindMgr._instance.GetCloserPos(agent.attackTarget.aICollider.groundBox.GetFindFarPos(GetPointDir(), curFindIndex));
            }
            if (nextFindTime < agent.curTime)
            {
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
