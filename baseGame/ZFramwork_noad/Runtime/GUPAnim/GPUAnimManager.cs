using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUAnimManager : MonoBehaviour
{
    [HideInInspector] public Mesh mesh;
    [HideInInspector] public Material material;
    [HideInInspector] public Material shadowMaterial;
    public AnimAtlasData data;
    public Camera clipBoundCamera;
    public int initialCapacity = 1024;
    public float fps = 10f;
    public float defaultScale = 1f;
    public float depthOffset;
    public bool drawShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    public bool cullByCamera = true;
    public float cullPadding = 2f;

    [Header("Bounds")]
    public bool showBounds = true;
    public Vector3 drawBoundsCenter = Vector3.zero;
    public Vector3 drawBoundsSize = new Vector3(10000f, 10000f, 10000f);

    [HideInInspector] public bool useYAsDepth = true;
    [HideInInspector] public float yToDepthScale = Gpu2DDepthUtility.YToZScale;

    private Matrix4x4[] matrices;
    private Vector3[] positions;
    private float[] renderScales;
    private bool[] roleAlive;
    private bool[] roleVisible;
    private bool[] flipXs;
    private int[] characterIndices;
    private int[] clipIndices;
    private int[] sortingOrders;
    private float[] jumpHeights;
    private int[] lastFrameIndices;
    private float[] animTimes;
    private float[] animSpeeds;
    private bool[] roleAnimated;
    private bool[] roleSyncTransform;
    private bool[] matrixDirty;
    private bool[] frameDirty;
    private GPUAgent[] roleAgents;

    private Vector4[] frameUVs;
    private Vector4[] frameOffsetFrames;
    private Vector4[] frameBaseCenters;
    private Vector4[] frameShadowData;
    private Vector4[] depthBiases;
    private int[] renderIndices;
    private int[] shadowRenderIndices;
    private int[] freeIds;

    private ComputeBuffer matrixBuffer;
    private ComputeBuffer frameUVBuffer;
    private ComputeBuffer frameOffsetBuffer;
    private ComputeBuffer frameBaseCenterBuffer;
    private ComputeBuffer frameShadowBuffer;
    private ComputeBuffer depthBiasBuffer;
    private ComputeBuffer renderIndexBuffer;
    private ComputeBuffer shadowRenderIndexBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer shadowArgsBuffer;

    private MaterialPropertyBlock mpb;
    private Bounds drawBounds;
    private Vector3 lastDrawBoundsCenter;
    private Vector3 lastDrawBoundsSize;

    private readonly List<int> dirtyMatrixIds = new List<int>();
    private readonly List<int> dirtyFrameIds = new List<int>();
    private readonly List<int> animatedRoleIds = new List<int>();
    private readonly List<int> syncTransformRoleIds = new List<int>();

    private AnimAtlasData cachedCullData;
    private Vector2 cachedCullExtents = Vector2.one;
    private bool cachedCullDrawShadow;
    private bool allGpuDataDirty = true;
    private bool renderListDirty = true;
    private int activeCount;
    private int renderCount;
    private int shadowRenderCount;
    private int roleCount;
    private int freeCount;
    private int bufferCapacity;

    public int RoleCount => roleCount;

    private void Start()
    {
        EnsureRuntime();
        ApplyManagedSettings();
    }
    private void Update()
    {
        if (!IsReady())
            return;

        SyncTransformAgents();
        UpdateAnimData(Time.deltaTime);

        if (cullByCamera || renderListDirty)
            BuildRenderIndices();

        UploadDirtyBuffers();
        Draw();
    }

    private void OnValidate()
    {
        ApplyManagedSettings();
        RefreshDrawBounds(true);
        renderListDirty = true;
    }

    private void ApplyManagedSettings()
    {
        useYAsDepth = true;
        yToDepthScale = Gpu2DDepthUtility.YToZScale;
    }

    public int CreateRole(Vector3 position, string clipName = null)
    {
        return CreateRole(position, defaultScale, clipName);
    }

    public int CreateRole(Vector3 position, float scale, string clipName = null)
    {
        return CreateRole(null, position, scale, clipName);
    }

    public int CreateRole(string characterName, Vector3 position, string clipName = null)
    {
        return CreateRole(characterName, position, defaultScale, clipName);
    }

    public int CreateRole(string characterName, Vector3 position, float scale, string clipName = null)
    {
        EnsureRuntime();
        if (data == null || data.clips == null || data.clips.Count == 0)
            return -1;

        data.EnsureRuntimeCache();
        EnsureCapacity(activeCount + 1);

        int characterIndex = ResolveCharacterIndex(characterName);
        int clipIndex = 0;
        if (data.HasCharacters())
        {
            if (!data.TryGetCharacterClipIndex(characterIndex, clipName, out clipIndex))
                return -1;
        }
        else if (!string.IsNullOrEmpty(clipName))
        {
            data.TryGetClipIndex(clipName, out clipIndex);
        }

        int id = AllocRoleId();
        positions[id] = position;
        renderScales[id] = scale;
        roleAlive[id] = true;
        roleVisible[id] = true;
        flipXs[id] = false;
        characterIndices[id] = characterIndex;
        clipIndices[id] = Mathf.Clamp(clipIndex, 0, data.clips.Count - 1);
        sortingOrders[id] = Gpu2DDepthUtility.AcquireSortingOrder();
        jumpHeights[id] = 0f;
        lastFrameIndices[id] = -1;
        animTimes[id] = 0f;
        animSpeeds[id] = 1f;

        UpdateMatrix(id);
        UpdateFrameData(id, true);
        UpdateAnimationMembership(id);
        renderListDirty = true;
        return id;
    }

    public void RemoveRole(int id)
    {
        if (!IsValidRole(id))
            return;

        roleAlive[id] = false;
        roleVisible[id] = false;
        Gpu2DDepthUtility.ReleaseSortingOrder(sortingOrders[id]);
        sortingOrders[id] = -1;
        SetAnimationMembership(id, false);
        SetTransformSync(id, null, false);
        freeIds[freeCount] = id;
        freeCount++;
        roleCount--;
        renderListDirty = true;
    }

    public void SetPosition(int id, Vector3 position)
    {
        if (!IsValidRole(id))
            return;

        positions[id] = position;
        UpdateMatrix(id);
        renderListDirty = true;
    }

    public void SetScale(int id, float scale)
    {
        if (!IsValidRole(id))
            return;

        renderScales[id] = scale;
        UpdateMatrix(id);
        renderListDirty = true;
    }

    public void SetVisible(int id, bool visible)
    {
        if (!IsValidRole(id) || roleVisible[id] == visible)
            return;

        roleVisible[id] = visible;
        renderListDirty = true;
    }

    public void SetFlipX(int id, bool flipX)
    {
        if (!IsValidRole(id))
            return;

        flipXs[id] = flipX;
        UpdateMatrix(id);
    }

    public void SetClip(int id, string clipName, bool resetTime = true)
    {
        if (!IsValidRole(id) || data == null)
            return;

        bool found = data.HasCharacters()
            ? data.TryGetCharacterClipIndex(characterIndices[id], clipName, out int clipIndex)
            : data.TryGetClipIndex(clipName, out clipIndex);

        if (!found)
            return;

        clipIndices[id] = clipIndex;
        if (resetTime)
            animTimes[id] = 0f;

        lastFrameIndices[id] = -1;
        UpdateFrameData(id, true);
        UpdateAnimationMembership(id);
    }

    public void SetCharacter(int id, string characterName, string clipName = null, bool resetTime = true)
    {
        if (!IsValidRole(id) || data == null || !data.HasCharacters())
            return;

        int characterIndex = ResolveCharacterIndex(characterName);
        if (characterIndex < 0)
            return;

        if (!data.TryGetCharacterClipIndex(characterIndex, clipName, out int clipIndex))
            return;

        characterIndices[id] = characterIndex;
        clipIndices[id] = clipIndex;
        if (resetTime)
            animTimes[id] = 0f;

        lastFrameIndices[id] = -1;
        UpdateMatrix(id);
        UpdateFrameData(id, true);
        UpdateAnimationMembership(id);
        renderListDirty = true;
    }

    public void SetSpeed(int id, float speed)
    {
        if (!IsValidRole(id))
            return;

        animSpeeds[id] = speed;
        UpdateAnimationMembership(id);
    }

    public void SetAnimTime(int id, float time)
    {
        if (!IsValidRole(id))
            return;

        animTimes[id] = Mathf.Max(0f, time);
        UpdateFrameData(id, false);
    }

    public void SetJumpHeight(int id, float height)
    {
        if (!IsValidRole(id))
            return;

        jumpHeights[id] = height;
        UpdateMatrix(id);
    }

    public void ClearRoles()
    {
        ReleaseAllSortingOrders();
        activeCount = 0;
        roleCount = 0;
        freeCount = 0;
        renderCount = 0;
        shadowRenderCount = 0;
        animatedRoleIds.Clear();
        syncTransformRoleIds.Clear();
        dirtyMatrixIds.Clear();
        dirtyFrameIds.Clear();
        allGpuDataDirty = true;
        renderListDirty = true;
    }

    public void SetTransformSync(int id, GPUAgent agent, bool enabled)
    {
        if (!IsValidRole(id))
            return;

        roleAgents[id] = enabled ? agent : null;
        SetTransformSyncMembership(id, enabled && agent != null);
    }

    [ContextMenu("Log Shadow Offset")]
    private void LogShadowOffset()
    {
        Vector2 dataOffset = data != null ? data.shadowOffset : Vector2.zero;
        Vector2 dataSize = data != null ? data.shadowSize : Vector2.zero;
        Debug.Log(
            $"GPUAnim shadow offset={dataOffset}, size={dataSize}. Shadow color/alpha is controlled by GPUAnimManager.",
            this
        );
    }

    private void UpdateAnimData(float deltaTime)
    {
        for (int i = animatedRoleIds.Count - 1; i >= 0; i--)
        {
            int id = animatedRoleIds[i];
            if (!IsValidRole(id))
            {
                animatedRoleIds.RemoveAt(i);
                continue;
            }

            int clipIndex = clipIndices[id];
            if (data.clipFrameCounts == null || clipIndex < 0 || clipIndex >= data.clipFrameCounts.Length)
                continue;

            int frameCount = data.clipFrameCounts[clipIndex];
            if (frameCount <= 1)
                continue;

            float clipFps = data.clipFpsValues[clipIndex] > 0f ? data.clipFpsValues[clipIndex] : fps;
            animTimes[id] += deltaTime * animSpeeds[id];

            if (!data.clipLoops[clipIndex])
            {
                float maxTime = Mathf.Max(0f, (frameCount - 1) / clipFps);
                animTimes[id] = Mathf.Min(animTimes[id], maxTime);
            }

            UpdateFrameData(id, false);
        }
    }

    private void SyncTransformAgents()
    {
        for (int i = syncTransformRoleIds.Count - 1; i >= 0; i--)
        {
            int id = syncTransformRoleIds[i];
            if (!IsValidRole(id))
            {
                syncTransformRoleIds.RemoveAt(i);
                continue;
            }

            GPUAgent agent = roleAgents[id];
            if (agent == null)
            {
                SetTransformSyncMembership(id, false);
                continue;
            }

            Transform agentTransform = agent.transform;
            if (!agentTransform.hasChanged)
                continue;

            positions[id] = agent.RenderPosition;
            UpdateMatrix(id);
            renderListDirty = true;
            agentTransform.hasChanged = false;
        }
    }

    private void Draw()
    {
        if (renderCount <= 0)
            return;
        RefreshDrawBounds(false);
        BindBuffers(material, true);

        Graphics.DrawMeshInstancedIndirect(
            mesh,
            0,
            material,
            drawBounds,
            argsBuffer,
            0,
            mpb,
            ShadowCastingMode.Off,
            false,
            gameObject.layer,
            null
        );

        if (!drawShadow || shadowMaterial == null || shadowRenderCount <= 0)
            return;

        BindBuffers(shadowMaterial, false);
        Graphics.DrawMeshInstancedIndirect(
            mesh,
            0,
            shadowMaterial,
            drawBounds,
            shadowArgsBuffer,
            0,
            mpb,
            ShadowCastingMode.Off,
            false,
            gameObject.layer,
            null
        );
    }

    private void BindBuffers(Material targetMaterial, bool bodyPass)
    {
        if (targetMaterial == null)
            return;

        mpb.Clear();
        mpb.SetBuffer("_Matrices", matrixBuffer);
        mpb.SetBuffer("_RenderIndices", bodyPass ? renderIndexBuffer : shadowRenderIndexBuffer);
        mpb.SetBuffer("_FrameBaseCenterBuffer", frameBaseCenterBuffer);
        mpb.SetBuffer("_DepthBiasBuffer", depthBiasBuffer);

        if (bodyPass)
        {
            mpb.SetTexture("_MainTex", data.atlas);
            mpb.SetBuffer("_FrameUVBuffer", frameUVBuffer);
            mpb.SetBuffer("_FrameOffsetBuffer", frameOffsetBuffer);
        }
        else
        {
            mpb.SetBuffer("_FrameShadowBuffer", frameShadowBuffer);
            mpb.SetFloat("_ShadowEnabled", drawShadow ? 1f : 0f);
            mpb.SetColor("_ShadowColor", shadowColor);
        }
    }

    private void BuildRenderIndices()
    {
        renderCount = 0;
        shadowRenderCount = 0;
        Rect cameraBounds = default;
        bool canCull = cullByCamera && TryGetCameraBounds(out cameraBounds);
        if (canCull)
            EnsureCullExtents();

        for (int i = 0; i < activeCount; i++)
        {
            if (roleAlive[i] && roleVisible[i] && (!canCull || IsRoleVisible(i, cameraBounds)))
            {
                renderIndices[renderCount] = i;
                renderCount++;
                if (drawShadow && RoleHasShadow(i))
                {
                    shadowRenderIndices[shadowRenderCount] = i;
                    shadowRenderCount++;
                }
            }
        }

        renderIndexBuffer.SetData(renderIndices, 0, 0, Mathf.Max(1, renderCount));
        shadowRenderIndexBuffer.SetData(shadowRenderIndices, 0, 0, Mathf.Max(1, shadowRenderCount));
        UpdateArgsBuffer(renderCount);
        UpdateShadowArgsBuffer(shadowRenderCount);
        renderListDirty = false;
    }

    private bool TryGetCameraBounds(out Rect bounds)
    {
        if (clipBoundCamera == null)
        {
            bounds = default;
            return false;
        }

        if (clipBoundCamera.orthographic)
        {
            float height = clipBoundCamera.orthographicSize * 2f;
            float width = height * clipBoundCamera.aspect;
            Vector3 center = clipBoundCamera.transform.position;
            bounds = new Rect(
                center.x - width * 0.5f - cullPadding,
                center.y - height * 0.5f - cullPadding,
                width + cullPadding * 2f,
                height + cullPadding * 2f
            );
            return true;
        }

        float distance = Mathf.Abs(clipBoundCamera.transform.position.z - transform.position.z);
        Vector3 bottomLeft = clipBoundCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = clipBoundCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distance));
        float minX = Mathf.Min(bottomLeft.x, topRight.x) - cullPadding;
        float maxX = Mathf.Max(bottomLeft.x, topRight.x) + cullPadding;
        float minY = Mathf.Min(bottomLeft.y, topRight.y) - cullPadding;
        float maxY = Mathf.Max(bottomLeft.y, topRight.y) + cullPadding;
        bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }

    private bool IsRoleVisible(int id, Rect cameraBounds)
    {
        Vector3 position = positions[id];
        float scale = Mathf.Abs(GetFinalScale(id));
        float paddingX = cachedCullExtents.x * scale;
        float paddingY = cachedCullExtents.y * scale;
        return position.x >= cameraBounds.xMin - paddingX
            && position.x <= cameraBounds.xMax + paddingX
            && position.y >= cameraBounds.yMin - paddingY
            && position.y <= cameraBounds.yMax + paddingY;
    }

    private void EnsureCullExtents()
    {
        if (cachedCullData == data && cachedCullDrawShadow == drawShadow)
            return;

        cachedCullData = data;
        cachedCullDrawShadow = drawShadow;
        cachedCullExtents = CalculateCullExtents();
    }

    private Vector2 CalculateCullExtents()
    {
        if (data == null || data.frames == null || data.frames.Count == 0)
            return Vector2.one;

        float maxX = 0.5f;
        float maxY = 0.5f;

        if (data.HasCharacters())
        {
            for (int characterIndex = 0; characterIndex < data.characters.Count; characterIndex++)
            {
                AnimCharacter character = data.characters[characterIndex];
                if (character == null || character.clips == null)
                    continue;

                for (int clipIndex = 0; clipIndex < character.clips.Count; clipIndex++)
                {
                    AnimClip clip = character.clips[clipIndex];
                    if (clip == null)
                        continue;

                    for (int frameIndex = 0; frameIndex < clip.frameCount; frameIndex++)
                    {
                        AccumulateCullExtents(
                            data.frames[clip.startFrame + frameIndex],
                            character.centerOffset,
                            character.shadowOffset,
                            character.shadowSize,
                            ref maxX,
                            ref maxY
                        );
                    }
                }
            }

            return new Vector2(maxX, maxY);
        }

        for (int i = 0; i < data.frames.Count; i++)
            AccumulateCullExtents(data.frames[i], data.centerOffset, data.shadowOffset, data.shadowSize, ref maxX, ref maxY);

        return new Vector2(maxX, maxY);
    }

    private void AccumulateCullExtents(
        FrameRuntimeData frame,
        Vector2 centerOffset,
        Vector2 shadowOffset,
        Vector2 shadowSize,
        ref float maxX,
        ref float maxY)
    {
        Vector2 baseSize = frame.baseSize;
        float baseHeight = Mathf.Max(baseSize.y, 1f);
        Vector2 anchorPixel = new Vector2(baseSize.x * 0.5f + centerOffset.x, centerOffset.y);

        maxX = Mathf.Max(maxX, Mathf.Abs(0f - anchorPixel.x) / baseHeight);
        maxX = Mathf.Max(maxX, Mathf.Abs(baseSize.x - anchorPixel.x) / baseHeight);
        maxY = Mathf.Max(maxY, Mathf.Abs(0f - anchorPixel.y) / baseHeight);
        maxY = Mathf.Max(maxY, Mathf.Abs(baseSize.y - anchorPixel.y) / baseHeight);

        if (!drawShadow)
            return;

        Vector2 shadowCenter = anchorPixel + shadowOffset;
        Vector2 safeShadowSize = new Vector2(Mathf.Max(1f, shadowSize.x), Mathf.Max(1f, shadowSize.y));
        maxX = Mathf.Max(maxX, Mathf.Abs(shadowCenter.x - safeShadowSize.x - anchorPixel.x) / baseHeight);
        maxX = Mathf.Max(maxX, Mathf.Abs(shadowCenter.x + safeShadowSize.x - anchorPixel.x) / baseHeight);
        maxY = Mathf.Max(maxY, Mathf.Abs(shadowCenter.y - safeShadowSize.y - anchorPixel.y) / baseHeight);
        maxY = Mathf.Max(maxY, Mathf.Abs(shadowCenter.y + safeShadowSize.y - anchorPixel.y) / baseHeight);
    }

    private void UpdateMatrix(int id)
    {
        float finalScale = GetFinalScale(id);
        float xScale = flipXs[id] ? -finalScale : finalScale;
        Vector3 renderPosition = positions[id];
        if (useYAsDepth)
            renderPosition.z = Gpu2DDepthUtility.CalculateDepthZ(positions[id], sortingOrders[id], id, depthOffset, false);

        matrices[id] = Matrix4x4.TRS(renderPosition, Quaternion.identity, new Vector3(xScale, finalScale, finalScale));
        depthBiases[id] = new Vector4(
            useYAsDepth ? Gpu2DDepthUtility.CalculateSortingDepthBias(sortingOrders[id], id) : 0f,
            jumpHeights[id],
            0f,
            0f
        );
        MarkMatrixDirty(id);
    }

    private void UpdateFrameData(int id, bool force)
    {
        int runtimeFrameIndex = ResolveRuntimeFrameIndex(id);
        if (runtimeFrameIndex < 0 || (!force && runtimeFrameIndex == lastFrameIndices[id]))
            return;

        lastFrameIndices[id] = runtimeFrameIndex;
        frameUVs[id] = data.frameUVs[runtimeFrameIndex];
        frameOffsetFrames[id] = data.frameOffsetFrames[runtimeFrameIndex];
        Vector4 baseSize = data.frameBaseSizes[runtimeFrameIndex];

        AnimCharacter character = data.GetCharacter(characterIndices[id]);
        Vector2 centerOffset = character != null ? character.centerOffset : data.centerOffset;
        bool hasShadow = RoleHasShadow(id);
        Vector2 shadowOffset = hasShadow ? (character != null ? character.shadowOffset : data.shadowOffset) : Vector2.zero;
        Vector2 shadowSize = hasShadow ? (character != null ? character.shadowSize : data.shadowSize) : Vector2.zero;

        frameBaseCenters[id] = new Vector4(baseSize.x, baseSize.y, centerOffset.x, centerOffset.y);
        frameShadowData[id] = new Vector4(shadowOffset.x, shadowOffset.y, shadowSize.x, shadowSize.y);
        MarkFrameDirty(id);
    }

    private int ResolveRuntimeFrameIndex(int id)
    {
        int clipIndex = clipIndices[id];
        if (data == null || data.clipFrameCounts == null || clipIndex < 0 || clipIndex >= data.clipFrameCounts.Length)
            return -1;

        int frameCount = data.clipFrameCounts[clipIndex];
        if (frameCount <= 0)
            return -1;

        float clipFps = data.clipFpsValues[clipIndex] > 0f ? data.clipFpsValues[clipIndex] : fps;
        int frameIndex = Mathf.FloorToInt(animTimes[id] * clipFps);
        return data.GetFrameIndex(clipIndex, frameIndex);
    }

    private void EnsureRuntime()
    {
        if (mesh == null)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mesh = quad.GetComponent<MeshFilter>().sharedMesh;
            Destroy(quad);
        }

        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        if (material == null)
        {
            Shader bodyShader = Shader.Find("Custom/GPUAnim_Instanced_URP");
            if (bodyShader != null)
            {
                material = new Material(bodyShader);
                material.hideFlags = HideFlags.DontSave;
            }
        }

        if (shadowMaterial == null)
        {
            Shader shadowShader = Shader.Find("Custom/GPUAnim_Shadow_URP");
            if (shadowShader != null)
            {
                shadowMaterial = new Material(shadowShader);
                shadowMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        EnsureCapacity(Mathf.Max(1, initialCapacity));
        if (data != null)
            data.EnsureRuntimeCache();

        RefreshDrawBounds(true);
    }

    private int AllocRoleId()
    {
        if (freeCount > 0)
        {
            freeCount--;
            roleCount++;
            return freeIds[freeCount];
        }

        EnsureCapacity(activeCount + 1);
        int id = activeCount;
        activeCount++;
        roleCount++;
        return id;
    }

    private void EnsureCapacity(int capacity)
    {
        if (matrices != null && matrices.Length >= capacity && matrixBuffer != null)
            return;

        int newCapacity = matrices == null ? Mathf.Max(1, capacity) : Mathf.Max(capacity, matrices.Length * 2);

        Matrix4x4[] newMatrices = new Matrix4x4[newCapacity];
        Vector3[] newPositions = new Vector3[newCapacity];
        float[] newRenderScales = new float[newCapacity];
        bool[] newRoleAlive = new bool[newCapacity];
        bool[] newRoleVisible = new bool[newCapacity];
        bool[] newFlipXs = new bool[newCapacity];
        int[] newCharacterIndices = new int[newCapacity];
        int[] newClipIndices = new int[newCapacity];
        int[] newSortingOrders = new int[newCapacity];
        float[] newJumpHeights = new float[newCapacity];
        int[] newLastFrameIndices = new int[newCapacity];
        float[] newAnimTimes = new float[newCapacity];
        float[] newAnimSpeeds = new float[newCapacity];
        bool[] newRoleAnimated = new bool[newCapacity];
        bool[] newRoleSyncTransform = new bool[newCapacity];
        bool[] newMatrixDirty = new bool[newCapacity];
        bool[] newFrameDirty = new bool[newCapacity];
        GPUAgent[] newRoleAgents = new GPUAgent[newCapacity];
        Vector4[] newFrameUVs = new Vector4[newCapacity];
        Vector4[] newFrameOffsetFrames = new Vector4[newCapacity];
        Vector4[] newFrameBaseCenters = new Vector4[newCapacity];
        Vector4[] newFrameShadowData = new Vector4[newCapacity];
        Vector4[] newDepthBiases = new Vector4[newCapacity];
        int[] newRenderIndices = new int[newCapacity];
        int[] newShadowRenderIndices = new int[newCapacity];
        int[] newFreeIds = new int[newCapacity];

        for (int i = 0; i < newLastFrameIndices.Length; i++)
        {
            newLastFrameIndices[i] = -1;
            newSortingOrders[i] = -1;
        }

        if (matrices != null)
        {
            System.Array.Copy(matrices, newMatrices, activeCount);
            System.Array.Copy(positions, newPositions, activeCount);
            System.Array.Copy(renderScales, newRenderScales, activeCount);
            System.Array.Copy(roleAlive, newRoleAlive, activeCount);
            System.Array.Copy(roleVisible, newRoleVisible, activeCount);
            System.Array.Copy(flipXs, newFlipXs, activeCount);
            System.Array.Copy(characterIndices, newCharacterIndices, activeCount);
            System.Array.Copy(clipIndices, newClipIndices, activeCount);
            System.Array.Copy(sortingOrders, newSortingOrders, activeCount);
            System.Array.Copy(jumpHeights, newJumpHeights, activeCount);
            System.Array.Copy(lastFrameIndices, newLastFrameIndices, activeCount);
            System.Array.Copy(animTimes, newAnimTimes, activeCount);
            System.Array.Copy(animSpeeds, newAnimSpeeds, activeCount);
            System.Array.Copy(roleAnimated, newRoleAnimated, activeCount);
            System.Array.Copy(roleSyncTransform, newRoleSyncTransform, activeCount);
            System.Array.Copy(roleAgents, newRoleAgents, activeCount);
            System.Array.Copy(frameUVs, newFrameUVs, activeCount);
            System.Array.Copy(frameOffsetFrames, newFrameOffsetFrames, activeCount);
            System.Array.Copy(frameBaseCenters, newFrameBaseCenters, activeCount);
            System.Array.Copy(frameShadowData, newFrameShadowData, activeCount);
            System.Array.Copy(depthBiases, newDepthBiases, activeCount);
            System.Array.Copy(renderIndices, newRenderIndices, renderCount);
            System.Array.Copy(shadowRenderIndices, newShadowRenderIndices, shadowRenderCount);
            System.Array.Copy(freeIds, newFreeIds, freeCount);
        }

        matrices = newMatrices;
        positions = newPositions;
        renderScales = newRenderScales;
        roleAlive = newRoleAlive;
        roleVisible = newRoleVisible;
        flipXs = newFlipXs;
        characterIndices = newCharacterIndices;
        clipIndices = newClipIndices;
        sortingOrders = newSortingOrders;
        jumpHeights = newJumpHeights;
        lastFrameIndices = newLastFrameIndices;
        animTimes = newAnimTimes;
        animSpeeds = newAnimSpeeds;
        roleAnimated = newRoleAnimated;
        roleSyncTransform = newRoleSyncTransform;
        matrixDirty = newMatrixDirty;
        frameDirty = newFrameDirty;
        roleAgents = newRoleAgents;
        frameUVs = newFrameUVs;
        frameOffsetFrames = newFrameOffsetFrames;
        frameBaseCenters = newFrameBaseCenters;
        frameShadowData = newFrameShadowData;
        depthBiases = newDepthBiases;
        renderIndices = newRenderIndices;
        shadowRenderIndices = newShadowRenderIndices;
        freeIds = newFreeIds;

        RecreateGpuBuffers(newCapacity);
        allGpuDataDirty = true;
        renderListDirty = true;
    }

    private void RecreateGpuBuffers(int capacity)
    {
        ReleaseGpuBuffers();

        bufferCapacity = capacity;
        matrixBuffer = new ComputeBuffer(capacity, sizeof(float) * 16);
        frameUVBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        frameOffsetBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        frameBaseCenterBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        frameShadowBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        depthBiasBuffer = new ComputeBuffer(capacity, sizeof(float) * 4);
        renderIndexBuffer = new ComputeBuffer(capacity, sizeof(int));
        shadowRenderIndexBuffer = new ComputeBuffer(capacity, sizeof(int));
        argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        shadowArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        UpdateArgsBuffer(0);
        UpdateShadowArgsBuffer(0);
    }

    private void UploadDirtyBuffers()
    {
        if (allGpuDataDirty)
        {
            matrixBuffer.SetData(matrices);
            frameUVBuffer.SetData(frameUVs);
            frameOffsetBuffer.SetData(frameOffsetFrames);
            frameBaseCenterBuffer.SetData(frameBaseCenters);
            frameShadowBuffer.SetData(frameShadowData);
            depthBiasBuffer.SetData(depthBiases);
            allGpuDataDirty = false;
            System.Array.Clear(matrixDirty, 0, matrixDirty.Length);
            System.Array.Clear(frameDirty, 0, frameDirty.Length);
            dirtyMatrixIds.Clear();
            dirtyFrameIds.Clear();
            return;
        }

        for (int i = 0; i < dirtyMatrixIds.Count; i++)
        {
            int id = dirtyMatrixIds[i];
            if (id < 0 || id >= bufferCapacity)
                continue;

            matrixBuffer.SetData(matrices, id, id, 1);
            depthBiasBuffer.SetData(depthBiases, id, id, 1);
            matrixDirty[id] = false;
        }

        for (int i = 0; i < dirtyFrameIds.Count; i++)
        {
            int id = dirtyFrameIds[i];
            if (id < 0 || id >= bufferCapacity)
                continue;

            frameUVBuffer.SetData(frameUVs, id, id, 1);
            frameOffsetBuffer.SetData(frameOffsetFrames, id, id, 1);
            frameBaseCenterBuffer.SetData(frameBaseCenters, id, id, 1);
            frameShadowBuffer.SetData(frameShadowData, id, id, 1);
            frameDirty[id] = false;
        }

        dirtyMatrixIds.Clear();
        dirtyFrameIds.Clear();
    }

    private void MarkMatrixDirty(int id)
    {
        if (matrixDirty == null || id < 0 || id >= matrixDirty.Length || matrixDirty[id])
            return;

        matrixDirty[id] = true;
        dirtyMatrixIds.Add(id);
    }

    private void MarkFrameDirty(int id)
    {
        if (frameDirty == null || id < 0 || id >= frameDirty.Length || frameDirty[id])
            return;

        frameDirty[id] = true;
        dirtyFrameIds.Add(id);
    }

    private void UpdateArgsBuffer(int count)
    {
        if (argsBuffer == null || mesh == null)
            return;

        uint[] args = new uint[5];
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)Mathf.Max(0, count);
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        args[4] = 0;
        argsBuffer.SetData(args);
    }

    private void UpdateShadowArgsBuffer(int count)
    {
        if (shadowArgsBuffer == null || mesh == null)
            return;

        uint[] args = new uint[5];
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)Mathf.Max(0, count);
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        args[4] = 0;
        shadowArgsBuffer.SetData(args);
    }

    private void UpdateAnimationMembership(int id)
    {
        if (!IsValidRole(id) || data == null || data.clipFrameCounts == null)
        {
            SetAnimationMembership(id, false);
            return;
        }

        int clipIndex = clipIndices[id];
        bool animated = clipIndex >= 0
            && clipIndex < data.clipFrameCounts.Length
            && data.clipFrameCounts[clipIndex] > 1
            && !Mathf.Approximately(animSpeeds[id], 0f);

        SetAnimationMembership(id, animated);
    }

    private void SetAnimationMembership(int id, bool animated)
    {
        if (id < 0 || roleAnimated == null || id >= roleAnimated.Length || roleAnimated[id] == animated)
            return;

        roleAnimated[id] = animated;
        if (animated)
        {
            animatedRoleIds.Add(id);
            return;
        }

        animatedRoleIds.Remove(id);
    }

    private void SetTransformSyncMembership(int id, bool enabled)
    {
        if (id < 0 || roleSyncTransform == null || id >= roleSyncTransform.Length || roleSyncTransform[id] == enabled)
            return;

        roleSyncTransform[id] = enabled;
        if (enabled)
        {
            syncTransformRoleIds.Add(id);
            if (roleAgents[id] != null)
                roleAgents[id].transform.hasChanged = false;
            return;
        }

        syncTransformRoleIds.Remove(id);
    }

    private void RefreshDrawBounds(bool force)
    {
        if (!force && lastDrawBoundsCenter == drawBoundsCenter && lastDrawBoundsSize == drawBoundsSize)
            return;

        lastDrawBoundsCenter = drawBoundsCenter;
        lastDrawBoundsSize = drawBoundsSize;
        drawBounds = new Bounds(drawBoundsCenter, drawBoundsSize);
    }

    private bool IsReady()
    {
        return mesh != null
            && material != null
            && data != null
            && data.atlas != null
            && data.clips != null
            && data.clips.Count > 0
            && data.frames != null
            && data.frames.Count > 0
            && data.frameUVs != null
            && data.frameOffsetFrames != null
            && data.frameBaseSizes != null
            && data.clipStartFrames != null
            && data.clipFrameCounts != null
            && data.clipFpsValues != null
            && data.clipLoops != null
            && matrixBuffer != null
            && depthBiasBuffer != null
            && roleCount > 0;
    }

    public bool IsValidRole(int id)
    {
        return id >= 0 && id < activeCount && roleAlive[id];
    }

    private int ResolveCharacterIndex(string characterName)
    {
        if (data == null || !data.HasCharacters())
            return -1;

        if (!string.IsNullOrEmpty(characterName) && data.TryGetCharacterIndex(characterName, out int characterIndex))
            return characterIndex;

        return data.GetDefaultCharacterIndex();
    }

    private float GetFinalScale(int id)
    {
        float scale = renderScales[id];
        AnimCharacter character = data != null ? data.GetCharacter(characterIndices[id]) : null;
        return scale * (character != null ? Mathf.Max(0.01f, character.baseScale) : 1f);
    }

    private bool RoleHasShadow(int id)
    {
        if (data == null)
            return true;

        AnimCharacter character = data.GetCharacter(characterIndices[id]);
        return character != null ? character.hasShadow : data.hasShadow;
    }

    private void ReleaseAllSortingOrders()
    {
        if (sortingOrders == null || roleAlive == null)
            return;

        for (int i = 0; i < activeCount; i++)
        {
            if (!roleAlive[i])
                continue;

            Gpu2DDepthUtility.ReleaseSortingOrder(sortingOrders[i]);
            sortingOrders[i] = -1;
        }
    }

    private void ReleaseGpuBuffers()
    {
        if (matrixBuffer != null) matrixBuffer.Release();
        if (frameUVBuffer != null) frameUVBuffer.Release();
        if (frameOffsetBuffer != null) frameOffsetBuffer.Release();
        if (frameBaseCenterBuffer != null) frameBaseCenterBuffer.Release();
        if (frameShadowBuffer != null) frameShadowBuffer.Release();
        if (depthBiasBuffer != null) depthBiasBuffer.Release();
        if (renderIndexBuffer != null) renderIndexBuffer.Release();
        if (shadowRenderIndexBuffer != null) shadowRenderIndexBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
        if (shadowArgsBuffer != null) shadowArgsBuffer.Release();

        matrixBuffer = null;
        frameUVBuffer = null;
        frameOffsetBuffer = null;
        frameBaseCenterBuffer = null;
        frameShadowBuffer = null;
        depthBiasBuffer = null;
        renderIndexBuffer = null;
        shadowRenderIndexBuffer = null;
        argsBuffer = null;
        shadowArgsBuffer = null;
        bufferCapacity = 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showBounds) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(drawBoundsCenter, drawBoundsSize);
    }

    private void OnDestroy()
    {
        ReleaseAllSortingOrders();
        ReleaseGpuBuffers();
    }
}
