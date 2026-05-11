using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GpuRoleGpuManager : MonoBehaviour
{
    public Shader shader;
    public Shader shadowShader;
    public Camera clipBoundCamera;

    [Header("Runtime")]
    public int maxCharacterCount = 2000;
    public float pixelsPerUnit = 32f;
    [Range(0f, 1f)] public float alphaClipThreshold = 0.01f;

    [Header("Bounds")]
    public bool showBounds = true;
    public Vector3 drawBoundsCenter = Vector3.zero;
    public Vector3 drawBoundsSize = new Vector3(10000f, 10000f, 10000f);

    [HideInInspector]
    public bool useYDepthSorting = true;

    [HideInInspector]
    public bool lowerYIsCloser = true;

    [HideInInspector]
    public bool preserveAgentZ;

    [HideInInspector]
    public float yToZScale = 0.01f;

    [HideInInspector]
    public float sameDepthTieZStep = 0.01f;

    [HideInInspector]
    public float sortingOrderDepthStep = 0.0001f;

    [HideInInspector]
    public bool laterAgentIsCloser = true;

    [HideInInspector]
    public float depthBaseZ = 0f;

    [HideInInspector]
    public bool writeDepth = true;

    [HideInInspector]
    public bool drawShadow = true;

    [Header("Culling")]
    public bool cullByCamera = true;
    public float cullPadding = 2f;

    [Header("Performance")]
    public bool autoRebuild = true;

    private int instanceCount;
    private int rebuildCount;

    private readonly List<GpuRoleAgent> _agents = new List<GpuRoleAgent>();
    private readonly Dictionary<int, SpriteUVData> _uvBySpriteId = new Dictionary<int, SpriteUVData>();
    private readonly Dictionary<int, Matrix4x4> _spriteMatrixCache = new Dictionary<int, Matrix4x4>();
    private readonly HashSet<GpuRoleExportData> _cachedExportDataSet = new HashSet<GpuRoleExportData>();
    private readonly Dictionary<GpuRoleExportData, Dictionary<string, int>> _slotIndexCache = new Dictionary<GpuRoleExportData, Dictionary<string, int>>();
private readonly Dictionary<AnimExportData, int[]> _animSlotToExportSlotCache = new Dictionary<AnimExportData, int[]>();
    private readonly Dictionary<BatchKey, AtlasBatch> _batchMap = new Dictionary<BatchKey, AtlasBatch>();
    private readonly List<AtlasBatch> _batches = new List<AtlasBatch>();
    private readonly Dictionary<(int agentIndex, int exportSlotIndex), List<(AtlasBatch batch, int instanceIndex)>> _slotLocationMap = new Dictionary<(int, int), List<(AtlasBatch, int)>>();
    private readonly List<SpriteUVData> _slotBuildSprites = new List<SpriteUVData>(4);
    private Material _material;
    private Material _shadowMaterial;
    private MaterialPropertyBlock _shadowMpb;
    private Mesh _quadMesh;
    private Matrix4x4[] _agentMatrices;
    private Vector4[] _agentAnimData;
    private Vector4[] _agentAnimExtraData;
    private Vector4[] _agentColors;
    private Vector4[] _shadowData;
    private Vector4[] _shadowColors;
    private int[] _shadowRenderIndices;
    private ComputeBuffer _agentMatrixBuffer;
    private ComputeBuffer _agentAnimBuffer;
    private ComputeBuffer _agentAnimExtraBuffer;
    private ComputeBuffer _agentColorBuffer;
    private ComputeBuffer _shadowDataBuffer;
    private ComputeBuffer _shadowColorBuffer;
    private ComputeBuffer _shadowRenderIndexBuffer;
    private ComputeBuffer _shadowArgsBuffer;
    private bool _topologyDirty = true;
    private float _lastAppliedAlphaClipThreshold = -1f;
    private int _lastAppliedWriteDepth = -1;
    private Bounds _drawBounds;
    private Vector3 _lastDrawBoundsCenter;
    private Vector3 _lastDrawBoundsSize;
    private int _shadowRenderCount;
    private Vector2 _cachedCullExtents = new Vector2(2f, 2f);
    private bool[] _culledFlags = System.Array.Empty<bool>();
    private int[] _agentRemap;
    private int[] _visibleAgentIndices;
    private ComputeBuffer _agentRemapBuffer;

    private struct BatchKey : System.IEquatable<BatchKey>
    {
        public int atlasIndex;
        public int animIndex;
        public int internalOrder;

        public BatchKey(int atlasIndex, int animIndex, int internalOrder)
        {
            this.atlasIndex = atlasIndex;
            this.animIndex = animIndex;
            this.internalOrder = internalOrder;
        }

        public bool Equals(BatchKey other)
        {
            return atlasIndex == other.atlasIndex &&
                   animIndex == other.animIndex &&
                   internalOrder == other.internalOrder;
        }

        public override bool Equals(object obj)
        {
            return obj is BatchKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = atlasIndex;
                hash = (hash * 397) ^ animIndex;
                hash = (hash * 397) ^ internalOrder;
                return hash;
            }
        }
    }

    private class AtlasBatch
    {
        public BatchKey key;
        public MaterialPropertyBlock mpb;
        public int count;
        public ComputeBuffer instanceDataBuffer;
        public ComputeBuffer uvBuffer;
        public ComputeBuffer spriteMatrixBuffer;
        public ComputeBuffer argsBuffer;
    }

    private struct InstanceBuildData
    {
        public int agentIndex;
        public int animSlotIndex;
        public int exportSlotIndex; // 用于批次内排序
        public SpriteUVData uv;
        public bool visible;
    }

    private void Awake()
    {
        ApplyManagedSettings();

        if (shader == null)
            shader = Shader.Find("GpuPaperDoll/GpuRuntime");
        if (shadowShader == null)
            shadowShader = Shader.Find("GpuPaperDoll/GpuRoleShadow");

        if (shader == null)
        {
            Debug.LogError("[GpuRoleGpuManager] Missing shader: GpuPaperDoll/GpuRuntime");
            enabled = false;
            return;
        }

        _material = new Material(shader);
        if (shadowShader != null)
            _shadowMaterial = new Material(shadowShader);
        _quadMesh = CreateQuadMesh();
        RefreshDrawBounds(true);
        EnsureAgentBuffers();
    }

    private void OnValidate()
    {
        ApplyManagedSettings();
        RefreshDrawBounds(true);
    }

    private void ApplyManagedSettings()
    {
        useYDepthSorting = true;
        lowerYIsCloser = true;
        preserveAgentZ = false;
        laterAgentIsCloser = true;
        writeDepth = true;
        yToZScale = Gpu2DDepthUtility.YToZScale;
        sameDepthTieZStep = Gpu2DDepthUtility.SameDepthTieZStep;
        sortingOrderDepthStep = Gpu2DDepthUtility.SortingOrderDepthStep;
    }

    public void Register(GpuRoleAgent agent)
    {
        if (agent == null || _agents.Contains(agent)) return;
        if (_agents.Count >= maxCharacterCount)
        {
            Debug.LogError($"[GpuRoleGpuManager] Agent count exceeds maxCharacterCount: {maxCharacterCount}");
            return;
        }

        agent.EnsureInitialized();
        agent.runtimeIndex = _agents.Count;
        agent.sortingOrder = AcquireSortingOrder();
        _agents.Add(agent);
        agent.manager = this;
        CacheExportData(agent.exportData);
        EnsureAgentBuffers();
        if (autoRebuild)
            _topologyDirty = true;
    }

    public void Unregister(GpuRoleAgent agent)
    {
        if (agent == null) return;
        int index = _agents.IndexOf(agent);
        if (index < 0) return;

        _agents.RemoveAt(index);
        ReleaseSortingOrder(agent.sortingOrder);
        if (agent.manager == this)
            agent.manager = null;
        agent.runtimeIndex = -1;
        agent.sortingOrder = -1;
    }

    public void MarkAgentStyleDirty(GpuRoleAgent agent)
    {
        if (autoRebuild)
            _topologyDirty = true;
    }

    public void MarkAgentAnimationDirty(GpuRoleAgent agent)
    {
    }

    public void MarkAgentAnimationTopologyDirty(GpuRoleAgent agent)
    {
        if (autoRebuild)
            _topologyDirty = true;
    }

    public void MarkAgentVisualDirty(GpuRoleAgent agent)
    {
    }

    private int _fastPathHit;
    private int _fastPathMiss;
    private int _fastPathMissNoLocation;
    private int _fastPathMissAtlasChanged;

    private void LogFastPathStats()
    {
        if (_fastPathHit + _fastPathMiss > 0)
        {
            Debug.Log($"[GpuRoleGpuManager] FastPath: hit={_fastPathHit} miss={_fastPathMiss} (noLocation={_fastPathMissNoLocation} atlasChanged={_fastPathMissAtlasChanged})");
            _fastPathHit = 0;
            _fastPathMiss = 0;
            _fastPathMissNoLocation = 0;
            _fastPathMissAtlasChanged = 0;
        }
    }

    /// <summary>
    /// 换装快速路径：spriteId 不变图集时原地更新 batch buffer，避免全量 RebuildBatches。
    /// </summary>
    public bool TryUpdateSlotSprite(int agentRuntimeIndex, int exportSlotIndex, int spriteId)
    {
        var locKey = (agentRuntimeIndex, exportSlotIndex);
        if (!_slotLocationMap.TryGetValue(locKey, out var locList) || locList.Count == 0)
        {
            _fastPathMiss++;
            _fastPathMissNoLocation++;
            return false;
        }

        Vector4 uvRect;
        Matrix4x4 matrix;
        int targetAtlasIndex = -1;

        if (spriteId >= 0 && _uvBySpriteId.TryGetValue(spriteId, out SpriteUVData uv))
        {
            // Atlas changes are handled by lighting the matching prebuilt placeholder.
            targetAtlasIndex = uv.atlasIndex;
            uvRect = new Vector4(uv.uMin, uv.vMin, uv.uMax, uv.vMax);
            _spriteMatrixCache.TryGetValue(spriteId, out matrix);
        }
        else
        {
            // spriteId < 0（隐藏）或查不到 UV 时，退化为零区域
            uvRect = Vector4.zero;
            matrix = Matrix4x4.identity;
        }

        bool updated = false;
        for (int i = 0; i < locList.Count; i++)
        {
            var (batch, instanceIdx) = locList[i];
            bool isTargetAtlas = targetAtlasIndex < 0 || batch.key.atlasIndex == targetAtlasIndex;
            batch.uvBuffer.SetData(new[] { isTargetAtlas ? uvRect : Vector4.zero }, 0, instanceIdx, 1);
            batch.spriteMatrixBuffer.SetData(new[] { isTargetAtlas ? matrix : Matrix4x4.identity }, 0, instanceIdx, 1);
            updated |= isTargetAtlas;
        }

        if (!updated)
        {
            _fastPathMiss++;
            _fastPathMissAtlasChanged++;
            return false;
        }

        _fastPathHit++;
        return true;
    }

    /// <summary>
    /// 隐藏/显示 slot 快速路径：原地更新 UV rect，避免全量重建。
    /// </summary>
    public bool TryUpdateSlotVisible(int agentRuntimeIndex, int exportSlotIndex, bool visible, int spriteId = -1)
    {
        // 显示时走 TryUpdateSlotSprite（含 atlas 一致性校验）
        if (visible && spriteId >= 0)
            return TryUpdateSlotSprite(agentRuntimeIndex, exportSlotIndex, spriteId);

        var locKey = (agentRuntimeIndex, exportSlotIndex);
        if (!_slotLocationMap.TryGetValue(locKey, out var locList) || locList.Count == 0)
            return false;

        Vector4 uvRect = Vector4.zero;
        Matrix4x4 matrix = Matrix4x4.identity;

        for (int i = 0; i < locList.Count; i++)
        {
            var (batch, instanceIdx) = locList[i];
            batch.uvBuffer.SetData(new[] { uvRect }, 0, instanceIdx, 1);
            batch.spriteMatrixBuffer.SetData(new[] { matrix }, 0, instanceIdx, 1);
        }

        return true;
    }

    public int AgentCount => _agents.Count;
    public int BatchCount => _batches.Count;
    public int InstanceCount => instanceCount;
    public int RebuildCount => rebuildCount;

    /// <summary>
    /// 手动触发一次全量重建
    /// </summary>
    public void Rebuild()
    {
        _topologyDirty = true;
    }

    private float _nextFastPathStatsTime;

    private void LateUpdate()
    {
        if (_material == null || _quadMesh == null)
            return;

        if (_agentMatrixBuffer == null || _agentMatrices == null)
            EnsureAgentBuffers();

        if (Time.time >= _nextFastPathStatsTime)
        {
            LogFastPathStats();
            _nextFastPathStatsTime = Time.time + 1f;
        }

        if (_topologyDirty)
            RebuildBatches();

        UpdateBatchMaterialProperties();
        RefreshDrawBounds(false);

        // 视锥体裁剪
        Rect cameraBounds = default;
        bool canCull = cullByCamera && clipBoundCamera != null && TryGetCameraBounds(out cameraBounds);

        UploadAgentBuffers(canCull ? cameraBounds : default(Rect?));

        DrawShadows();

        for (int i = 0; i < _batches.Count; i++)
        {
            AtlasBatch batch = _batches[i];
            if (batch.count == 0) continue;

            Graphics.DrawMeshInstancedIndirect(
                _quadMesh,
                0,
                _material,
                _drawBounds,
                batch.argsBuffer,
                0,
                batch.mpb,
                ShadowCastingMode.Off,
                false,
                gameObject.layer,
                null
            );
        }
    }

    private void RebuildBatches()
    {
        ReleaseBatches();

        Dictionary<BatchKey, List<InstanceBuildData>> grouped = new Dictionary<BatchKey, List<InstanceBuildData>>();
        for (int a = 0; a < _agents.Count; a++)
        {
            GpuRoleAgent agent = _agents[a];
            if (agent == null || agent.exportData == null)
                continue;

            CacheExportData(agent.exportData);
            agent.EnsureInitialized();

            AnimExportData anim = agent.GetCurrentAnim();
            if (anim == null || anim.slotKeys == null)
                continue;

            int[] spriteIds = agent.GetCurrentSlotSpriteIds();
            bool[] visible = agent.GetCurrentSlotVisible();
            if (spriteIds == null || visible == null)
                continue;

            int[] animSlotToExportSlot = GetAnimSlotToExportSlotMap(agent.exportData, anim);
            if (animSlotToExportSlot == null)
                continue;

            for (int animSlotIndex = 0; animSlotIndex < anim.slotKeys.Count; animSlotIndex++)
            {
                int exportSlotIndex = animSlotIndex < animSlotToExportSlot.Length ? animSlotToExportSlot[animSlotIndex] : -1;
                if (exportSlotIndex < 0)
                    continue;

                if (exportSlotIndex < 0 || exportSlotIndex >= spriteIds.Length || exportSlotIndex >= visible.Length)
                    continue;

                int spriteId = visible[exportSlotIndex] ? spriteIds[exportSlotIndex] : -1;
                _slotBuildSprites.Clear();
                CollectBatchSprites(agent.exportData, exportSlotIndex, spriteId, _slotBuildSprites);
                if (_slotBuildSprites.Count == 0)
                    continue;

                int internalOrder = GetBatchOrder(agent.exportData, exportSlotIndex);
                int animBatchIndex = UsesCombinedAnimTexture(agent.exportData) ? 0 : agent.CurrentAnimIndex;
                for (int buildIndex = 0; buildIndex < _slotBuildSprites.Count; buildIndex++)
                {
                    SpriteUVData uv = _slotBuildSprites[buildIndex];
                    BatchKey key = new BatchKey(uv.atlasIndex, animBatchIndex, internalOrder);
                    if (!grouped.TryGetValue(key, out List<InstanceBuildData> list))
                    {
                        list = new List<InstanceBuildData>();
                        grouped.Add(key, list);
                    }

                    list.Add(new InstanceBuildData
                    {
                        agentIndex = agent.runtimeIndex,
                        animSlotIndex = animSlotIndex,
                        exportSlotIndex = exportSlotIndex,
                        uv = uv,
                        visible = spriteId >= 0 && uv.spriteId == spriteId
                    });
                }
            }
        }

        // 缓存 exportData 查询结果（同一个 animIndex 共享）
        var exportDataCache = new Dictionary<int, GpuRoleExportData>();
        foreach (var kv in grouped)
        {
            // 批次内按导出序号排序，保证渲染顺序正确
            kv.Value.Sort((a, b) => a.exportSlotIndex.CompareTo(b.exportSlotIndex));
            int animIdx = kv.Key.animIndex;
            if (!exportDataCache.TryGetValue(animIdx, out GpuRoleExportData exportDataForBatch))
            {
                exportDataForBatch = FindExportDataForBatch(animIdx);
                exportDataCache[animIdx] = exportDataForBatch;
            }
            CreateBatch(kv.Key, kv.Value, exportDataForBatch);
        }

        _batches.Sort((a, b) =>
        {
            int order = a.key.internalOrder.CompareTo(b.key.internalOrder);
            if (order != 0) return order;

            int anim = a.key.animIndex.CompareTo(b.key.animIndex);
            if (anim != 0) return anim;

            return a.key.atlasIndex.CompareTo(b.key.atlasIndex);
        });

        instanceCount = 0;
        for (int i = 0; i < _batches.Count; i++)
            instanceCount += _batches[i].count;
        rebuildCount++;
        _topologyDirty = false;
    }

    private void CreateBatch(BatchKey key, List<InstanceBuildData> instances, GpuRoleExportData exportData)
    {
        if (instances == null || instances.Count == 0)
            return;

        if (exportData == null || exportData.atlases == null || key.atlasIndex < 0 || key.atlasIndex >= exportData.atlases.Count)
            return;

        AtlasData atlas = exportData.atlases[key.atlasIndex];
        if (atlas == null || atlas.texture == null)
            return;

        AnimExportData anim = exportData.animations[key.animIndex];
        Texture2D animTex = GetRuntimeAnimTexture(exportData, anim);
        if (anim == null || animTex == null)
            return;

        int count = instances.Count;
        Vector4[] instanceData = new Vector4[count];
        Vector4[] uvRects = new Vector4[count];
        Matrix4x4[] spriteMatrices = new Matrix4x4[count];

        for (int i = 0; i < count; i++)
        {
            InstanceBuildData instance = instances[i];
            instanceData[i] = new Vector4(instance.agentIndex, instance.animSlotIndex, 0f, 0f);
            if (instance.visible)
            {
                uvRects[i] = new Vector4(instance.uv.uMin, instance.uv.vMin, instance.uv.uMax, instance.uv.vMax);
                spriteMatrices[i] = _spriteMatrixCache.TryGetValue(instance.uv.spriteId, out Matrix4x4 cached)
                    ? cached : CreateSpriteMatrix(instance.uv);
            }
            else
            {
                uvRects[i] = Vector4.zero;
                spriteMatrices[i] = Matrix4x4.identity;
            }
        }

        AtlasBatch batch = new AtlasBatch
        {
            key = key,
            count = count,
            mpb = new MaterialPropertyBlock(),
            instanceDataBuffer = new ComputeBuffer(count, sizeof(float) * 4),
            uvBuffer = new ComputeBuffer(count, sizeof(float) * 4),
            spriteMatrixBuffer = new ComputeBuffer(count, sizeof(float) * 16),
            argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments)
        };

        batch.instanceDataBuffer.SetData(instanceData);
        batch.uvBuffer.SetData(uvRects);
        batch.spriteMatrixBuffer.SetData(spriteMatrices);

        // 记录 slot 位置索引，用于换装时原地更新
        for (int i = 0; i < count; i++)
        {
            var instance = instances[i];
            var locKey = (instance.agentIndex, instance.exportSlotIndex);
            if (!_slotLocationMap.TryGetValue(locKey, out var locList))
            {
                locList = new List<(AtlasBatch, int)>();
                _slotLocationMap[locKey] = locList;
            }
            locList.Add((batch, i));
        }

        uint[] args = new uint[5];
        args[0] = _quadMesh.GetIndexCount(0);
        args[1] = (uint)count;
        args[2] = _quadMesh.GetIndexStart(0);
        args[3] = _quadMesh.GetBaseVertex(0);
        args[4] = 0;
        batch.argsBuffer.SetData(args);

        batch.mpb.SetTexture("_MainTex", atlas.texture);
        batch.mpb.SetTexture("_AnimTex", animTex);
        batch.mpb.SetFloat("_AlphaClipThreshold", alphaClipThreshold);
        batch.mpb.SetFloat("_AnimSlotCount", GetRuntimeAnimTexWidth(exportData, anim) / 3f);
        batch.mpb.SetFloat("_AnimTexHeight", GetRuntimeAnimTexHeight(exportData, anim));
        batch.mpb.SetBuffer("_AgentMatrices", _agentMatrixBuffer);
        batch.mpb.SetBuffer("_AgentAnimData", _agentAnimBuffer);
        batch.mpb.SetBuffer("_AgentAnimExtraData", _agentAnimExtraBuffer);
        batch.mpb.SetBuffer("_AgentColors", _agentColorBuffer);
        batch.mpb.SetBuffer("_AgentRemap", _agentRemapBuffer);
        batch.mpb.SetBuffer("_InstanceData", batch.instanceDataBuffer);
        batch.mpb.SetBuffer("_InstanceUVRects", batch.uvBuffer);
        batch.mpb.SetBuffer("_InstanceSpriteMatrices", batch.spriteMatrixBuffer);

        _batchMap[key] = batch;
        _batches.Add(batch);
    }

    private void UploadAgentBuffers(Rect? cameraBounds = null)
    {
        int agentCount = _agents.Count;
        bool depthSorting = useYDepthSorting;
        _shadowRenderCount = 0;
        bool hasCullBounds = cameraBounds.HasValue;
        Rect cullRect = cameraBounds.GetValueOrDefault();
        int capacity = _agentMatrices.Length;
        int dummySlot = capacity - 1;
        int visibleCount = 0;

        for (int i = 0; i < agentCount; i++)
        {
            GpuRoleAgent agent = _agents[i];
            if (agent == null)
                continue;

            Vector3 finalScale = agent.GetCachedLossyScale() * agent.scale;
            Vector3 position = agent.transform.position;
            if (depthSorting)
                position.z = CalculateDepthZ(position, agent, i);

            bool isCulled = hasCullBounds && !IsAgentVisible(position, finalScale, cullRect);
            _culledFlags[i] = isCulled;

            if (isCulled)
            {
                _agentRemap[i] = dummySlot;
                goto ShadowData;
            }

            // Visible agent: compute and store at compacted position
            {
                int idx = visibleCount;
                _visibleAgentIndices[visibleCount++] = i;
                _agentRemap[i] = idx;

                _agentMatrices[idx] = Matrix4x4.TRS(position, agent.transform.rotation, finalScale);
                _agentAnimData[idx] = agent.GetGpuAnimState();
                Vector4 animExtra = agent.GetGpuAnimExtraState();
                animExtra.y = depthSorting ? CalculateSortingDepthBias(agent, i) : 0f;
                _agentAnimExtraData[idx] = animExtra;

                Color c = agent.color;
                if (!agent.RuntimeVisible)
                    c.a = 0f;
                _agentColors[idx] = new Vector4(c.r, c.g, c.b, c.a);
            }

            ShadowData:
            Vector2 shadowOffset = agent.GetShadowOffset();
            Vector2 shadowSize = agent.GetShadowSize();
            Color shadowColor = agent.GetShadowColor();
            _shadowData[i] = new Vector4(shadowOffset.x, shadowOffset.y, shadowSize.x, shadowSize.y);
            _shadowColors[i] = new Vector4(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a);
            if (drawShadow && agent.RuntimeShadowVisible && !isCulled && _shadowRenderCount < _shadowRenderIndices.Length)
                _shadowRenderIndices[_shadowRenderCount++] = i;
        }

        // Ensure dummy slot is safe
        _agentColors[dummySlot] = Vector4.zero;
        _agentMatrices[dummySlot] = Matrix4x4.zero;
        _agentAnimData[dummySlot] = Vector4.zero;
        _agentAnimExtraData[dummySlot] = Vector4.zero;

        int uploadCount = visibleCount + 1; // visible data + dummy
        if (uploadCount > 1)
        {
            _agentMatrixBuffer.SetData(_agentMatrices, 0, 0, uploadCount);
            _agentAnimBuffer.SetData(_agentAnimData, 0, 0, uploadCount);
            _agentAnimExtraBuffer.SetData(_agentAnimExtraData, 0, 0, uploadCount);
            _agentColorBuffer.SetData(_agentColors, 0, 0, uploadCount);
        }

        if (agentCount > 0)
        {
            _agentRemapBuffer.SetData(_agentRemap, 0, 0, agentCount);
            _shadowDataBuffer.SetData(_shadowData, 0, 0, agentCount);
            _shadowColorBuffer.SetData(_shadowColors, 0, 0, agentCount);
        }
        int shadowCount = Mathf.Max(1, _shadowRenderCount);
        _shadowRenderIndexBuffer.SetData(_shadowRenderIndices, 0, 0, shadowCount);
        UpdateShadowArgsBuffer(_shadowRenderCount);
    }

    private void EnsureAgentBuffers()
    {
        int capacity = Mathf.Max(1, maxCharacterCount);
        if (_agentMatrixBuffer != null &&
            _shadowDataBuffer != null &&
            _shadowColorBuffer != null &&
            _shadowRenderIndexBuffer != null &&
            _shadowArgsBuffer != null &&
            _agentMatrices != null &&
            _agentMatrices.Length == capacity + 1)
            return;

        ReleaseAgentBuffers();

        int bufSize = capacity + 1; // +1 for dummy slot
        _agentMatrices = new Matrix4x4[bufSize];
        _agentAnimData = new Vector4[bufSize];
        _agentAnimExtraData = new Vector4[bufSize];
        _agentColors = new Vector4[bufSize];
        _shadowData = new Vector4[capacity];
        _shadowColors = new Vector4[capacity];
        _shadowRenderIndices = new int[capacity];
        _agentRemap = new int[capacity];
        _visibleAgentIndices = new int[capacity];
        _agentMatrixBuffer = new ComputeBuffer(bufSize, sizeof(float) * 16);
        _agentAnimBuffer = new ComputeBuffer(bufSize, sizeof(float) * 4);
        _agentAnimExtraBuffer = new ComputeBuffer(bufSize, sizeof(float) * 4);
        _agentColorBuffer = new ComputeBuffer(bufSize, sizeof(float) * 4);
        _agentRemapBuffer = new ComputeBuffer(capacity, sizeof(int));
        _shadowDataBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        _shadowColorBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        _shadowRenderIndexBuffer = new ComputeBuffer(capacity, sizeof(int));
        _shadowArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        UpdateShadowArgsBuffer(0);
        if (_culledFlags.Length != capacity)
            _culledFlags = new bool[capacity];

        // Init dummy slot at capacity
        int dummySlot = capacity;
        _agentMatrices[dummySlot] = Matrix4x4.zero;
        _agentAnimData[dummySlot] = Vector4.zero;
        _agentAnimExtraData[dummySlot] = Vector4.zero;
        _agentColors[dummySlot] = Vector4.zero;
        _agentMatrixBuffer.SetData(new Matrix4x4[] { Matrix4x4.zero }, 0, dummySlot, 1);
        _agentAnimBuffer.SetData(new Vector4[] { Vector4.zero }, 0, dummySlot, 1);
        _agentAnimExtraBuffer.SetData(new Vector4[] { Vector4.zero }, 0, dummySlot, 1);
        _agentColorBuffer.SetData(new Vector4[] { Vector4.zero }, 0, dummySlot, 1);
    }

    private void CacheExportData(GpuRoleExportData exportData)
    {
        if (exportData == null || exportData.spriteUVs == null)
            return;

        if (!_cachedExportDataSet.Add(exportData))
            return; // 已缓存

        for (int i = 0; i < exportData.spriteUVs.Count; i++)
        {
            SpriteUVData uv = exportData.spriteUVs[i];
            if (!_uvBySpriteId.ContainsKey(uv.spriteId))
            {
                _uvBySpriteId.Add(uv.spriteId, uv);
                _spriteMatrixCache[uv.spriteId] = CreateSpriteMatrix(uv);
            }
        }
    }

    private GpuRoleExportData FindExportDataForBatch(int animIndex)
    {
        for (int i = 0; i < _agents.Count; i++)
        {
            GpuRoleAgent agent = _agents[i];
            if (agent == null || agent.exportData == null || agent.exportData.animations == null)
                continue;

            if (animIndex >= 0 && animIndex < agent.exportData.animations.Count)
                return agent.exportData;
        }

        return null;
    }

    private static bool UsesCombinedAnimTexture(GpuRoleExportData exportData)
    {
        return exportData != null && exportData.combinedAnimDataTex != null;
    }

    private static Texture2D GetRuntimeAnimTexture(GpuRoleExportData exportData, AnimExportData anim)
    {
        if (UsesCombinedAnimTexture(exportData))
            return exportData.combinedAnimDataTex;

        return anim != null ? anim.animDataTex : null;
    }

    private static int GetRuntimeAnimTexWidth(GpuRoleExportData exportData, AnimExportData anim)
    {
        if (UsesCombinedAnimTexture(exportData))
            return exportData.combinedAnimDataTexWidth;

        return anim != null ? anim.animDataTexWidth : 0;
    }

    private static int GetRuntimeAnimTexHeight(GpuRoleExportData exportData, AnimExportData anim)
    {
        if (UsesCombinedAnimTexture(exportData))
            return exportData.combinedAnimDataTexHeight;

        return anim != null ? anim.animDataTexHeight : 0;
    }

    private void ReassignRuntimeIndices()
    {
        for (int i = 0; i < _agents.Count; i++)
            _agents[i].runtimeIndex = i;
    }

    private int AcquireSortingOrder()
    {
        return Gpu2DDepthUtility.AcquireSortingOrder();
    }

    private void ReleaseSortingOrder(int sortingOrder)
    {
        Gpu2DDepthUtility.ReleaseSortingOrder(sortingOrder);
    }

    private int GetBatchOrder(GpuRoleExportData exportData, int slotIndex)
    {
        return slotIndex;
    }

    private void CollectBatchSprites(GpuRoleExportData exportData, int slotIndex, int spriteId, List<SpriteUVData> results)
    {
        if (results == null)
            return;

        if (spriteId >= 0 && _uvBySpriteId.TryGetValue(spriteId, out SpriteUVData currentUv))
            AddUniqueAtlasSprite(results, currentUv);

        if (exportData == null || exportData.slots == null || slotIndex < 0 || slotIndex >= exportData.slots.Count)
            return;

        SlotExportData slot = exportData.slots[slotIndex];
        if (slot == null)
            return;

        if (slot.defaultSpriteId >= 0 && _uvBySpriteId.TryGetValue(slot.defaultSpriteId, out SpriteUVData defaultUv))
            AddUniqueAtlasSprite(results, defaultUv);

        if (slot.availableSpriteIds != null)
        {
            for (int i = 0; i < slot.availableSpriteIds.Length; i++)
            {
                int availableSpriteId = slot.availableSpriteIds[i];
                if (availableSpriteId >= 0 && _uvBySpriteId.TryGetValue(availableSpriteId, out SpriteUVData availableUv))
                    AddUniqueAtlasSprite(results, availableUv);
            }
        }

        // slot 自身没有 sprite，但可能被 group variant 引用（如眼闭上 slot）
        if (results.Count == 0 && exportData.groups != null)
        {
            for (int g = 0; g < exportData.groups.Count; g++)
            {
                var group = exportData.groups[g];
                if (group == null || group.variants == null || group.slotIndices == null)
                    continue;

                int pos = System.Array.IndexOf(group.slotIndices, slotIndex);
                if (pos < 0)
                    continue;

                for (int v = 0; v < group.variants.Count; v++)
                {
                    var variantSpriteIds = group.variants[v].spriteIds;
                    if (variantSpriteIds == null || pos >= variantSpriteIds.Length)
                        continue;

                    int vid = variantSpriteIds[pos];
                    if (vid >= 0 && _uvBySpriteId.TryGetValue(vid, out SpriteUVData vu))
                        AddUniqueAtlasSprite(results, vu);
                }
                break;
            }
        }
    }

    private static void AddUniqueAtlasSprite(List<SpriteUVData> results, SpriteUVData uv)
    {
        if (uv == null)
            return;

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i] != null && results[i].atlasIndex == uv.atlasIndex)
                return;
        }

        results.Add(uv);
    }

    private int[] GetAnimSlotToExportSlotMap(GpuRoleExportData exportData, AnimExportData anim)
    {
        if (exportData == null || anim == null || anim.slotKeys == null)
            return null;

        if (_animSlotToExportSlotCache.TryGetValue(anim, out int[] cached) &&
            cached != null &&
            cached.Length == anim.slotKeys.Count)
            return cached;

        Dictionary<string, int> slotIndexByKey = GetSlotIndexCache(exportData);
        int[] map = new int[anim.slotKeys.Count];
        for (int i = 0; i < map.Length; i++)
        {
            BakedSlotData slot = anim.slotKeys[i];
            if (slot != null && !string.IsNullOrEmpty(slot.slotKey) && slotIndexByKey.TryGetValue(slot.slotKey, out int slotIndex))
                map[i] = slotIndex;
            else
                map[i] = -1;
        }

        _animSlotToExportSlotCache[anim] = map;
        return map;
    }

    private Dictionary<string, int> GetSlotIndexCache(GpuRoleExportData exportData)
    {
        if (_slotIndexCache.TryGetValue(exportData, out Dictionary<string, int> cached) && cached != null)
            return cached;

        Dictionary<string, int> map = new Dictionary<string, int>();
        if (exportData != null && exportData.slots != null)
        {
            for (int i = 0; i < exportData.slots.Count; i++)
            {
                SlotExportData slot = exportData.slots[i];
                if (slot != null && !string.IsNullOrEmpty(slot.slotKey))
                    map[slot.slotKey] = i;
            }
        }

        _slotIndexCache[exportData] = map;
        return map;
    }

    private void UpdateBatchMaterialProperties()
    {
        int writeDepthValue = (writeDepth || useYDepthSorting) ? 1 : 0;
        if (_lastAppliedWriteDepth != writeDepthValue)
        {
            _lastAppliedWriteDepth = writeDepthValue;
            _material.SetInt("_ZWrite", writeDepthValue);
        }

        if (Mathf.Approximately(_lastAppliedAlphaClipThreshold, alphaClipThreshold))
            return;

        _lastAppliedAlphaClipThreshold = alphaClipThreshold;
        for (int i = 0; i < _batches.Count; i++)
            _batches[i].mpb.SetFloat("_AlphaClipThreshold", alphaClipThreshold);

        if (_shadowMaterial != null)
            _shadowMaterial.SetFloat("_AlphaCutoff", alphaClipThreshold);
    }

    private void DrawShadows()
    {
        if (!drawShadow || _shadowMaterial == null || _shadowRenderCount <= 0)
            return;

        if (_shadowMpb == null)
            _shadowMpb = new MaterialPropertyBlock();

        _shadowMpb.Clear();
        _shadowMpb.SetBuffer("_AgentMatrices", _agentMatrixBuffer);
        _shadowMpb.SetBuffer("_AgentAnimExtraData", _agentAnimExtraBuffer);
        _shadowMpb.SetBuffer("_AgentRemap", _agentRemapBuffer);
        _shadowMpb.SetBuffer("_ShadowData", _shadowDataBuffer);
        _shadowMpb.SetBuffer("_ShadowColors", _shadowColorBuffer);
        _shadowMpb.SetBuffer("_ShadowRenderIndices", _shadowRenderIndexBuffer);

        Graphics.DrawMeshInstancedIndirect(
            _quadMesh,
            0,
            _shadowMaterial,
            _drawBounds,
            _shadowArgsBuffer,
            0,
            _shadowMpb,
            ShadowCastingMode.Off,
            false,
            gameObject.layer,
            null
        );
    }

    private void UpdateShadowArgsBuffer(int count)
    {
        if (_shadowArgsBuffer == null || _quadMesh == null)
            return;

        uint[] args = new uint[5];
        args[0] = _quadMesh.GetIndexCount(0);
        args[1] = (uint)Mathf.Max(0, count);
        args[2] = _quadMesh.GetIndexStart(0);
        args[3] = _quadMesh.GetBaseVertex(0);
        args[4] = 0;
        _shadowArgsBuffer.SetData(args);
    }

    private void RefreshDrawBounds(bool force)
    {
        if (!force &&
            _lastDrawBoundsCenter == drawBoundsCenter &&
            _lastDrawBoundsSize == drawBoundsSize)
            return;

        _lastDrawBoundsCenter = drawBoundsCenter;
        _lastDrawBoundsSize = drawBoundsSize;
        _drawBounds = new Bounds(drawBoundsCenter, drawBoundsSize);
    }

    private bool TryGetCameraBounds(out Rect bounds)
    {
        Camera cam = clipBoundCamera;
        if (cam == null)
        {
            bounds = default;
            return false;
        }

        if (cam.orthographic)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            Vector3 center = cam.transform.position;
            bounds = new Rect(
                center.x - width * 0.5f - cullPadding,
                center.y - height * 0.5f - cullPadding,
                width + cullPadding * 2f,
                height + cullPadding * 2f
            );
            return true;
        }

        float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, distance));
        float minX = Mathf.Min(bottomLeft.x, topRight.x) - cullPadding;
        float maxX = Mathf.Max(bottomLeft.x, topRight.x) + cullPadding;
        float minY = Mathf.Min(bottomLeft.y, topRight.y) - cullPadding;
        float maxY = Mathf.Max(bottomLeft.y, topRight.y) + cullPadding;
        bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }

    private bool IsAgentVisible(Vector3 position, Vector3 scale, Rect cameraBounds)
    {
        float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        Vector2 extents = _cachedCullExtents * maxScale;
        return position.x >= cameraBounds.xMin - extents.x
            && position.x <= cameraBounds.xMax + extents.x
            && position.y >= cameraBounds.yMin - extents.y
            && position.y <= cameraBounds.yMax + extents.y;
    }

    private float CalculateDepthZ(Vector3 position, GpuRoleAgent agent, int agentIndex)
    {
        int order = agent != null ? agent.sortingOrder : -1;
        return Gpu2DDepthUtility.CalculateDepthZ(position, order, agentIndex, depthBaseZ, preserveAgentZ);
    }

    private float CalculateSortingDepthBias(GpuRoleAgent agent, int agentIndex)
    {
        int order = agent != null ? agent.sortingOrder : -1;
        return Gpu2DDepthUtility.CalculateSortingDepthBias(order, agentIndex);
    }

    private Matrix4x4 CreateSpriteMatrix(SpriteUVData uv)
    {
        float worldW = uv.cropW / pixelsPerUnit;
        float worldH = uv.cropH / pixelsPerUnit;
        Vector3 pivotOffset = new Vector3(-worldW * uv.pivotX, -worldH * uv.pivotY, 0f);
        return Matrix4x4.TRS(pivotOffset, Quaternion.identity, new Vector3(worldW, worldH, 1f));
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "GpuRoleGpuRuntimeQuad";
        mesh.vertices = new[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(1f, 1f, 0f)
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };
        mesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
        mesh.RecalculateBounds();
        return mesh;
    }

    private void ReleaseBatches()
    {
        for (int i = 0; i < _batches.Count; i++)
        {
            AtlasBatch batch = _batches[i];
            if (batch.instanceDataBuffer != null) batch.instanceDataBuffer.Release();
            if (batch.uvBuffer != null) batch.uvBuffer.Release();
            if (batch.spriteMatrixBuffer != null) batch.spriteMatrixBuffer.Release();
            if (batch.argsBuffer != null) batch.argsBuffer.Release();
        }

        _batchMap.Clear();
        _batches.Clear();
        _slotLocationMap.Clear();
    }

    private void ReleaseAgentBuffers()
    {
        if (_agentMatrixBuffer != null) _agentMatrixBuffer.Release();
        if (_agentAnimBuffer != null) _agentAnimBuffer.Release();
        if (_agentAnimExtraBuffer != null) _agentAnimExtraBuffer.Release();
        if (_agentColorBuffer != null) _agentColorBuffer.Release();
        if (_agentRemapBuffer != null) _agentRemapBuffer.Release();
        if (_shadowDataBuffer != null) _shadowDataBuffer.Release();
        if (_shadowColorBuffer != null) _shadowColorBuffer.Release();
        if (_shadowRenderIndexBuffer != null) _shadowRenderIndexBuffer.Release();
        if (_shadowArgsBuffer != null) _shadowArgsBuffer.Release();
        _agentMatrixBuffer = null;
        _agentAnimBuffer = null;
        _agentAnimExtraBuffer = null;
        _agentColorBuffer = null;
        _agentRemapBuffer = null;
        _shadowDataBuffer = null;
        _shadowColorBuffer = null;
        _shadowRenderIndexBuffer = null;
        _shadowArgsBuffer = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showBounds) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(drawBoundsCenter, drawBoundsSize);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _agents.Count; i++)
        {
            GpuRoleAgent agent = _agents[i];
            if (agent == null)
                continue;

            ReleaseSortingOrder(agent.sortingOrder);
            agent.sortingOrder = -1;
            if (agent.manager == this)
                agent.manager = null;
        }

        ReleaseBatches();
        ReleaseAgentBuffers();

        if (_quadMesh != null)
            DestroyImmediate(_quadMesh);
        if (_material != null)
            DestroyImmediate(_material);
        if (_shadowMaterial != null)
            DestroyImmediate(_shadowMaterial);
    }
}
