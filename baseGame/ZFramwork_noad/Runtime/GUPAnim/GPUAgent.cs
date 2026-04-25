using UnityEngine;

public class GPUAgent : MonoBehaviour
{
    public GPUAnimManager manager;
    public bool autoInitialize = true;
    public bool initializeOnEnable = true;
    public bool removeOnDisable;
    public bool removeOnDestroy = true;

    public bool syncTransformPosition = true;
    public Vector3 positionOffset;

    public string clipName;
    public float scale = 1f;
    public float speed = 1f;
    public bool visible = true;
    public bool flipX;

    private int roleId = -1;
    private Vector3 lastPosition;
    private string lastClipName;
    private float lastScale;
    private float lastSpeed;
    private bool lastVisible;
    private bool lastFlipX;

    public int RoleId => roleId;
    public bool IsInitialized => manager != null && manager.IsValidRole(roleId);

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<GPUAnimManager>();
        }
    }

    private void OnEnable()
    {
        if (autoInitialize && initializeOnEnable)
        {
            Initialize();
        }
    }

    private void Start()
    {
        if (autoInitialize && !initializeOnEnable)
        {
            Initialize();
        }
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        Sync();
    }

    private void OnDisable()
    {
        if (removeOnDisable)
        {
            Remove();
        }
    }

    private void OnDestroy()
    {
        if (removeOnDestroy)
        {
            Remove();
        }
    }

    public int Initialize()
    {
        if (IsInitialized)
        {
            Sync(true);
            return roleId;
        }

        if (manager == null)
        {
            manager = FindObjectOfType<GPUAnimManager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("GPUAgent needs a GPUAnimManager.", this);
            return -1;
        }

        roleId = manager.CreateRole(GetRenderPosition(), scale, clipName);
        if (roleId < 0)
        {
            return roleId;
        }

        Sync(true);
        return roleId;
    }

    public void Remove()
    {
        if (manager != null && manager.IsValidRole(roleId))
        {
            manager.RemoveRole(roleId);
        }

        roleId = -1;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        if (IsInitialized)
        {
            manager.SetPosition(roleId, GetRenderPosition());
            lastPosition = GetRenderPosition();
        }
    }

    public void SetClip(string newClipName, bool resetTime = true)
    {
        clipName = newClipName;
        if (IsInitialized)
        {
            manager.SetClip(roleId, clipName, resetTime);
            lastClipName = clipName;
        }
    }

    public void SetScale(float newScale)
    {
        scale = newScale;
        if (IsInitialized)
        {
            manager.SetScale(roleId, scale);
            lastScale = scale;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (IsInitialized)
        {
            manager.SetSpeed(roleId, speed);
            lastSpeed = speed;
        }
    }

    public void SetVisible(bool newVisible)
    {
        visible = newVisible;
        if (IsInitialized)
        {
            manager.SetVisible(roleId, visible);
            lastVisible = visible;
        }
    }

    public void SetFlipX(bool newFlipX)
    {
        flipX = newFlipX;
        if (IsInitialized)
        {
            manager.SetFlipX(roleId, flipX);
            lastFlipX = flipX;
        }
    }

    public void SetAnimTime(float time)
    {
        if (IsInitialized)
        {
            manager.SetAnimTime(roleId, time);
        }
    }

    public void Sync()
    {
        Sync(false);
    }

    private void Sync(bool force)
    {
        Vector3 renderPosition = GetRenderPosition();
        if (syncTransformPosition && (force || renderPosition != lastPosition))
        {
            manager.SetPosition(roleId, renderPosition);
            lastPosition = renderPosition;
        }

        if (force || clipName != lastClipName)
        {
            manager.SetClip(roleId, clipName);
            lastClipName = clipName;
        }

        if (force || !Mathf.Approximately(scale, lastScale))
        {
            manager.SetScale(roleId, scale);
            lastScale = scale;
        }

        if (force || !Mathf.Approximately(speed, lastSpeed))
        {
            manager.SetSpeed(roleId, speed);
            lastSpeed = speed;
        }

        if (force || visible != lastVisible)
        {
            manager.SetVisible(roleId, visible);
            lastVisible = visible;
        }

        if (force || flipX != lastFlipX)
        {
            manager.SetFlipX(roleId, flipX);
            lastFlipX = flipX;
        }
    }

    private Vector3 GetRenderPosition()
    {
        return transform.position + positionOffset;
    }
}
