using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class PathTest : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float moveSpeed = 10;
    int count = 0;
    private List<Vector3> curPath;
    int pathIndex = 0;
    Vector3 curPathPos;
    bool isMoving = false;
    bool isCalculating = false;
    void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        FindPos();
    }

    void FindPos()
    {
        if (isCalculating) return;

        isCalculating = true;
        isMoving = false; // 🔥 停住

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(RandomMgr.Range(0, 200), RandomMgr.Range(0, 200), 0);

        PathManager._instance.RequestPath(startPos, endPos, (path) =>
        {
            isCalculating = false;

            if (path == null || path.Count == 0)
                return;

            curPath = path;

            // 🔥 跳过起点
            pathIndex = 1;
            if (pathIndex >= curPath.Count)
                pathIndex = 0;

            isMoving = true; // 🔥 开始走
        });
    }

    void FixedUpdate()
    {
        // ❗ 没路径 or 不在移动 → 不动
        if (!isMoving || curPath == null || pathIndex >= curPath.Count)
            return;

        float step = moveSpeed * Time.fixedDeltaTime;

        Vector3 target = curPath[pathIndex];
        float sqrDist = (transform.position - target).sqrMagnitude;

        if (sqrDist <= step * step)
        {
            pathIndex++;

            // ✅ 到终点了
            if (pathIndex >= curPath.Count)
            {
                isMoving = false;

                // 🔥 到达后再请求新路径
                FindPos();
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

    void OnDrawGizmosSelected()
    {
        if (curPath == null || curPath.Count == 0)
            return;

        var array = curPath.ToArray();

        Vector3 prev = transform.position;

        for (int i = array.Length - 1; i >= 0; i--)
        {
            // 画线
            Gizmos.color = Color.green;
            Gizmos.DrawLine(prev, array[i]);

            // 起点/终点区分
            if (i == array.Length - 1)
                Gizmos.color = Color.blue;   // 起点
            else if (i == 0)
                Gizmos.color = Color.red;    // 终点
            else
                Gizmos.color = Color.cyan;

            Gizmos.DrawSphere(array[i], 0.25f);

            prev = array[i];
        }

        // 当前目标点
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(curPathPos, 0.4f);
    }
}