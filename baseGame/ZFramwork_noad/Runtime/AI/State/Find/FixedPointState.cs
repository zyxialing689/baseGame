using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedPointState : AIState
{
    bool isLock = false;
    float nextFindTime = 0;
    private Stack<Vector3> curPath;
    private bool isFinished = false;
    private Vector3 lastPos;
    Vector3 curPathPos;
    public override void Awake()
    {
        isLock = false;
        isFinished = false;
        switch (stateData.fixedPoint)
        {
            case FixedPointType.self_far_point:
                agent.StartPath(agent.transform.position, agent.GetFindFarPos(agent.transform.position), OnCompletePath);
                isLock = true;
                break;
            case FixedPointType.enemy_far_point:
                UpdateExecute();
                break;
            case FixedPointType.enemy_point:
                UpdateExecute();
                break;
        }
        FixedUpdateExecute();
    }

    public override void Start()
    {
        agent.animator.SetInteger("type", 0);
        FixedUpdateExecute();
    }
    public override void FixedUpdateExecute()
    {

        if (curPath != null && curPath.Count > 0)
        {


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
    }
    public override void UpdateExecute()
    {

        if (nextFindTime < agent.curTime&&!isLock)
        {
            switch (stateData.fixedPoint)
            {
                case FixedPointType.enemy_far_point:
                    FindEnemy_far_point();
                    break;
                case FixedPointType.enemy_point:
                    FindEnemy_point();
                    break;
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
                    isFinished = true;
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
        UpdateNextFindTime();
    }
    private void UpdateNextFindTime()
    {
        nextFindTime = RandomMgr.GetValue() * stateData.findInterval + agent.curTime;
        isLock = false;
    }
    public override AIStateData TryNextCond()
    {
        if (isFinished)
        {
            return stateData.condAIStateData[0]; ;
        }
        else
        {
            return null;
        }
     
    }
    public override void Exit()
    {
        agent.animator.SetFloat("speed", 0);
    }

    private void FindEnemy_far_point()
    {
        if (agent.attackTarget != null)
        {
            ClearPath();
            agent.StartPath(agent.transform.position, agent.attackTarget.GetFindFarPos(agent.transform.position), OnCompletePath);
            isLock = true;
        }
        else
        {
            agent.attackTarget = AIMgr.GetCloserTarget(agent, ClosestType.Y, stateData.findCotainSky);
            if (agent.attackTarget != null)
            {
                ClearPath();
                agent.StartPath(agent.transform.position, agent.attackTarget.GetFindFarPos(agent.transform.position), OnCompletePath);
            }
            else
            {
                ClearPath();
                agent.StartPath(agent.transform.position, agent.GetFindFarPos(agent.transform.position), OnCompletePath);
            }
            isLock = true;
        }
    }
    private void FindEnemy_point()
    {
        if (agent.attackTarget != null)
        {

            ClearPath();
            agent.StartPath(agent.transform.position, agent.attackTarget.GetFindPos(agent.transform.position), OnCompletePath);
            isLock = true;
        }
        else
        {
            agent.attackTarget = AIMgr.GetCloserTarget(agent, ClosestType.Y, stateData.findCotainSky);
            if (agent.attackTarget != null)
            {
                ClearPath();
                agent.StartPath(agent.transform.position, agent.attackTarget.GetFindPos(agent.transform.position), OnCompletePath);
            }
            else
            {
                ClearPath();
                agent.StartPath(agent.transform.position, agent.GetFindFarPos(agent.transform.position), OnCompletePath);
            }
            isLock = true;
        }
    }
    private void ClearPath()
    {
        if (curPath != null)
        {
            curPath.Clear();
        }
    }
}
