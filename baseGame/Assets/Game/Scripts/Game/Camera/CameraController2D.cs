using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    private Camera cam;

    [Header("地图参数")]
    public Vector2 mapCenter = new Vector2(100, 100);
    public float mapWidth = 200;
    public float mapHeight = 200;

    [Header("缩放")]
    public float minSize = 10f;
    public float maxSize = 40f;
    public float zoomSpeed = 2f;

    [Header("拖动")]
    public float dragSpeed = 1f;

    [Header("跟随")]
    public Transform target;
    public float followSpeed = 5f;
    public bool followX = true;
    public bool followY = true;
    private Vector3 followVelocity = Vector3.zero;
    public float followSmoothTime = 0.15f;

    private Vector3 lastTouchPos;
    private bool isDragging = false;
    private Vector2 mouseDownPos;
    private bool isClick = false;
    public float clickThreshold = 10f; // 像素阈值（可调）

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = 20;
        transform.localPosition = new Vector3(100, 100);
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
        ClampCamera();

    }

    void FixedUpdate()
    {
        HandleFollow();
    }



    // ================= PC操作 =================
    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
            isClick = true;

            lastTouchPos = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;

            // ⭐ 一开始不选目标
        }

        if (Input.GetMouseButton(0))
        {
            // ⭐ 判断是否超过点击阈值（变成拖动）
            if (Vector2.Distance(mouseDownPos, Input.mousePosition) > clickThreshold)
            {
                isClick = false;

                // ⭐ 一旦拖动，取消跟随
                target = null;
            }

            if (isDragging)
            {
                Vector3 curPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 delta = lastTouchPos - curPos;

                Vector3 targetPos = cam.transform.position + delta * dragSpeed;
                cam.transform.position = ClampPosition(targetPos);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // ⭐ 只有“点击”才选中目标
            if (isClick)
            {
                TrySelectTarget_NoCollider(Input.mousePosition);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Zoom(scroll * 10f);
        }
    }

    // ================= 手机触摸 =================
    void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                mouseDownPos = t.position;
                isClick = true;

                lastTouchPos = cam.ScreenToWorldPoint(t.position);
            }
            else if (t.phase == TouchPhase.Moved)
            {
                if (Vector2.Distance(mouseDownPos, t.position) > clickThreshold)
                {
                    isClick = false;

                    // ⭐ 拖动就取消跟随
                    target = null;
                }

                Vector3 curPos = cam.ScreenToWorldPoint(t.position);
                Vector3 delta = lastTouchPos - curPos;

                Vector3 targetPos = cam.transform.position + delta * dragSpeed;
                cam.transform.position = ClampPosition(targetPos);
            }
            else if (t.phase == TouchPhase.Ended)
            {
                if (isClick)
                {
                    TrySelectTarget_NoCollider(t.position);
                }
            }
        }
        else if (Input.touchCount == 2)
        {
            isClick = false; // 双指必定不是点击

            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 prev1 = t1.position - t1.deltaPosition;
            Vector2 prev2 = t2.position - t2.deltaPosition;

            float prevDist = (prev1 - prev2).magnitude;
            float curDist = (t1.position - t2.position).magnitude;

            float delta = curDist - prevDist;

            Zoom(delta * 0.01f);
        }
    }
    // ================= 缩放 =================
    void Zoom(float delta)
    {
        float size = cam.orthographicSize;
        size -= delta * zoomSpeed;
        size = Mathf.Clamp(size, minSize, maxSize);

        cam.orthographicSize = size;
    }

    // ================= 跟随 =================
    void HandleFollow()
    {
        if (target == null) return;

        Vector3 currentPos = cam.transform.position;
        Vector3 targetPos = target.position;

        targetPos.z = currentPos.z;

        // ⭐ 关键1：先Clamp目标
        targetPos = ClampPosition(targetPos);

        Vector3 newPos = Vector3.SmoothDamp(
            currentPos,
            targetPos,
            ref followVelocity,
            followSmoothTime
        );

        // ⭐ 关键2：再Clamp一次（防冲出）
        newPos = ClampPosition(newPos);

        if (!followX) newPos.x = currentPos.x;
        if (!followY) newPos.y = currentPos.y;

        cam.transform.position = newPos;
    }

    // ================= 通用Clamp =================
    Vector3 ClampPosition(Vector3 pos)
    {
        float height = cam.orthographicSize * 2;
        float width = height * cam.aspect;

        float halfH = height / 2;
        float halfW = width / 2;

        float minX = mapCenter.x - mapWidth / 2 + halfW;
        float maxX = mapCenter.x + mapWidth / 2 - halfW;
        float minY = mapCenter.y - mapHeight / 2 + halfH;
        float maxY = mapCenter.y + mapHeight / 2 - halfH;

        if (minX > maxX)
            pos.x = mapCenter.x;
        else
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (minY > maxY)
            pos.y = mapCenter.y;
        else
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }

    void ClampCamera()
    {
        cam.transform.position = ClampPosition(cam.transform.position);
    }

    void TrySelectTarget_NoCollider(Vector2 screenPos)
    {
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        PathTest[] all = FindObjectsOfType<PathTest>();

        SpriteRenderer hitSprite = null;
        float maxZ = float.MinValue;

        foreach (var sr in all)
        {
            if (sr.spriteRenderer == null) continue;

            if (sr.spriteRenderer.bounds.Contains(worldPos))
            {
                if (sr.transform.position.z > maxZ)
                {
                    maxZ = sr.transform.position.z;
                    hitSprite = sr.spriteRenderer;
                }
            }
        }

        if (hitSprite != null)
        {
            target = hitSprite.transform;
        }
    }
}