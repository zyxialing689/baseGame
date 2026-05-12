using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ZMoveTo : Action
{
    public SharedVector3List path; // ⭐ 直接拿共享路径
    public float moveSpeed = 1f;
    private int pathIndex;
    private List<Vector3> curPath;
    private GpuAgentBase gpuAgent;
    private TaskStatus status = TaskStatus.Running;

    // 跳跃
    private float _jumpTimer;
    private float _jumpAnimTimer;
    private float _jumpDuration;
    private float _jumpPeak;
    public Vector2 jumpIntervalRange = new Vector2(2f, 5f);
    public Vector2 jumpHeightRange = new Vector2(1f, 3f);
    public Vector2 jumpDurationRange = new Vector2(0.4f, 0.8f);

    public override void OnStart()
    {
        _jumpTimer = Random.Range(jumpIntervalRange.x, jumpIntervalRange.y);
        _jumpAnimTimer = -1f;

        curPath = path.Value;
        gpuAgent = gameObject.GetComponent<GpuAgentBase>();
        moveSpeed = 1 + Random.value * gpuAgent.baseMoveSpeed;
     
        gpuAgent.SetMoveAnimSpeed(moveSpeed);
        if (curPath == null || curPath.Count == 0)
        {
            status = TaskStatus.Failure;
            return;
        }

        pathIndex = 1;
        if (pathIndex >= curPath.Count)
            pathIndex = 0;

        status = TaskStatus.Running; // ⭐ 重置
        gpuAgent.Play("MOVE");
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
        // ⭐ 计算方向并翻转
        float dirX = target.x - transform.position.x;
        gpuAgent.SetFlipX(dirX > 0);
        float sqrDist = (transform.position - target).sqrMagnitude;

        if (sqrDist <= step * step)
        {
            pathIndex++;

            // ✅ 到终点
            if (pathIndex >= curPath.Count)
            {
                status = TaskStatus.Success; // ⭐ 改状态
                gpuAgent.Play("IDLE");
                return;
            }

            target = curPath[pathIndex];
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            step
        );
        gpuAgent.SetPosition(transform.position);

        // 跳跃
        UpdateJump();
    }

    private void UpdateJump()
    {
        if (_jumpAnimTimer >= 0f)
        {
            _jumpAnimTimer += Time.fixedDeltaTime;
            if (_jumpAnimTimer >= _jumpDuration)
            {
                gpuAgent.SetJumpHeight(0f);
                _jumpAnimTimer = -1f;
                _jumpTimer = Random.Range(jumpIntervalRange.x, jumpIntervalRange.y);
                // 落地，恢复走路动画
                gpuAgent.Play("MOVE");
            }
            else
            {
                float t = _jumpAnimTimer / _jumpDuration;
                gpuAgent.SetJumpHeight(Mathf.Sin(t * Mathf.PI) * _jumpPeak);
            }
        }
        else
        {
            _jumpTimer -= Time.fixedDeltaTime;
            if (_jumpTimer <= 0f)
            {
                _jumpDuration = Random.Range(jumpDurationRange.x, jumpDurationRange.y);
                _jumpPeak = Random.Range(jumpHeightRange.x, jumpHeightRange.y);
                _jumpAnimTimer = 0f;
                // 起跳，空中强制 IDLE
                gpuAgent.Play("IDLE");
            }
        }
    }

    public override void OnEnd()
    {
        gpuAgent.SetJumpHeight(0f);
        status = TaskStatus.Running;
        curPath = null;
    }
}