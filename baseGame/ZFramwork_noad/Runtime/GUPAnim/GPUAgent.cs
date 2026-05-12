using UnityEngine;

public class GPUAgent : GpuAgentBase
{
    public GPUAnimManager manager;
    public bool autoInitialize = true;
    public bool initializeOnEnable = true;
    public bool removeOnDisable;
    public bool removeOnDestroy = true;

    public bool syncTransformPosition = false;
    public Vector3 positionOffset;

    public string characterName;
    public string clipName;
    [HideInInspector]
    public float animSpeed = 1f;
    public bool flipX;

    private int roleId = -1;
    private Vector3 lastPosition;
    private string lastCharacterName;
    private string lastClipName;
    private float lastScale;
    private float lastAnimSpeed;
    private bool lastVisible;
    private bool lastFlipX;
    private float lastJumpHeight;

    public int RoleId => roleId;
    public bool IsInitialized => manager != null && manager.IsValidRole(roleId);
    public Vector3 RenderPosition => transform.position + positionOffset;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<GPUAnimManager>();
        }
    }

    private void OnEnable()
    {
        if (IsInitialized)
        {
            Sync(true);
            manager.SetVisible(roleId, visible);
            manager.SetTransformSync(roleId, this, syncTransformPosition);
            return;
        }

        if (autoInitialize && initializeOnEnable)
        {
            Initialize();
        }
    }

    private void Start()
    {
        if (IsInitialized)
        {
            Sync(true);
            manager.SetTransformSync(roleId, this, syncTransformPosition && isActiveAndEnabled);
            return;
        }

        if (autoInitialize && !initializeOnEnable)
        {
            Initialize();
        }
    }

    private void OnDisable()
    {
        if (IsInitialized)
        {
            manager.SetTransformSync(roleId, this, false);
            manager.SetVisible(roleId, false);
        }

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
            manager.SetTransformSync(roleId, this, syncTransformPosition && isActiveAndEnabled);
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

        roleId = manager.CreateRole(characterName, GetRenderPosition(), scale, clipName);
        if (roleId < 0)
        {
            return roleId;
        }

        Sync(true);
        manager.SetTransformSync(roleId, this, syncTransformPosition && isActiveAndEnabled);
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

    public override void SetPosition(Vector3 position)
    {
        transform.position = position;
        if (IsInitialized)
        {
            manager.SetPosition(roleId, GetRenderPosition());
            lastPosition = GetRenderPosition();
            transform.hasChanged = false;
        }
    }

    public override void Play(string name)
    {
        SetClip(name);
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

    public void SetCharacter(string newCharacterName, string newClipName = null, bool resetTime = true)
    {
        characterName = newCharacterName;
        if (!string.IsNullOrEmpty(newClipName))
        {
            clipName = newClipName;
        }

        if (IsInitialized)
        {
            manager.SetCharacter(roleId, characterName, clipName, resetTime);
            lastCharacterName = characterName;
            lastClipName = clipName;
        }
    }

    public override void SetScale(float newScale)
    {
        scale = newScale;
        if (IsInitialized)
        {
            manager.SetScale(roleId, scale);
            lastScale = scale;
        }
    }

    public override void SetAnimSpeed(float newSpeed)
    {
        animSpeed = newSpeed;
        if (IsInitialized)
        {
            manager.SetSpeed(roleId, animSpeed);
            lastAnimSpeed = animSpeed;
        }
    }

    public override void SetVisible(bool newVisible)
    {
        visible = newVisible;
        if (IsInitialized)
        {
            manager.SetVisible(roleId, visible);
            lastVisible = visible;
        }
    }

    public override void SetFlipX(bool newFlipX)
    {
        flipX = newFlipX;
        if (IsInitialized)
        {
            manager.SetFlipX(roleId, flipX);
            lastFlipX = flipX;
        }
    }

    public void SetSyncTransform(bool sync)
    {
        syncTransformPosition = sync;
        if (IsInitialized)
        {
            manager.SetTransformSync(roleId, this, sync && isActiveAndEnabled);
            if (sync)
                transform.hasChanged = true;
        }
    }

    public void SetAnimTime(float time)
    {
        if (IsInitialized)
        {
            manager.SetAnimTime(roleId, time);
        }
    }

    public override void SetJumpHeight(float height)
    {
        base.SetJumpHeight(height);
        if (IsInitialized)
        {
            manager.SetJumpHeight(roleId, height);
        }
    }

    public void Sync()
    {
        Sync(false);
    }

    private void Sync(bool force)
    {
        Vector3 renderPosition = GetRenderPosition();
        if (force || (syncTransformPosition && renderPosition != lastPosition))
        {
            manager.SetPosition(roleId, renderPosition);
            lastPosition = renderPosition;
            if (force)
                transform.hasChanged = false;
        }

        if (force || characterName != lastCharacterName)
        {
            manager.SetCharacter(roleId, characterName, clipName);
            lastCharacterName = characterName;
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

        if (force || !Mathf.Approximately(animSpeed, lastAnimSpeed))
        {
            manager.SetSpeed(roleId, animSpeed);
            lastAnimSpeed = animSpeed;
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

        if (force || !Mathf.Approximately(jumpHeight, lastJumpHeight))
        {
            manager.SetJumpHeight(roleId, jumpHeight);
            lastJumpHeight = jumpHeight;
        }
    }

    private Vector3 GetRenderPosition()
    {
        return transform.position + positionOffset;
    }
}
