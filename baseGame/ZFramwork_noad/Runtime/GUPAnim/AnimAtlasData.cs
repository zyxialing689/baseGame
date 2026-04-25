using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AnimAtlasData", menuName = "Anim/AtlasData")]
public class AnimAtlasData : ScriptableObject
{
    public Texture2D atlas;
    public Vector2 centerOffset;
    public Vector2 shadowOffset;
    public List<AnimClip> clips;
    public List<FrameRuntimeData> frames;

    private Dictionary<string, int> clipIndexMap;

    public bool TryGetClip(string clipName, out AnimClip clip)
    {
        EnsureClipMap();

        if (clipIndexMap.TryGetValue(clipName, out int index))
        {
            clip = clips[index];
            return true;
        }

        clip = null;
        return false;
    }

    public bool TryGetClipIndex(string clipName, out int index)
    {
        EnsureClipMap();
        return clipIndexMap.TryGetValue(clipName, out index);
    }

    public FrameRuntimeData GetClipFrame(AnimClip clip, int clipFrameIndex)
    {
        if (frames == null || clip == null || clip.frameCount <= 0)
        {
            return default;
        }

        int frameOffset = clip.loop
            ? Mod(clipFrameIndex, clip.frameCount)
            : Mathf.Clamp(clipFrameIndex, 0, clip.frameCount - 1);

        return frames[clip.startFrame + frameOffset];
    }

    private void EnsureClipMap()
    {
        if (clipIndexMap != null)
        {
            return;
        }

        clipIndexMap = new Dictionary<string, int>();
        if (clips == null)
        {
            return;
        }

        for (int i = 0; i < clips.Count; i++)
        {
            if (clips[i] != null && !string.IsNullOrEmpty(clips[i].name))
            {
                clipIndexMap[clips[i].name] = i;
            }
        }
    }

    private int Mod(int value, int length)
    {
        int result = value % length;
        return result < 0 ? result + length : result;
    }
}

[System.Serializable]
public class AnimClip
{
    public string name;
    public int startFrame;
    public int frameCount;
    public float fps = 10f;
    public bool loop = true;
}

[System.Serializable]
public class FrameRuntimeData
{
    public Rect uv;
    public Vector2 offset;
    public Vector2 size;
    public Vector2 baseSize;
}
