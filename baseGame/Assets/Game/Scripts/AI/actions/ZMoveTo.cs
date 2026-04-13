using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ZMoveTo : Action
{
    public SharedVector3List path; // ⭐ 直接拿共享路径
    public float moveSpeed = 5f;

    private int pathIndex;
    private List<Vector3> curPath;
    private TaskStatus status = TaskStatus.Running;
    public override void OnStart()
    {
        curPath = path.Value;

        if (curPath == null || curPath.Count == 0)
        {
            status = TaskStatus.Failure;
            return;
        }

        pathIndex = 1;
        if (pathIndex >= curPath.Count)
            pathIndex = 0;

        status = TaskStatus.Running; // ⭐ 重置
    }

    public override TaskStatus OnUpdate()
    {
        return status;
    }

    public override void OnFixedUpdate()
    {
        if (status != TaskStatus.Running)
            return;

        if (curPath == null || pathIndex >= curPath.Count)
            return;

        float step = moveSpeed * Time.fixedDeltaTime;

        Vector3 target = curPath[pathIndex];
        float sqrDist = (transform.position - target).sqrMagnitude;

        if (sqrDist <= step * step)
        {
            pathIndex++;

            // ✅ 到终点
            if (pathIndex >= curPath.Count)
            {
                status = TaskStatus.Success; // ⭐ 改状态
                return;
            }

            target = curPath[pathIndex];
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            step
        );
    }

    public override void OnEnd()
    {
        status = TaskStatus.Running;
        curPath = null;
    }
}