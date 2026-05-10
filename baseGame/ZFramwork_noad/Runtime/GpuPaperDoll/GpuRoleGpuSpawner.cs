using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GPU role stress-test spawner.
/// Random animation, outfit, color, and scale are applied only when spawning.
/// Update only moves transforms when moveAgents is enabled.
/// </summary>
public class GpuRoleGpuSpawner : MonoBehaviour
{
    [Header("References")]
    public GpuRoleGpuManager manager;
    public GpuRoleExportData exportData;

    [Header("Spawn")]
    public bool spawnOnStart = true;
    public int roleCount = 1000;
    public int columns = 50;
    public float spacing = 2.2f;

    [Header("Default Animation")]
    public int animIndex = 0;
    public float playbackSpeed = 1f;

    [Header("Default Visual")]
    public float scale = 1f;
    public Color color = Color.white;

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int randomSeed = 12345;

    [Header("Random Animation")]
    public bool randomAnim = true;
    public int randomAnimMinIndex = 0;
    public int randomAnimMaxIndex = -1;

    [Header("Random Playback Speed")]
    public bool randomPlaybackSpeed;
    public Vector2 playbackSpeedRange = new Vector2(0.85f, 1.15f);

    [Header("Random Group Variants")]
    public bool randomGroupVariants = true;
    [Range(0f, 1f)] public float groupNoneChance;

    [Header("Random Independent Slots")]
    public bool randomIndependentSlots = true;
    [Range(0f, 1f)] public float independentSlotNoneChance = 0.1f;

    [Header("Random Color")]
    public bool randomColor;
    [Range(0f, 1f)] public float randomColorSaturation = 0.25f;
    [Range(0f, 1f)] public float randomColorValue = 1f;

    [Header("Random Scale")]
    public bool randomScale;
    public Vector2 scaleRange = new Vector2(0.9f, 1.1f);

    [Header("Move")]
    public bool moveAgents;
    public float moveAmplitude = 0.25f;
    public float moveSpeed = 2f;

    [Header("Random Outfit Change (test)")]
    public bool randomOutfitChange;
    public Vector2 outfitChangeInterval = new Vector2(0f, 0f);
    public int maxOutfitChangesPerFrame = 100000;
    public bool logOutfitChangeStats = true;
    public float outfitChangeStatsInterval = 1f;

    [Header("Random Animation Switch (test)")]
    public bool randomAnimationSwitch;
    public Vector2 animationSwitchInterval = new Vector2(0.1f, 0.5f);
    public int maxAnimationSwitchesPerFrame = 1000;
    public bool avoidSameAnimationSwitch = true;
    public bool logAnimationSwitchStats = true;
    public float animationSwitchStatsInterval = 1f;

    private readonly List<Transform> _spawned = new List<Transform>();
    private readonly List<GpuRoleAgent> _spawnedAgents = new List<GpuRoleAgent>();
    private Vector3[] _basePositions;
    private float[] _nextOutfitChangeTimes;
    private float[] _nextAnimationSwitchTimes;
    private int _outfitChangeCursor;
    private int _animationSwitchCursor;
    private int _animationSwitchCount;
    private float _nextAnimationSwitchStatsTime;
    private float _nextOutfitChangeStatsTime;
    private int _lastManagerRebuildCount;
    private int _outfitChangeCount;

    private void Start()
    {
        if (spawnOnStart)
            Spawn();
    }

    private void Update()
    {
        float t = Time.time * moveSpeed;
        for (int i = 0; i < _spawned.Count; i++)
        {
            Transform tr = _spawned[i];
            if (tr == null) continue;

            if (moveAgents)
            {
                Vector3 pos = _basePositions[i];
                pos.x += Mathf.Sin(t + i * 0.173f) * moveAmplitude;
                pos.y += Mathf.Cos(t * 0.73f + i * 0.119f) * moveAmplitude;
                tr.position = pos;
            }
        }

        UpdateRandomOutfitChange();
    }

    private void LateUpdate()
    {
        UpdateRandomAnimationSwitch();
    }

