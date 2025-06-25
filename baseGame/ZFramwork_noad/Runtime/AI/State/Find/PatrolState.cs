using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : AIState
{

    float nextFindTime = 0;
    private Stack<Vector3> curPath;
    private bool isNonePath;
    private Vector3 lastPos;
    Vector3 curPathPos;
    public override void Start()
    {
        agent.animator.SetInteger("type", 0);
        isNonePath = false;
        if (agent.attackTarget != null)
        {
            curPath = PathFindMgr._instance.GetPatrolPath(agent.attackTarget.transform.position);
            var arr = curPath.ToArray();
            lastPos = arr[arr.Length - 1];
        }
        else
        {
            curPath = PathFindMgr._instance.GetPatrolPath(agent.transform.position);
            var arr = curPath.ToArray();
            lastPos = arr[arr.Length - 1];
        }
      
    }
    public override void FixedUpdateExecute()
    {
        if (curPath != null && curPath.Count > 0)
        {

            //agent.transform.position = Vector3.Lerp(agent.transform.position, curPath.Peek(), Time.fixedDeltaTime * 1f);
            if (agent.transform.position.x > curPathPos.x)
            {
                agent.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                agent.transform.localScale = new Vector3(-1, 1, 1);
            }
            agent.animator.SetFloat("speed", agent.moveSpeed);
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, curPathPos, Time.fixedDeltaTime * agent.moveSpeed);
        }
        else
        {
            agent.animator.SetFloat("speed", 0);
            ZLogUtil.LogError("Ã»ÓÐpatolÂ·¾¶");
            isNonePath = true;
        }
    }

    public override void UpdateExecute()
    {
        if (nextFindTime < agent.curTime)
        {

            if (agent.attackTarget != null)
            {
                curPath = PathFindMgr._instance.GetPatrolPath(agent.attackTarget.transform.position);
                var arr = curPath.ToArray();
                lastPos = arr[arr.Length - 1];
                UpdateNextFindTime();
            }
            else
            {
                curPath = PathFindMgr._instance.GetPatrolPath(agent.transform.position);
                var arr = curPath.ToArray();
                lastPos = arr[arr.Length - 1];
                UpdateNextFindTime();
            }
        }
        if (curPath != null && curPath.Count > 0)
        {
            curPathPos = curPath.Peek();
            if (Vector3.Distance(agent.transform.position, curPathPos) < 0.2f)
            {
                curPathPos = curPath.Pop();
                if (curPath.Count < 1)
                {
                    agent.agentTempData.patrolTimes++;
                    return;
                }
            }
        }
    }
    private void OnCompletePath(Stack<Vector3> path)
    {
        curPath = path;
        var arr = curPath.ToArray();
        lastPos = arr[arr.Length - 1];
    }
    private void UpdateNextFindTime()
    {
        nextFindTime = RandomMgr.GetValue() * stateData.findInterval + agent.curTime;
    }

    public override bool TryRestCond()
    {
        return ISOverTime();//|| isNonePath;
    }
 
}
