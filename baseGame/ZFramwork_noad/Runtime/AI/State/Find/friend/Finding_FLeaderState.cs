using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finding_FLeaderState : AIState
{
    bool isLock = false;
    float nextFindTime = 0;
    private Stack<Vector3> curPath;
    private AIAgent leader;
    Vector3 curPathPos;
    private Vector3 lastPos;
    public override void Start()
    {
        isLock = false;
        leader = null;
        agent.animator.SetInteger("type", 0);
    }
    public override void FixedUpdateExecute()
    {
        if (leader!=null&& curPath != null && curPath.Count > 0)
        {

            if(Vector3.Distance(agent.transform.position, leader.transform.position) < stateData.stopDis)
            {
                findFinish = true;
                return;
            }
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
            //ZLogUtil.LogError("Ã»ÓÐfindÂ·¾¶");
        }

    }

    public override void UpdateExecute()
    {
        if (nextFindTime < agent.curTime || (curPath!=null && curPath.Count==0))
        {
            if (leader != null && !isLock)
            {

                ClearPath();
                agent.StartPath(agent.transform.position, leader.GetFindPos(agent.transform.position), OnCompletePath);
                isLock = true;
            }
            else
            {

                if (agent != agent.team.leader)
                {
                    leader = agent.team.leader;
                }
                if (leader == null)
                {
                    findFinish = true;
                }
            }
        }
        if(curPath!=null&& curPath.Count > 0)
        {
            curPathPos = curPath.Peek();
            if (Vector3.Distance(agent.transform.position, curPathPos) < 0.01f)
            {
                curPathPos = curPath.Pop();
                if (curPath.Count < 1)
                {
                    findFinish = true;
                    return;
                }
            }
        }

    }
    bool findFinish = false;
    private void OnCompletePath(Stack<Vector3> path)
    {
        curPath = path;
        var arr = curPath.ToArray();
        lastPos = arr[arr.Length - 1];
        UpdateNextFindTime();
    }
    public override bool TryRestCond()
    {
        return findFinish||ISOverTime();
    }
    public override void Exit()
    {
        agent.animator.SetFloat("speed", 0);
        agent.RestCurStackPath();
        findFinish = false;
    }

    private void UpdateNextFindTime()
    {
        nextFindTime = RandomMgr.GetValue() * stateData.findInterval + agent.curTime;
        isLock = false;
    }
    private void ClearPath()
    {
        if (curPath != null)
        {
            curPath.Clear();
        }
    }
}
