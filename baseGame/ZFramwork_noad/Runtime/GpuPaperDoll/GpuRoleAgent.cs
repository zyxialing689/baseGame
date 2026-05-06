using System.Collections.Generic;
using UnityEngine;

public class GpuRoleAgent : MonoBehaviour
{
    public GpuRoleExportData exportData;

    [Header("Animation")]
    public int animIndex = 0;
    public float playbackSpeed = 1f;
    public bool playOnEnable = true;

    [Header("Initial Group Variants")]
    public string[] initialGroupVariants = new string[0];

    [Header("Initial Independent Slot SpriteId")]
    public int[] initialIndependentSlotSpriteIds = new int[0];

    public Color color = Color.white;

    public float scale = 1f;

    [System.NonSerialized] public GpuRoleGpuManager manager;
    [System.NonSerialized] public int runtimeIndex = -1;

    private int[] _slotSpriteIds;
    private bool[] _slotVisible;
    private Dictionary<string, int> _slotIndexByKey = new Dictionary<string, int>();
    private Dictionary<int, int> _slotToGroupMap = new Dictionary<int, int>();
    private float _animStartTime;
    private bool _initialized;

    public bool IsInitialized => _initialized;
    public float AnimationStartTime => _animStartTime;
    public int SlotCount => _slotSpriteIds != null ? _slotSpriteIds.Length : 0;
    public Vector3 Position => transform.position;
    public Vector3 LocalPosition => transform.localPosition;
    public Quaternion Rotation => transform.rotation;
    public Vector3 LocalScale => transform.localScale;
    public bool FlipX => transform.localScale.x < 0f;
    public bool FlipY => transform.localScale.y < 0f;
    public int CurrentAnimIndex
    {
        get
        {
            if (exportData == null || exportData.animations == null || exportData.animations.Count == 0)
                return 0;
            return Mathf.Clamp(animIndex, 0, exportData.animations.Count - 1);
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();
        if (exportData == null) return;

        if (manager == null)
            manager = Object.FindObjectOfType<GpuRoleGpuManager>();

        if (manager == null)
        {
            Debug.LogError("[GpuRoleAgent] 场景中没有 GpuRoleGpuManager");
            return;
        }

        if (playOnEnable)
            _animStartTime = Time.time;

        manager.Register(this);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.Unregister(this);
    }

    public void EnsureInitialized()
    {
        if (_initialized && _slotSpriteIds != null && exportData != null && _slotSpriteIds.Length == exportData.slots.Count)
            return;

        _initialized = false;
        _slotIndexByKey.Clear();
        _slotToGroupMap.Clear();

        if (exportData == null || exportData.slots == null)
            return;

        for (int i = 0; i < exportData.slots.Count; i++)
            _slotIndexByKey[exportData.slots[i].slotKey] = i;

        if (exportData.groups != null)
        {
            for (int g = 0; g < exportData.groups.Count; g++)
            {
                GroupExportData group = exportData.groups[g];
                if (group.slotIndices == null) continue;
                for (int i = 0; i < group.slotIndices.Length; i++)
                    _slotToGroupMap[group.slotIndices[i]] = g;
            }
        }

        _slotSpriteIds = new int[exportData.slots.Count];
        _slotVisible = new bool[exportData.slots.Count];
        for (int i = 0; i < exportData.slots.Count; i++)
        {
            _slotSpriteIds[i] = exportData.slots[i].defaultSpriteId;
            _slotVisible[i] = true;
        }

        _initialized = true;
        ApplyInitialGroupVariants();
        ApplyInitialIndependentSlots();

        animIndex = CurrentAnimIndex;
        _animStartTime = Time.time;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetPosition(float x, float y)
    {
        Vector3 position = transform.position;
        position.x = x;
        position.y = y;
        transform.position = position;
    }

    public void SetPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

    public void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void Move(Vector3 delta)
    {
        transform.position += delta;
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void SetRotationZ(float degrees)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, degrees);
    }

    public void SetLocalScale(Vector3 localScale)
    {
        transform.localScale = localScale;
    }

    public void SetFlipX(bool flipped)
    {
        Vector3 localScale = transform.localScale;
        float x = Mathf.Abs(localScale.x);
        if (Mathf.Approximately(x, 0f))
            x = 1f;
        localScale.x = flipped ? -x : x;
        transform.localScale = localScale;
    }

    public void SetFlipY(bool flipped)
    {
        Vector3 localScale = transform.localScale;
        float y = Mathf.Abs(localScale.y);
        if (Mathf.Approximately(y, 0f))
            y = 1f;
        localScale.y = flipped ? -y : y;
        transform.localScale = localScale;
    }

    public void SetFacingX(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
            return;

        SetFlipX(direction < 0f);
    }

    public void LookAt2D(Vector3 worldPosition)
    {
        SetFacingX(worldPosition.x - transform.position.x);
    }

    public void RebuildInitialState()
    {
        _initialized = false;
        EnsureInitialized();
        manager?.MarkAgentStyleDirty(this);
    }

    public void Play(int index)
    {
        TryPlay(index);
    }

    public bool TryPlay(int index)
    {
        if (exportData == null || exportData.animations == null || exportData.animations.Count == 0)
            return false;

        animIndex = Mathf.Clamp(index, 0, exportData.animations.Count - 1);
        _animStartTime = Time.time;
        manager?.MarkAgentAnimationTopologyDirty(this);
        return true;
    }

    public void RestartAnimation()
    {
        _animStartTime = Time.time;
        manager?.MarkAgentAnimationDirty(this);
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        manager?.MarkAgentAnimationDirty(this);
    }

    public void Play(string animName)
    {
        TryPlay(animName);
    }

    public bool TryPlay(string animName)
    {
        if (exportData == null || exportData.animations == null) return false;
        for (int i = 0; i < exportData.animations.Count; i++)
        {
            if (exportData.animations[i].animName == animName)
            {
                return TryPlay(i);
            }
        }

        Debug.LogWarning($"[GpuRoleAgent] 未找到动画: {animName}");
        return false;
    }

    public bool HasAnimation(string animName)
    {
        if (exportData == null || exportData.animations == null)
            return false;

        for (int i = 0; i < exportData.animations.Count; i++)
        {
            if (exportData.animations[i].animName == animName)
                return true;
        }

        return false;
    }

    public string GetCurrentAnimationName()
    {
        AnimExportData anim = GetCurrentAnim();
        return anim != null ? anim.animName : string.Empty;
    }

    public void SetGroupVariant(int groupId, int variantIndex)
    {
        TrySetGroupVariant(groupId, variantIndex);
    }

    public bool TrySetGroupVariant(int groupId, int variantIndex)
    {
        EnsureInitialized();
        GroupExportData group = FindGroupById(groupId);
        if (group == null) return false;
        return SetGroupVariantInternal(group, variantIndex);
    }

    public void SetGroupVariant(string groupName, string variantName)
    {
        TrySetGroupVariant(groupName, variantName);
    }

    public bool TrySetGroupVariant(string groupName, string variantName)
    {
        EnsureInitialized();
        if (exportData == null || exportData.groups == null) return false;

        for (int g = 0; g < exportData.groups.Count; g++)
        {
            GroupExportData group = exportData.groups[g];
            if (group.groupName != groupName) continue;

            if (string.IsNullOrEmpty(variantName))
            {
                if (group.slotIndices != null)
                {
                    for (int i = 0; i < group.slotIndices.Length; i++)
                    {
                        int slotIndex = group.slotIndices[i];
                        if (slotIndex >= 0 && slotIndex < _slotVisible.Length)
                            _slotVisible[slotIndex] = false;
                    }
                }
                manager?.MarkAgentStyleDirty(this);
                return true;
            }

            if (group.variants != null)
            {
                for (int v = 0; v < group.variants.Count; v++)
                {
                    if (group.variants[v].variantName == variantName)
                    {
                        return SetGroupVariantInternal(group, v);
                    }
                }
            }

            Debug.LogWarning($"[GpuRoleAgent] Group {groupName} 未找到 Variant: {variantName}");
            return false;
        }

        return false;
    }

    public bool HideGroup(string groupName)
    {
        return TrySetGroupVariant(groupName, string.Empty);
    }

    public void SetSlotSprite(string slotKey, int spriteId, bool force = false)
    {
        TrySetSlotSprite(slotKey, spriteId, force);
    }

    public bool TrySetSlotSprite(string slotKey, int spriteId, bool force = false)
    {
        EnsureInitialized();
        if (!_slotIndexByKey.TryGetValue(slotKey, out int slotIndex))
            return false;

        if (!force && _slotToGroupMap.ContainsKey(slotIndex))
        {
            Debug.LogWarning($"[GpuRoleAgent] Slot {slotKey} 属于 Group，请使用 SetGroupVariant，或 force=true");
            return false;
        }

        _slotSpriteIds[slotIndex] = spriteId;
        _slotVisible[slotIndex] = spriteId >= 0;
        manager?.MarkAgentStyleDirty(this);
        return true;
    }

    public void SetSlotVisible(string slotKey, bool visible, bool force = false)
    {
        TrySetSlotVisible(slotKey, visible, force);
    }

    public bool TrySetSlotVisible(string slotKey, bool visible, bool force = false)
    {
        EnsureInitialized();
        if (!_slotIndexByKey.TryGetValue(slotKey, out int slotIndex))
            return false;

        if (!force && _slotToGroupMap.ContainsKey(slotIndex))
        {
            Debug.LogWarning($"[GpuRoleAgent] Slot {slotKey} 属于 Group，请使用 SetGroupVariant，或 force=true");
            return false;
        }

        _slotVisible[slotIndex] = visible;
        manager?.MarkAgentStyleDirty(this);
        return true;
    }

    public void SetColor(Color c)
    {
        color = c;
        manager?.MarkAgentVisualDirty(this);
    }

    public void SetScale(float s)
    {
        scale = s;
        manager?.MarkAgentVisualDirty(this);
    }

    public void SetAlpha(float alpha)
    {
        color.a = alpha;
        manager?.MarkAgentVisualDirty(this);
    }

    public int[] GetCurrentSlotSpriteIds()
    {
        EnsureInitialized();
        return _slotSpriteIds;
    }

    public bool[] GetCurrentSlotVisible()
    {
        EnsureInitialized();
        return _slotVisible;
    }

    public int GetSlotSpriteId(int slotIndex)
    {
        EnsureInitialized();
        if (_slotSpriteIds == null || slotIndex < 0 || slotIndex >= _slotSpriteIds.Length)
            return -1;
        return _slotSpriteIds[slotIndex];
    }

    public bool IsSlotVisible(int slotIndex)
    {
        EnsureInitialized();
        if (_slotVisible == null || slotIndex < 0 || slotIndex >= _slotVisible.Length)
            return false;
        return _slotVisible[slotIndex];
    }

    public bool TryGetExportSlotIndex(string slotKey, out int slotIndex)
    {
        EnsureInitialized();
        return _slotIndexByKey.TryGetValue(slotKey, out slotIndex);
    }

    public AnimExportData GetCurrentAnim()
    {
        if (exportData == null || exportData.animations == null || exportData.animations.Count == 0)
            return null;

        animIndex = CurrentAnimIndex;
        return exportData.animations[animIndex];
    }

    public Vector4 GetGpuAnimState()
    {
        AnimExportData anim = GetCurrentAnim();
        float frameRate = anim != null ? anim.frameRate : 30f;
        float frameCount = anim != null && anim.frames != null ? Mathf.Max(1, anim.frames.Count) : 1;
        return new Vector4(_animStartTime, playbackSpeed, frameRate, frameCount);
    }

    public Vector4 GetGpuAnimExtraState()
    {
        AnimExportData anim = GetCurrentAnim();
        float animTexY = anim != null ? anim.animDataTexY : 0f;
        return new Vector4(animTexY, 0f, 0f, 0f);
    }

    private bool SetGroupVariantInternal(GroupExportData group, int variantIndex)
    {
        if (group == null || group.variants == null || variantIndex < 0 || variantIndex >= group.variants.Count)
            return false;

        GroupVariant variant = group.variants[variantIndex];
        if (group.slotIndices == null || variant.spriteIds == null)
            return false;

        int count = Mathf.Min(group.slotIndices.Length, variant.spriteIds.Length);
        for (int i = 0; i < count; i++)
        {
            int slotIndex = group.slotIndices[i];
            if (slotIndex < 0 || slotIndex >= _slotSpriteIds.Length) continue;

            int spriteId = variant.spriteIds[i];
            _slotSpriteIds[slotIndex] = spriteId;
            _slotVisible[slotIndex] = spriteId >= 0;
        }

        manager?.MarkAgentStyleDirty(this);
        return true;
    }

    private GroupExportData FindGroupById(int groupId)
    {
        if (exportData == null || exportData.groups == null) return null;
        for (int i = 0; i < exportData.groups.Count; i++)
        {
            if (exportData.groups[i].groupId == groupId)
                return exportData.groups[i];
        }

        return null;
    }

    private void ApplyInitialGroupVariants()
    {
        if (exportData == null || exportData.groups == null || initialGroupVariants == null) return;
        for (int g = 0; g < exportData.groups.Count && g < initialGroupVariants.Length; g++)
            SetGroupVariant(exportData.groups[g].groupName, initialGroupVariants[g]);
    }

    private void ApplyInitialIndependentSlots()
    {
        if (exportData == null || exportData.slots == null || initialIndependentSlotSpriteIds == null) return;

        List<int> independent = GetIndependentSlotIndices();
        for (int i = 0; i < independent.Count && i < initialIndependentSlotSpriteIds.Length; i++)
        {
            int slotIndex = independent[i];
            int spriteId = initialIndependentSlotSpriteIds[i];
            _slotSpriteIds[slotIndex] = spriteId;
            _slotVisible[slotIndex] = spriteId >= 0;
        }
    }

    private List<int> GetIndependentSlotIndices()
    {
        HashSet<int> grouped = new HashSet<int>();
        if (exportData != null && exportData.groups != null)
        {
            for (int g = 0; g < exportData.groups.Count; g++)
            {
                int[] slotIndices = exportData.groups[g].slotIndices;
                if (slotIndices == null) continue;
                for (int i = 0; i < slotIndices.Length; i++)
                    grouped.Add(slotIndices[i]);
            }
        }

        List<int> result = new List<int>();
        if (exportData != null && exportData.slots != null)
        {
            for (int i = 0; i < exportData.slots.Count; i++)
            {
                if (!grouped.Contains(i))
                    result.Add(i);
            }
        }

        return result;
    }
}