    [ContextMenu("Respawn GPU Roles")]
    public void Spawn()
    {
        Clear();
        ResolveReferences();

        if (!ValidateReferences())
            return;

        if (useRandomSeed)
            Random.InitState(randomSeed);

        roleCount = Mathf.Max(1, roleCount);
        columns = Mathf.Max(1, columns);
        spacing = Mathf.Max(0.01f, spacing);

        _basePositions = new Vector3[roleCount];
        _nextOutfitChangeTimes = new float[roleCount];
        _nextAnimationSwitchTimes = new float[roleCount];
        _outfitChangeCursor = 0;
        _animationSwitchCursor = 0;
        _animationSwitchCount = 0;
        _outfitChangeCount = 0;
        _nextAnimationSwitchStatsTime = Time.time + Mathf.Max(0.1f, animationSwitchStatsInterval);
        _nextOutfitChangeStatsTime = Time.time + Mathf.Max(0.1f, outfitChangeStatsInterval);
        _lastManagerRebuildCount = manager != null ? manager.RebuildCount : 0;

        for (int i = 0; i < roleCount; i++)
        {
            int x = i % columns;
            int y = i / columns;
            Vector3 pos = transform.position + new Vector3(x * spacing, -y * spacing, 0f);
            _basePositions[i] = pos;

            GameObject go = new GameObject($"GpuRoleAgent_{i:0000}");
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;

            GpuRoleAgent agent = go.AddComponent<GpuRoleAgent>();
            agent.exportData = exportData;
            agent.manager = manager;
            agent.animIndex = GetRandomAnimIndex();
            agent.playbackSpeed = GetRandomPlaybackSpeed();
            agent.scale = GetRandomScale();
            agent.color = GetRandomColor();

            _spawned.Add(go.transform);
            _spawnedAgents.Add(agent);
            go.SetActive(true);

            ApplyRandomStyle(agent);
            ScheduleNextOutfitChange(i);
            ScheduleNextAnimationSwitch(i);
        }

        manager.MarkAgentStyleDirty(null);

    }

    [ContextMenu("Clear GPU Roles")]
    public void Clear()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            Transform tr = _spawned[i];
            if (tr == null) continue;

