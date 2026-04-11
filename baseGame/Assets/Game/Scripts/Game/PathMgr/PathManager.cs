using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathManager : MonoBehaviour
{
    public static PathManager _instance;

    void Awake()
    {
        _instance = this;
    }
    public static void Init()
    {
        GameObject obj = new GameObject("PathManager");
        _instance = obj.AddComponent<PathManager>();
    }

    [Header("最大同时计算数量")]
    public int maxConcurrent = 1;

    private int currentRunning = 0;

    private Queue<PathRequest> requestQueue = new Queue<PathRequest>();

    // =============================
    // 对外接口
    // =============================
    public void RequestPath(Vector3 start, Vector3 end, System.Action<List<Vector3>> callback)
    {
        PathRequest request = new PathRequest(start, end, callback);
        requestQueue.Enqueue(request);

        TryProcessNext();
    }

    // =============================
    // 调度器（核心）
    // =============================
    private void TryProcessNext()
    {
        if (currentRunning >= maxConcurrent) return;
        if (requestQueue.Count == 0) return;

        PathRequest request = requestQueue.Dequeue();
        currentRunning++;

        StartCoroutine(ProcessPath(request));
    }

    // =============================
    // 寻路处理
    // =============================
    private IEnumerator ProcessPath(PathRequest request)
    {
        ABPath path = ABPath.Construct(request.start, request.end);
        AstarPath.StartPath(path);

        // 等待计算完成（异步）
        yield return StartCoroutine(path.WaitForPath());

        List<Vector3> result = new List<Vector3>();

        if (!path.error && path.vectorPath != null && path.vectorPath.Count > 0)
        {
            List<Vector3> smooth = SmoothPath.SmoothSimple(path.vectorPath);

            for (int i = 0; i<smooth.Count; i++)
            {
                result.Add(smooth[i]);
            }
        }

        // 回调
        request.callback?.Invoke(result);

        currentRunning--;

        // 继续处理队列
        TryProcessNext();
    }

    // =============================
    // 可选：清空队列（比如切场景）
    // =============================
    public void ClearQueue()
    {
        requestQueue.Clear();
    }
}