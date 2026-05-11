using System.Collections.Generic;
using UnityEngine;

public class GpuRoleAgent : GpuAgentBase
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

    [Header("Shadow")]
    public bool useShadow = true;
    public bool overrideShadowSettings;
    public Vector2 shadowOffset = Vector2.zero;
    public Vector2 shadowSize = new Vector2(1.4f, 0.35f);
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);

    [System.NonSerialized] public GpuRoleGpuManager manager;
    [System.NonSerialized] public int runtimeIndex = -1;
    [System.NonSerialized] public int sortingOrder = -1;

    private int[] _slotSpriteIds;
    private bool[] _slotVisible;
    private Dictionary<string, int> _slotIndexByKey = new Dictionary<string, int>();
    private Dictionary<int, int> _slotToGroupMap = new Dictionary<int, int>();
    private float _animStartTime;
    private bool _initialized;
    private string _currentAnimName;
    private int _lastScaleFrame = -1;
    private Vector3 _cachedLossyScale;

    public bool IsInitialized => _initialized;
    public float AnimationStartTime => _animStartTime;
    public Vector3 GetCachedLossyScale()
    {
        if (Time.frameCount != _lastScaleFrame)
        {
            _lastScaleFrame = Time.frameCount;
            _cachedLossyScale = transform.lossyScale;
        }
        return _cachedLossyScale;
    }
    public int SlotCount => _slotSpriteIds != null ? _slotSpriteIds.Length : 0;
    public Vector3 Position => transform.position;
    public Vector3 LocalPosition => transform.localPosition;
    public Quaternion Rotation => transform.rotation;
    public Vector3 LocalScale => transform.localScale;
    public bool FlipX => transform.localScale.x < 0f;
    public bool FlipY => transform.localScale.y < 0f;
    public bool Visible => visible;
    public bool RuntimeVisible => visible && isActiveAndEnabled;
    public bool RuntimeShadowVisible => RuntimeVisible && IsShadowEnabled() && GetShadowColor().a > 0f && GetShadowSize().x > 0f && GetShadowSize().y > 0f;
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
        manager.MarkAgentVisualDirty(this);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.MarkAgentVisualDirty(this);
    }

    private void OnDestroy()
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

    public override void SetFlipX(bool flipped)
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
        return true;
    }

    public void RestartAnimation()
    {
        _animStartTime = Time.time;
    }

    /// <summary>
    /// 设置动画播放进度（归一化时间 0~1）
    /// </summary>
    /// <param name="normalizedTime">0~1，0 是动画开头，1 是动画结尾</param>
    public void SetAnimTime(float normalizedTime)
    {
        AnimExportData anim = GetCurrentAnim();
        if (anim == null) return;

        float clampedTime = Mathf.Clamp01(normalizedTime);
        _animStartTime = Time.time - clampedTime * anim.length / Mathf.Max(0.001f, playbackSpeed);
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
    }

    public override void SetAnimSpeed(float speed)
    {
        playbackSpeed = speed;
    }

    public override void Play(string animName)
    {
        TryPlay(animName);
    }

    public bool TryPlay(string animName)
    {
        if (exportData == null || exportData.animations == null) return false;

        // 如果正在播放相同动画，跳过
        if (_currentAnimName == animName)
            return true;

        for (int i = 0; i < exportData.animations.Count; i++)
        {
            if (exportData.animations[i].animName == animName)
            {
                _currentAnimName = animName;
                // 切换动画时保持当前归一化进度，避免跳帧
                AnimExportData oldAnim = GetCurrentAnim();
                float normalizedTime = 0f;
                if (oldAnim != null && oldAnim.length > 0f)
                {
                    float elapsed = Time.time - _animStartTime;
                    normalizedTime = (elapsed * playbackSpeed) / oldAnim.length;
                }

                animIndex = i;
                _animStartTime = Time.time - normalizedTime * exportData.animations[i].length / Mathf.Max(0.001f, playbackSpeed);
                return true;
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
                        {
                            _slotVisible[slotIndex] = false;
                            manager?.TryUpdateSlotVisible(runtimeIndex, slotIndex, false);
                        }
                    }
                }
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

        // 快速路径：图集不变时原地更新 batch buffer，跳过全量重建
        // 如果快速路径失败，数据已写入内存，下次自然重建时会同步到 GPU
        manager?.TryUpdateSlotSprite(runtimeIndex, slotIndex, spriteId);

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

        if (visible)
        {
            int sid = _slotSpriteIds[slotIndex];
            manager?.TryUpdateSlotSprite(runtimeIndex, slotIndex, sid);
        }
        else
        {
            manager?.TryUpdateSlotVisible(runtimeIndex, slotIndex, false);
        }

        return true;
    }

    public void SetColor(Color c)
    {
        color = c;
        manager?.MarkAgentVisualDirty(this);
    }

    public override void SetScale(float s)
    {
        base.SetScale(s);
        manager?.MarkAgentVisualDirty(this);
    }

    public void SetAlpha(float alpha)
    {
        color.a = alpha;
        manager?.MarkAgentVisualDirty(this);
    }

    public override void SetVisible(bool value)
    {
        if (visible == value)
            return;

        base.SetVisible(value);
        manager?.MarkAgentVisualDirty(this);
    }

    public void SetShadowVisible(bool value)
    {
        if (useShadow == value)
            return;

        useShadow = value;
        manager?.MarkAgentVisualDirty(this);
    }

    public void SetShadow(Vector2 offset, Vector2 size, Color color)
    {
        overrideShadowSettings = true;
        shadowOffset = offset;
        shadowSize = size;
        shadowColor = color;
        manager?.MarkAgentVisualDirty(this);
    }

    public Vector2 GetShadowOffset()
    {
        return overrideShadowSettings || exportData == null ? shadowOffset : exportData.shadowOffset;
    }

    public Vector2 GetShadowSize()
    {
        return overrideShadowSettings || exportData == null ? shadowSize : exportData.shadowSize;
    }

    public Color GetShadowColor()
    {
        return overrideShadowSettings || exportData == null ? shadowColor : exportData.shadowColor;
    }

    public bool IsShadowEnabled()
    {
        return useShadow && (overrideShadowSettings || exportData == null || exportData.useShadow);
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

    public bool IsSlotInGroup(string slotKey)
    {
        EnsureInitialized();
        if (!_slotIndexByKey.TryGetValue(slotKey, out int slotIndex))
            return false;
        return _slotToGroupMap.ContainsKey(slotIndex);
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

            // 快速路径：slot 在 batch 中则原地更新 GPU，不在则等下次重建同步
            if (spriteId >= 0)
                manager?.TryUpdateSlotSprite(runtimeIndex, slotIndex, spriteId);
            else
                manager?.TryUpdateSlotVisible(runtimeIndex, slotIndex, false);
        }

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

    // ===== 随机换装 =====

    /// <summary>
    /// 随机换装（考虑互斥组约束）
    /// 对每个 Group 随机选一个 Variant，对每个独立 Slot 随机选一个 Sprite
    /// 互斥组内只会有一个成员被选中
    /// </summary>
    /// <param name="groupNoneChance">Group 隐藏概率（0~1）</param>
    /// <param name="independentNoneChance">独立 Slot 隐藏概率（0~1）</param>
    public void RandomizeStyle(float groupNoneChance = 0f, float independentNoneChance = 0.1f)
    {
        EnsureInitialized();
        if (exportData == null) return;

        // 1. 随机 Group Variants（考虑互斥）
        if (exportData.groups != null)
        {
            // 先收集所有互斥组中已选中的 groupId，避免重复处理
            HashSet<int> processedGroups = new HashSet<int>();

            // 如果有互斥数据，按互斥组处理
            if (exportData.exclusiveGroups != null && exportData.exclusiveGroups.Count > 0)
            {
                foreach (var eg in exportData.exclusiveGroups)
                {
                    if (eg == null) continue;

                    // 从互斥组中随机选一个成员
                    List<ExclusiveCandidate> candidates = new List<ExclusiveCandidate>();
                    if (eg.memberGroupIds != null)
                    {
                        for (int i = 0; i < eg.memberGroupIds.Count; i++)
                            candidates.Add(new ExclusiveCandidate(true, eg.memberGroupIds[i]));
                    }
                    if (eg.memberSlotIndices != null)
                    {
                        for (int i = 0; i < eg.memberSlotIndices.Count; i++)
                        {
                            int slotIndex = eg.memberSlotIndices[i];
                            if (!IsSlotOwnedByExclusiveMemberGroup(eg, slotIndex))
                                candidates.Add(new ExclusiveCandidate(false, slotIndex));
                        }
                    }

                    if (candidates.Count == 0) continue;

                    // 互斥组是否允许全隐藏（由数据中的 canBeNone 决定）
                    if (eg.canBeNone)
                        candidates.Add(new ExclusiveCandidate(false, -1)); // "none" sentinel

                    // 随机选一个
                    ExclusiveCandidate chosen = candidates[Random.Range(0, candidates.Count)];

                    if (chosen.id == -1) // "none" 被选中，所有成员隐藏
                    {
                        if (eg.memberGroupIds != null)
                        {
                            for (int i = 0; i < eg.memberGroupIds.Count; i++)
                            {
                                GroupExportData g = FindGroupById(eg.memberGroupIds[i]);
                                if (g != null && g.slotIndices != null)
                                {
                                    for (int si = 0; si < g.slotIndices.Length; si++)
                                    {
                                        int slotIdx = g.slotIndices[si];
                                        if (slotIdx >= 0 && slotIdx < _slotVisible.Length)
                                        {
                                            _slotVisible[slotIdx] = false;
                                            manager?.TryUpdateSlotVisible(runtimeIndex, slotIdx, false);
                                        }
                                    }
                                }
                                processedGroups.Add(eg.memberGroupIds[i]);
                            }
                        }
                        if (eg.memberSlotIndices != null)
                        {
                            for (int i = 0; i < eg.memberSlotIndices.Count; i++)
                            {
                                int idx = eg.memberSlotIndices[i];
                                if (!IsSlotOwnedByExclusiveMemberGroup(eg, idx) && idx >= 0 && idx < _slotVisible.Length)
                                {
                                    _slotVisible[idx] = false;
                                    _slotSpriteIds[idx] = -1;
                                    manager?.TryUpdateSlotVisible(runtimeIndex, idx, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 如果是 group，随机选 variant
                        if (chosen.isGroup)
                        {
                            GroupExportData group = FindGroupById(chosen.id);
                            if (group != null && group.variants != null && group.variants.Count > 0)
                            {
                                int vi = Random.Range(0, group.variants.Count);
                                SetGroupVariantInternal(group, vi);
                            }
                            processedGroups.Add(chosen.id);
                        }
                        // 如果是独立 slot，随机选 sprite
                        else
                        {
                            RandomizeSingleSlot(chosen.id, independentNoneChance);
                            SyncSlotToManager(chosen.id);
                        }

                        // 收集选中 group 的 slot indices，避免后续隐藏时覆盖
                        HashSet<int> chosenSlots = null;
                        if (chosen.isGroup)
                        {
                            GroupExportData chosenGroup = FindGroupById(chosen.id);
                            if (chosenGroup != null && chosenGroup.slotIndices != null)
                            {
                                chosenSlots = new HashSet<int>(chosenGroup.slotIndices);
                            }
                        }

                        // 互斥组中其他成员全部隐藏
                        if (eg.memberGroupIds != null) foreach (var gId in eg.memberGroupIds)
                        {
                            if (!(chosen.isGroup && gId == chosen.id) && !processedGroups.Contains(gId))
                            {
                                GroupExportData otherGroup = FindGroupById(gId);
                                if (otherGroup != null && otherGroup.slotIndices != null)
                                {
                                    for (int si = 0; si < otherGroup.slotIndices.Length; si++)
                                    {
                                        int slotIdx = otherGroup.slotIndices[si];
                                        if (slotIdx < 0 || slotIdx >= _slotVisible.Length) continue;
                                        // 跳过属于选中组的 slot，避免覆盖选中组的显示
                                        if (chosenSlots != null && chosenSlots.Contains(slotIdx))
                                            continue;
                                        _slotVisible[slotIdx] = false;
                                        manager?.TryUpdateSlotVisible(runtimeIndex, slotIdx, false);
                                    }
                                }
                                processedGroups.Add(gId);
                            }
                        }
                        if (eg.memberSlotIndices != null) foreach (var idx in eg.memberSlotIndices)
                        {
                            if (IsSlotOwnedByExclusiveMemberGroup(eg, idx))
                                continue;

                            if (!(chosen.isGroup == false && idx == chosen.id) && idx >= 0 && idx < _slotVisible.Length)
                            {
                                _slotVisible[idx] = false;
                                _slotSpriteIds[idx] = -1;
                                manager?.TryUpdateSlotVisible(runtimeIndex, idx, false);
                            }
                        }
                    }
                }
            }

            // 处理不在互斥组中的 Group
            for (int g = 0; g < exportData.groups.Count; g++)
            {
                GroupExportData group = exportData.groups[g];
                if (group == null || group.variants == null || group.variants.Count == 0)
                    continue;
                if (processedGroups.Contains(group.groupId))
                    continue;

                if (group.canBeEmpty && groupNoneChance > 0f && Random.value < groupNoneChance)
                {
                    // 隐藏整个组
                    if (group.slotIndices != null)
                    {
                        for (int si = 0; si < group.slotIndices.Length; si++)
                        {
                            int slotIdx = group.slotIndices[si];
                            if (slotIdx >= 0 && slotIdx < _slotVisible.Length)
                            {
                                _slotVisible[slotIdx] = false;
                                manager?.TryUpdateSlotVisible(runtimeIndex, slotIdx, false);
                            }
                        }
                    }
                }
                else
                {
                    int vi = Random.Range(0, group.variants.Count);
                    SetGroupVariantInternal(group, vi);
                }
            }
        }

        // 2. 随机独立 Slot（考虑互斥）
        if (exportData.slots != null)
        {
            HashSet<int> processedSlots = new HashSet<int>();

            // 标记所有属于 group 的 slot
            if (exportData.groups != null)
            {
                for (int g = 0; g < exportData.groups.Count; g++)
                {
                    if (exportData.groups[g].slotIndices != null)
                    {
                        for (int si = 0; si < exportData.groups[g].slotIndices.Length; si++)
                            processedSlots.Add(exportData.groups[g].slotIndices[si]);
                    }
                }
            }

            // 标记已在互斥组中处理过的 slot（无论 visible 状态，防止后面重新随机）
            if (exportData.exclusiveGroups != null)
            {
                foreach (var eg in exportData.exclusiveGroups)
                {
                    if (eg == null || eg.memberSlotIndices == null) continue;

                    foreach (var idx in eg.memberSlotIndices)
                    {
                        if (idx >= 0 && idx < _slotVisible.Length)
                            processedSlots.Add(idx);
                    }
                }
            }

            for (int s = 0; s < exportData.slots.Count; s++)
            {
                if (processedSlots.Contains(s))
                    continue;

                RandomizeSingleSlot(s, independentNoneChance);
                // 独立 slot 随机后尝试快速路径更新 GPU
                if (manager != null)
                {
                    int sid = _slotSpriteIds[s];
                    if (sid >= 0)
                        manager.TryUpdateSlotSprite(runtimeIndex, s, sid);
                    else
                        manager.TryUpdateSlotVisible(runtimeIndex, s, false);
                }
            }
        }
    }

    private struct ExclusiveCandidate
    {
        public readonly bool isGroup;
        public readonly int id;

        public ExclusiveCandidate(bool isGroup, int id)
        {
            this.isGroup = isGroup;
            this.id = id;
        }
    }

    private void SyncSlotToManager(int slotIndex)
    {
        if (manager == null || slotIndex < 0 || _slotSpriteIds == null || _slotVisible == null ||
            slotIndex >= _slotSpriteIds.Length || slotIndex >= _slotVisible.Length)
            return;

        int spriteId = _slotSpriteIds[slotIndex];
        if (_slotVisible[slotIndex] && spriteId >= 0)
            manager.TryUpdateSlotSprite(runtimeIndex, slotIndex, spriteId);
        else
            manager.TryUpdateSlotVisible(runtimeIndex, slotIndex, false);
    }

    private bool IsSlotOwnedByExclusiveMemberGroup(ExclusiveGroupExportData exclusiveGroup, int slotIndex)
    {
        if (exclusiveGroup == null || exclusiveGroup.memberGroupIds == null || slotIndex < 0)
            return false;

        for (int i = 0; i < exclusiveGroup.memberGroupIds.Count; i++)
        {
            GroupExportData group = FindGroupById(exclusiveGroup.memberGroupIds[i]);
            if (group == null || group.slotIndices == null)
                continue;

            for (int s = 0; s < group.slotIndices.Length; s++)
            {
                if (group.slotIndices[s] == slotIndex)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 随机换装（完整版，含动画、颜色、缩放）
    /// </summary>
    public void RandomizeFull(
        float groupNoneChance = 0f,
        float independentNoneChance = 0.1f,
        bool randomAnim = true,
        bool randomColor = false,
        bool randomScale = false,
        float colorSaturation = 0.25f,
        float colorValue = 1f,
        Vector2? scaleRange = null)
    {
        RandomizeStyle(groupNoneChance, independentNoneChance);

        if (randomAnim && exportData != null && exportData.animations != null && exportData.animations.Count > 0)
        {
            animIndex = Random.Range(0, exportData.animations.Count);
            _animStartTime = Time.time;
        }

        if (randomColor)
        {
            Color c = Color.HSVToRGB(Random.value, colorSaturation, colorValue);
            c.a = color.a;
            color = c;
            manager?.MarkAgentVisualDirty(this);
        }

        if (randomScale)
        {
            Vector2 sr = scaleRange ?? new Vector2(0.9f, 1.1f);
            scale = Random.Range(sr.x, sr.y);
            manager?.MarkAgentVisualDirty(this);
        }
    }

    /// <summary>
    /// 随机单个独立 Slot 的 Sprite
    /// </summary>
    private void RandomizeSingleSlot(int slotIndex, float noneChance)
    {
        if (slotIndex < 0 || slotIndex >= exportData.slots.Count)
            return;

        SlotExportData slotData = exportData.slots[slotIndex];
        if (slotData == null) return;

        if (slotData.canBeEmpty && noneChance > 0f && Random.value < noneChance)
        {
            _slotVisible[slotIndex] = false;
            _slotSpriteIds[slotIndex] = -1;
            return;
        }

        if (slotData.availableSpriteIds == null || slotData.availableSpriteIds.Length == 0)
            return;

        int spriteId = slotData.availableSpriteIds[Random.Range(0, slotData.availableSpriteIds.Length)];
        _slotSpriteIds[slotIndex] = spriteId;
        _slotVisible[slotIndex] = spriteId >= 0;
    }

}