            if (Application.isPlaying)
                Destroy(tr.gameObject);
            else
                DestroyImmediate(tr.gameObject);
        }

        _spawned.Clear();
        _spawnedAgents.Clear();
        _basePositions = null;
        _nextOutfitChangeTimes = null;
        _nextAnimationSwitchTimes = null;
        _outfitChangeCount = 0;
        _animationSwitchCount = 0;
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void ResolveReferences()
    {
        if (manager == null)
            manager = Object.FindObjectOfType<GpuRoleGpuManager>();
    }

    private bool ValidateReferences()
    {
        if (manager == null)
        {
            Debug.LogError("[GpuRoleGpuSpawner] Missing GpuRoleGpuManager.");
            return false;
        }

        if (exportData == null)
        {
            Debug.LogError("[GpuRoleGpuSpawner] Missing GpuRoleExportData.");
            return false;
        }

        if (exportData.slots == null || exportData.slots.Count == 0)
        {
            Debug.LogError("[GpuRoleGpuSpawner] ExportData has no slots.");
            return false;
        }

        if (exportData.animations == null || exportData.animations.Count == 0)
        {
            Debug.LogError("[GpuRoleGpuSpawner] ExportData has no animations.");
            return false;
        }

        return true;
    }

    private void ApplyRandomStyle(GpuRoleAgent agent)
    {
        if (agent == null || exportData == null)
            return;

        float gnChance = randomGroupVariants ? groupNoneChance : 1f;
        float inChance = randomIndependentSlots ? independentSlotNoneChance : 1f;
        agent.RandomizeStyle(gnChance, inChance);
    }

    private int GetRandomAnimIndex()
    {
        int maxAnim = exportData.animations.Count - 1;
        if (!randomAnim)
            return Mathf.Clamp(animIndex, 0, maxAnim);

        int min = Mathf.Clamp(randomAnimMinIndex, 0, maxAnim);
        int max = randomAnimMaxIndex < 0 ? maxAnim : Mathf.Clamp(randomAnimMaxIndex, 0, maxAnim);
        if (max < min) max = min;

        return Random.Range(min, max + 1);
    }

    private float GetRandomPlaybackSpeed()
    {
        if (!randomPlaybackSpeed)
            return playbackSpeed;

        float min = Mathf.Min(playbackSpeedRange.x, playbackSpeedRange.y);
        float max = Mathf.Max(playbackSpeedRange.x, playbackSpeedRange.y);
        return Random.Range(min, max);
    }

    private float GetRandomScale()
    {
        if (!randomScale)
            return scale;

        float min = Mathf.Min(scaleRange.x, scaleRange.y);
        float max = Mathf.Max(scaleRange.x, scaleRange.y);
        return Random.Range(min, max);
    }

    private Color GetRandomColor()
    {
        if (!randomColor)
            return color;

        Color c = Color.HSVToRGB(Random.value, randomColorSaturation, randomColorValue);
        c.a = color.a;
        return c;
    }

    private void UpdateRandomOutfitChange()
    {
        if (!randomOutfitChange || _spawnedAgents.Count == 0)
            return;

        if (_nextOutfitChangeTimes == null || _nextOutfitChangeTimes.Length < _spawnedAgents.Count)
            _nextOutfitChangeTimes = new float[_spawnedAgents.Count];

        int maxChanges = Mathf.Clamp(maxOutfitChangesPerFrame, 1, _spawnedAgents.Count);
        int checkedCount = 0;
        int changedThisFrame = 0;
        float now = Time.time;

        while (checkedCount < _spawnedAgents.Count && changedThisFrame < maxChanges)
        {
            int index = _outfitChangeCursor;
            _outfitChangeCursor = (_outfitChangeCursor + 1) % _spawnedAgents.Count;
            checkedCount++;

            if (now < _nextOutfitChangeTimes[index])
                continue;

            GpuRoleAgent agent = _spawnedAgents[index];
            if (agent != null && agent.isActiveAndEnabled)
            {
                ApplyRandomStyle(agent);
                changedThisFrame++;
                _outfitChangeCount++;
            }

            ScheduleNextOutfitChange(index);
        }

        if (logOutfitChangeStats && now >= _nextOutfitChangeStatsTime)
        {
            int rebuildCount = manager != null ? manager.RebuildCount : 0;
            Debug.Log($"[GpuRoleGpuSpawner] Slot changes={_outfitChangeCount}, manager rebuild delta={rebuildCount - _lastManagerRebuildCount}, total rebuild={rebuildCount}");
            _outfitChangeCount = 0;
            _lastManagerRebuildCount = rebuildCount;
            _nextOutfitChangeStatsTime = now + Mathf.Max(0.1f, outfitChangeStatsInterval);
        }
    }

    private void UpdateRandomAnimationSwitch()
    {
        if (!randomAnimationSwitch || _spawnedAgents.Count == 0 || exportData == null || exportData.animations == null)
            return;

        int animCount = exportData.animations.Count;
        if (animCount <= 1)
            return;

        if (_nextAnimationSwitchTimes == null || _nextAnimationSwitchTimes.Length < _spawnedAgents.Count)
            _nextAnimationSwitchTimes = new float[_spawnedAgents.Count];

        int maxSwitches = Mathf.Clamp(maxAnimationSwitchesPerFrame, 1, _spawnedAgents.Count);
        int checkedCount = 0;
        int switchedThisFrame = 0;
        float now = Time.time;

        while (checkedCount < _spawnedAgents.Count && switchedThisFrame < maxSwitches)
        {
            int index = _animationSwitchCursor;
            _animationSwitchCursor = (_animationSwitchCursor + 1) % _spawnedAgents.Count;
            checkedCount++;

            if (now < _nextAnimationSwitchTimes[index])
                continue;

            GpuRoleAgent agent = _spawnedAgents[index];
            if (agent == null || !agent.isActiveAndEnabled)
            {
                ScheduleNextAnimationSwitch(index);
                continue;
            }

            int nextAnim = Random.Range(0, animCount);
            if (avoidSameAnimationSwitch && animCount > 1)
            {
                int current = agent.CurrentAnimIndex;
                nextAnim = Random.Range(0, animCount - 1);
                if (nextAnim >= current)
                    nextAnim++;
            }

            agent.TryPlay(nextAnim);
            ScheduleNextAnimationSwitch(index);
            switchedThisFrame++;
            _animationSwitchCount++;
        }

        if (logAnimationSwitchStats && now >= _nextAnimationSwitchStatsTime)
        {
            int rebuildCount = manager != null ? manager.RebuildCount : 0;
            Debug.Log($"[GpuRoleGpuSpawner] Random animation switches={_animationSwitchCount}, manager rebuild delta={rebuildCount - _lastManagerRebuildCount}, total rebuild={rebuildCount}");
            _animationSwitchCount = 0;
            _lastManagerRebuildCount = rebuildCount;
            _nextAnimationSwitchStatsTime = now + Mathf.Max(0.1f, animationSwitchStatsInterval);
        }
    }

    private void ScheduleNextAnimationSwitch(int index)
    {
        if (_nextAnimationSwitchTimes == null || index < 0 || index >= _nextAnimationSwitchTimes.Length)
            return;

        float min = Mathf.Min(animationSwitchInterval.x, animationSwitchInterval.y);
        float max = Mathf.Max(animationSwitchInterval.x, animationSwitchInterval.y);
        float delay = max <= 0f ? 0f : Random.Range(Mathf.Max(0f, min), max);
        _nextAnimationSwitchTimes[index] = Time.time + delay;
    }

    private void ScheduleNextOutfitChange(int index)
    {
        if (_nextOutfitChangeTimes == null || index < 0 || index >= _nextOutfitChangeTimes.Length)
            return;

        float min = Mathf.Min(outfitChangeInterval.x, outfitChangeInterval.y);
        float max = Mathf.Max(outfitChangeInterval.x, outfitChangeInterval.y);
        float delay = max <= 0f ? 0f : Random.Range(Mathf.Max(0f, min), max);
        _nextOutfitChangeTimes[index] = Time.time + delay;
    }
}
