using UnityEngine;

public class GPUAnimManager : MonoBehaviour
{
    private const int BatchSize = 1023;

    public Mesh mesh;
    public Material material;
    public Material shadowMaterial;
    public AnimAtlasData data;

    public int initialCapacity = 128;
    public float fps = 10f;
    public float defaultScale = 1f;
    public bool useYAsDepth = true;
    public float yToDepthScale = 0.01f;
    public float depthOffset;
    public bool drawShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    public Vector2 shadowSize = new Vector2(44f, 14f);
    [Tooltip("Extra runtime shadow offset added on top of AnimAtlasData.shadowOffset.")]
    public Vector2 shadowOffset = Vector2.zero;
    public bool cullByCamera = true;
    public float cullPadding = 2f;
    private Matrix4x4[] matrices;
    private Vector3[] positions;
    private float[] renderScales;
    private bool[] roleAlive;
    private bool[] roleVisible;
    private bool[] flipXs;
    private int[] clipIndices;
    private float[] animTimes;
    private float[] animSpeeds;
    private int[] renderIndices;
    private int[] freeIds;

    private Matrix4x4[] batchMatrices;
    private Vector4[] batchUVs;
    private Vector4[] batchOffsetFrames;
    private Vector4[] batchBaseSizes;
    private Vector4[] batchCenterOffsets;
    private Vector4[] batchShadowOffsets;

    private MaterialPropertyBlock mpb;
    private readonly RenderIndexComparer renderIndexComparer = new RenderIndexComparer();
    private int activeCount;
    private int renderCount;
    private int roleCount;
    private int freeCount;

    public int RoleCount => roleCount;

    private void Start()
    {
        EnsureRuntime();
    }

    private void Update()
    {
        if (!IsReady())
        {
            return;
        }

        UpdateAnimData(Time.deltaTime);
        Draw();
    }

    public int CreateRole(Vector3 position, string clipName = null)
    {
        return CreateRole(position, defaultScale, clipName);
    }

    public int CreateRole(Vector3 position, float scale, string clipName = null)
    {
        EnsureRuntime();
        if (data == null || data.clips == null || data.clips.Count == 0)
        {
            return -1;
        }

        EnsureCapacity(activeCount + 1);

        int clipIndex = 0;
        if (!string.IsNullOrEmpty(clipName) && data != null)
        {
            data.TryGetClipIndex(clipName, out clipIndex);
        }

        int id = AllocRoleId();
        positions[id] = position;
        renderScales[id] = scale;
        roleAlive[id] = true;
        roleVisible[id] = true;
        flipXs[id] = false;
        clipIndices[id] = Mathf.Clamp(clipIndex, 0, data.clips.Count - 1);
        animTimes[id] = 0f;
        animSpeeds[id] = 1f;
        UpdateMatrix(id);
        return id;
    }

