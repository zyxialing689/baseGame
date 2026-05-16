using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUAgent))]
public class GPUAgentEditor : Editor
{
    private GPUAgent _agent;

    private void OnEnable()
    {
        _agent = (GPUAgent)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成子物体参考", GUILayout.Height(24)))
        {
            SpawnReferenceChild();
        }
        if (GUILayout.Button("删除子物体参考", GUILayout.Height(24)))
        {
            DeleteReferenceChild();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SpawnReferenceChild()
    {
        AnimAtlasData data = _agent.data != null ? _agent.data : (_agent.manager != null ? _agent.manager.data : null);
        if (data == null)
        {
            EditorUtility.DisplayDialog("提示", "请先给 GPUAgent 指定 Data", "确定");
            return;
        }
        data.EnsureRuntimeCache();

        // 解析 character 和 clip
        int characterIndex = -1;
        if (data.HasCharacters())
        {
            if (!string.IsNullOrEmpty(_agent.characterName))
            {
                if (!data.TryGetCharacterIndex(_agent.characterName, out characterIndex))
                    characterIndex = -1;
            }

            if (characterIndex < 0)
                characterIndex = data.GetDefaultCharacterIndex();
        }

        int clipIndex;
        if (!data.TryGetCharacterClipIndex(characterIndex, _agent.clipName, out clipIndex))
        {
            EditorUtility.DisplayDialog("提示", $"找不到 clip: {_agent.clipName}", "确定");
            return;
        }

        if (clipIndex < 0 || clipIndex >= data.clipFrameCounts.Length)
            return;

        AnimCharacter character = data.GetCharacter(characterIndex);
        Vector2 centerOffset = character != null ? character.centerOffset : data.centerOffset;
        float baseScale = character != null ? Mathf.Max(0.01f, character.baseScale) : 1f;

        // 第一帧
        int frameIndex = data.clipStartFrames[clipIndex];
        Vector4 uvRaw = data.frameUVs[frameIndex];
        Vector4 offsetSize = data.frameOffsetFrames[frameIndex];
        Vector4 baseSizeRaw = data.frameBaseSizes[frameIndex];

        Vector2 baseSize = new Vector2(baseSizeRaw.x, baseSizeRaw.y);
        Vector2 frameOffset = new Vector2(offsetSize.x, offsetSize.y);
        Vector2 frameSize = new Vector2(offsetSize.z, offsetSize.w);
        if (baseSize.y < 0.001f) baseSize.y = 1f;

        Vector2 anchorPixel = new Vector2(baseSize.x * 0.5f + centerOffset.x, centerOffset.y);

        // 可见区域的 anchoredPosition 空间顶点
        Vector2 minPixel = frameOffset;
        Vector2 maxPixel = frameOffset + frameSize;
        float invBaseH = 1f / baseSize.y;

        Vector3 v0 = new Vector3((minPixel.x - anchorPixel.x) * invBaseH, (minPixel.y - anchorPixel.y) * invBaseH, 0f);
        Vector3 v1 = new Vector3((maxPixel.x - anchorPixel.x) * invBaseH, (minPixel.y - anchorPixel.y) * invBaseH, 0f);
        Vector3 v2 = new Vector3((minPixel.x - anchorPixel.x) * invBaseH, (maxPixel.y - anchorPixel.y) * invBaseH, 0f);
        Vector3 v3 = new Vector3((maxPixel.x - anchorPixel.x) * invBaseH, (maxPixel.y - anchorPixel.y) * invBaseH, 0f);

        // 图集 UV
        Vector2 uvMin = new Vector2(uvRaw.x, uvRaw.y);
        Vector2 uvMax = new Vector2(uvRaw.x + uvRaw.z, uvRaw.y + uvRaw.w);

        DeleteReferenceChild();

        GameObject child = new GameObject($"{_agent.gameObject.name}_参考");
        child.transform.SetParent(_agent.transform, false);
        child.transform.localPosition = _agent.positionOffset;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one * _agent.scale * baseScale;
        Undo.RegisterCreatedObjectUndo(child, "Create Reference Child");

        Mesh mesh = new Mesh();
        mesh.name = $"RefFrame_{frameIndex}";
        mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
        mesh.uv = new Vector2[]
        {
            new Vector2(uvMin.x, uvMin.y),
            new Vector2(uvMax.x, uvMin.y),
            new Vector2(uvMin.x, uvMax.y),
            new Vector2(uvMax.x, uvMax.y)
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateBounds();

        MeshFilter mf = child.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = data.atlas;
        child.AddComponent<MeshRenderer>().sharedMaterial = mat;

        // flipX
        if (_agent.flipX)
        {
            Vector3 s = child.transform.localScale;
            s.x = -s.x;
            child.transform.localScale = s;
        }

        Selection.activeGameObject = child;
        Debug.Log($"[GPUAgentEditor] 已创建参考: {child.name}", child);
    }

    private void DeleteReferenceChild()
    {
        string suffix = "_参考";
        for (int i = _agent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = _agent.transform.GetChild(i);
            if (child.name.EndsWith(suffix))
                Undo.DestroyObjectImmediate(child.gameObject);
        }
    }
}