    public void RemoveRole(int id)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        roleAlive[id] = false;
        roleVisible[id] = false;
        freeIds[freeCount] = id;
        freeCount++;
        roleCount--;
    }

    public void SetPosition(int id, Vector3 position)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        positions[id] = position;
        UpdateMatrix(id);
    }

    public void SetScale(int id, float scale)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        renderScales[id] = scale;
        UpdateMatrix(id);
    }

    public void SetVisible(int id, bool visible)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        roleVisible[id] = visible;
    }

    public void SetFlipX(int id, bool flipX)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        flipXs[id] = flipX;
        UpdateMatrix(id);
    }

    public void SetClip(int id, string clipName, bool resetTime = true)
    {
        if (!IsValidRole(id) || data == null || !data.TryGetClipIndex(clipName, out int clipIndex))
        {
            return;
        }

        clipIndices[id] = clipIndex;
        if (resetTime)
        {
            animTimes[id] = 0f;
        }
    }

    public void SetSpeed(int id, float speed)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        animSpeeds[id] = speed;
    }

    public void SetAnimTime(int id, float time)
    {
        if (!IsValidRole(id))
        {
            return;
        }

        animTimes[id] = Mathf.Max(0f, time);
    }

    public void ClearRoles()
    {
        activeCount = 0;
        roleCount = 0;
        freeCount = 0;
    }

    [ContextMenu("Log Shadow Offset")]
    private void LogShadowOffset()
    {
        Vector2 dataOffset = data != null ? data.shadowOffset : Vector2.zero;
        Debug.Log(
            $"GPUAnim shadow offset data={dataOffset}, runtime={shadowOffset}, final={dataOffset + shadowOffset}",
            this
        );
    }

    private void UpdateAnimData(float deltaTime)
    {
        for (int i = 0; i < activeCount; i++)
        {
            if (!roleAlive[i])
            {
                continue;
            }

            AnimClip clip = data.clips[clipIndices[i]];
            if (clip == null || clip.frameCount <= 0)
            {
                continue;
            }

            float clipFps = clip.fps > 0f ? clip.fps : fps;
            animTimes[i] += deltaTime * animSpeeds[i];

            if (!clip.loop)
            {
                float maxTime = Mathf.Max(0f, (clip.frameCount - 1) / clipFps);
                animTimes[i] = Mathf.Min(animTimes[i], maxTime);
            }
        }
    }

    private void Draw()
    {
        mpb.SetTexture("_MainTex", data.atlas);
        BuildRenderIndices();
        if (renderCount == 0)
        {
            return;
        }

        for (int start = 0; start < renderCount; start += BatchSize)
        {
            int batchCount = Mathf.Min(BatchSize, renderCount - start);
            FillBatch(start, batchCount);

            mpb.SetVectorArray("_UV", batchUVs);
            mpb.SetVectorArray("_OffsetFrame", batchOffsetFrames);
            mpb.SetVectorArray("_BaseSize", batchBaseSizes);
            mpb.SetVectorArray("_CenterOffset", batchCenterOffsets);
            mpb.SetVectorArray("_ShadowOffset", batchShadowOffsets);
            mpb.SetFloat("_ShadowEnabled", drawShadow ? 1f : 0f);
            mpb.SetColor("_ShadowColor", shadowColor);
            mpb.SetVector("_ShadowSize", shadowSize);

            if (drawShadow && shadowMaterial != null)
            {
                Graphics.DrawMeshInstanced(
                    mesh,
                    0,
                    shadowMaterial,
                    batchMatrices,
                    batchCount,
                    mpb
                );
            }

            Graphics.DrawMeshInstanced(
                mesh,
                0,
                material,
                batchMatrices,
                batchCount,
                mpb
            );
        }
    }

    private void FillBatch(int start, int batchCount)
    {
        for (int i = 0; i < batchCount; i++)
        {
            int roleIndex = renderIndices[start + i];
            AnimClip clip = data.clips[clipIndices[roleIndex]];
            float clipFps = clip.fps > 0f ? clip.fps : fps;
            int frameIndex = Mathf.FloorToInt(animTimes[roleIndex] * clipFps);
            FrameRuntimeData frame = data.GetClipFrame(clip, frameIndex);

            batchMatrices[i] = matrices[roleIndex];
            batchUVs[i] = new Vector4(frame.uv.x, frame.uv.y, frame.uv.width, frame.uv.height);
            batchOffsetFrames[i] = new Vector4(frame.offset.x, frame.offset.y, frame.size.x, frame.size.y);
            batchBaseSizes[i] = new Vector4(frame.baseSize.x, frame.baseSize.y, 0f, 0f);
            batchCenterOffsets[i] = new Vector4(data.centerOffset.x, data.centerOffset.y, 0f, 0f);
            Vector2 finalShadowOffset = data.shadowOffset + shadowOffset;
            batchShadowOffsets[i] = new Vector4(finalShadowOffset.x, finalShadowOffset.y, 0f, 0f);
        }
    }

    private void BuildRenderIndices()
    {
        renderCount = 0;
        Rect cameraBounds = default;
        bool canCull = cullByCamera && TryGetCameraBounds(out cameraBounds);

        for (int i = 0; i < activeCount; i++)
        {
            if (roleAlive[i] && roleVisible[i] && (!canCull || IsRoleVisible(i, cameraBounds)))
            {
                renderIndices[renderCount] = i;
                renderCount++;
            }
        }

        renderIndexComparer.Positions = positions;
        System.Array.Sort(renderIndices, 0, renderCount, renderIndexComparer);
    }

    private bool TryGetCameraBounds(out Rect bounds)
    {
        Camera camera = UIManager.Instance.camera_scene;
        if (camera == null)
        {
            bounds = default;
            return false;
        }

        if (camera.orthographic)
        {
            float height = camera.orthographicSize * 2f;
            float width = height * camera.aspect;
            Vector3 center = camera.transform.position;
            bounds = new Rect(
                center.x - width * 0.5f - cullPadding,
                center.y - height * 0.5f - cullPadding,
                width + cullPadding * 2f,
                height + cullPadding * 2f
            );
            return true;
        }

        float distance = Mathf.Abs(camera.transform.position.z - transform.position.z);
        Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1f, 1f, distance));
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
        float scalePadding = Mathf.Abs(renderScales[id]);
        return position.x >= cameraBounds.xMin - scalePadding
            && position.x <= cameraBounds.xMax + scalePadding
            && position.y >= cameraBounds.yMin - scalePadding
            && position.y <= cameraBounds.yMax + scalePadding;
    }

    private void UpdateMatrix(int id)
    {
        float scale = renderScales[id];
        float xScale = flipXs[id] ? -scale : scale;
        Vector3 renderPosition = positions[id];
        if (useYAsDepth)
        {
            renderPosition.z = positions[id].z + depthOffset + positions[id].y * yToDepthScale;
        }

        matrices[id] = Matrix4x4.TRS(renderPosition, Quaternion.identity, new Vector3(xScale, scale, scale));
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
        {
            mpb = new MaterialPropertyBlock();
        }

        if (material == null)
        {
            Shader bodyShader = Shader.Find("Custom/GPUAnim_Instanced_URP");
            if (bodyShader != null)
            {
                material = new Material(bodyShader);
                material.hideFlags = HideFlags.DontSave;
            }
        }

        if (material != null)
        {
            material.enableInstancing = true;
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

        if (shadowMaterial != null)
        {
            shadowMaterial.enableInstancing = true;
        }

        if (batchMatrices == null)
        {
            batchMatrices = new Matrix4x4[BatchSize];
            batchUVs = new Vector4[BatchSize];
            batchOffsetFrames = new Vector4[BatchSize];
            batchBaseSizes = new Vector4[BatchSize];
            batchCenterOffsets = new Vector4[BatchSize];
            batchShadowOffsets = new Vector4[BatchSize];
        }

        EnsureCapacity(Mathf.Max(1, initialCapacity));
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
        if (matrices != null && matrices.Length >= capacity)
        {
            return;
        }

        int newCapacity = matrices == null ? Mathf.Max(1, capacity) : Mathf.Max(capacity, matrices.Length * 2);

        Matrix4x4[] newMatrices = new Matrix4x4[newCapacity];
        Vector3[] newPositions = new Vector3[newCapacity];
        float[] newRenderScales = new float[newCapacity];
        bool[] newRoleAlive = new bool[newCapacity];
        bool[] newRoleVisible = new bool[newCapacity];
        bool[] newFlipXs = new bool[newCapacity];
        int[] newClipIndices = new int[newCapacity];
        float[] newAnimTimes = new float[newCapacity];
        float[] newAnimSpeeds = new float[newCapacity];
        int[] newRenderIndices = new int[newCapacity];
        int[] newFreeIds = new int[newCapacity];

        if (matrices != null)
        {
            System.Array.Copy(matrices, newMatrices, activeCount);
            System.Array.Copy(positions, newPositions, activeCount);
            System.Array.Copy(renderScales, newRenderScales, activeCount);
            System.Array.Copy(roleAlive, newRoleAlive, activeCount);
            System.Array.Copy(roleVisible, newRoleVisible, activeCount);
            System.Array.Copy(flipXs, newFlipXs, activeCount);
            System.Array.Copy(clipIndices, newClipIndices, activeCount);
            System.Array.Copy(animTimes, newAnimTimes, activeCount);
            System.Array.Copy(animSpeeds, newAnimSpeeds, activeCount);
            System.Array.Copy(renderIndices, newRenderIndices, activeCount);
            System.Array.Copy(freeIds, newFreeIds, freeCount);
        }

        matrices = newMatrices;
        positions = newPositions;
        renderScales = newRenderScales;
        roleAlive = newRoleAlive;
        roleVisible = newRoleVisible;
        flipXs = newFlipXs;
        clipIndices = newClipIndices;
        animTimes = newAnimTimes;
        animSpeeds = newAnimSpeeds;
        renderIndices = newRenderIndices;
        freeIds = newFreeIds;
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
            && roleCount > 0;
    }

    public bool IsValidRole(int id)
    {
        return id >= 0 && id < activeCount && roleAlive[id];
    }

    private class RenderIndexComparer : System.Collections.Generic.IComparer<int>
    {
        public Vector3[] Positions;

        public int Compare(int left, int right)
        {
            return Positions[right].y.CompareTo(Positions[left].y);
        }
    }
}
