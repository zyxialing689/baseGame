using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AnimAtlasTool : EditorWindow
{
    private DefaultAsset characterFolder;

    private enum InputMode
    {
        Grid,
        Frames
    }

    private InputMode mode = InputMode.Grid;

    private int frameWidth = 128;
    private int frameHeight = 128;
    private Vector2 centerOffset;
    private Vector2 shadowOffset;
    private DefaultAsset outputFolder;
    private string outputName = "";
    private Texture2D previewAtlas;
    private Dictionary<string, List<FrameSource>> animFrames = new Dictionary<string, List<FrameSource>>();
    private const string KEY_CHAR = "AnimTool_Char";
    private const string KEY_OUT = "AnimTool_Out";
    private const string KEY_NAME = "AnimTool_Name";
    private const string KEY_MODE = "AnimTool_Mode";
    private const string KEY_W = "AnimTool_W";
    private const string KEY_H = "AnimTool_H";
    private const string KEY_CENTER_X = "AnimTool_Center_X";
    private const string KEY_CENTER_Y = "AnimTool_Center_Y";
    private const string KEY_SHADOW_X = "AnimTool_Shadow_X";
    private const string KEY_SHADOW_Y = "AnimTool_Shadow_Y";
    [MenuItem("ZFramework/Window/GPUAnim Atlas Tool")]
    public static void Open()
    {
        GetWindow<AnimAtlasTool>("AnimAtlas Tool");
    }
    private void OnEnable()
    {
        // load string paths
        string charPath = EditorPrefs.GetString(KEY_CHAR, "");
        string outPath = EditorPrefs.GetString(KEY_OUT, "");

        if (!string.IsNullOrEmpty(charPath))
            characterFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(charPath);

        if (!string.IsNullOrEmpty(outPath))
            outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(outPath);

        outputName = EditorPrefs.GetString(KEY_NAME, "");

        mode = (InputMode)EditorPrefs.GetInt(KEY_MODE, 0);

        frameWidth = EditorPrefs.GetInt(KEY_W, 128);
        frameHeight = EditorPrefs.GetInt(KEY_H, 128);
        centerOffset = new Vector2(
            EditorPrefs.GetFloat(KEY_CENTER_X, 0f),
            EditorPrefs.GetFloat(KEY_CENTER_Y, 0f)
        );
        shadowOffset = new Vector2(
            EditorPrefs.GetFloat(KEY_SHADOW_X, 0f),
            EditorPrefs.GetFloat(KEY_SHADOW_Y, 0f)
        );
    }
    private void SavePrefs()
    {
        if (characterFolder != null)
            EditorPrefs.SetString(KEY_CHAR, AssetDatabase.GetAssetPath(characterFolder));

        if (outputFolder != null)
            EditorPrefs.SetString(KEY_OUT, AssetDatabase.GetAssetPath(outputFolder));

        EditorPrefs.SetString(KEY_NAME, outputName);

        EditorPrefs.SetInt(KEY_MODE, (int)mode);

        EditorPrefs.SetInt(KEY_W, frameWidth);
        EditorPrefs.SetInt(KEY_H, frameHeight);
        EditorPrefs.SetFloat(KEY_CENTER_X, centerOffset.x);
        EditorPrefs.SetFloat(KEY_CENTER_Y, centerOffset.y);
        EditorPrefs.SetFloat(KEY_SHADOW_X, shadowOffset.x);
        EditorPrefs.SetFloat(KEY_SHADOW_Y, shadowOffset.y);
    }
    private void OnGUI()
    {
        GUILayout.Label("Character Animation Atlas", EditorStyles.boldLabel);

        characterFolder = (DefaultAsset)EditorGUILayout.ObjectField("Character Folder", characterFolder, typeof(DefaultAsset), false);
        GUILayout.Space(5);

        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Output Folder",
            outputFolder,
            typeof(DefaultAsset),
            false
        );

        outputName = EditorGUILayout.TextField("Output Name", outputName);
        centerOffset = EditorGUILayout.Vector2Field("Center Offset (Pixels)", centerOffset);
        shadowOffset = EditorGUILayout.Vector2Field("Shadow Offset (Pixels)", shadowOffset);
        EditorGUILayout.HelpBox("Offset is saved into AnimAtlasData. Positive Y moves up in sprite pixels, so 5 on a 256px frame is a small movement.", MessageType.Info);
        mode = (InputMode)EditorGUILayout.EnumPopup("Input Mode", mode);

        if (mode == InputMode.Grid)
        {
            frameWidth = EditorGUILayout.IntField("Frame Width", frameWidth);
            frameHeight = EditorGUILayout.IntField("Frame Height", frameHeight);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Scan Resources"))
        {
            ScanFolder();
        }

        if (GUILayout.Button("Build Trimmed Atlas"))
        {
            BuildAll();
        }

        GUILayout.Space(10);

        foreach (var kv in animFrames)
        {
            GUILayout.Label($"Clip: {kv.Key}  Frames: {kv.Value.Count}");
        }
        GUILayout.Space(10);

        if (previewAtlas != null)
        {
            GUILayout.Label("Atlas Preview", EditorStyles.boldLabel);

            float size = Mathf.Min(position.width - 20, 300);

            GUILayout.Label(previewAtlas, GUILayout.Width(size), GUILayout.Height(size));
        }
        SavePrefs();
    }

    // =========================================================
    // Scan source folder
    // =========================================================
    private void ScanFolder()
    {
        animFrames.Clear();

        string path = AssetDatabase.GetAssetPath(characterFolder);
        string[] subDirs = Directory.GetDirectories(path);

        foreach (var dir in subDirs)
        {
            string animName = Path.GetFileName(dir);
            List<FrameSource> frames = new List<FrameSource>();

            var files = Directory.GetFiles(dir);
            List<string> pngs = new List<string>();

            foreach (var f in files)
                if (f.EndsWith(".png")) pngs.Add(f);

            pngs.Sort();

            foreach (var file in pngs)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                if (tex == null) continue;

                if (mode == InputMode.Grid)
                    frames.AddRange(Split(tex));
                else
                    frames.Add(new FrameSource(tex, new Rect(0, 0, tex.width, tex.height)));
            }

            if (frames.Count > 0)
                animFrames.Add(animName, frames);
        }

        Debug.Log("Scan complete.");
    }

    // =========================================================
    // Split grid frames
    // =========================================================
    private List<FrameSource> Split(Texture2D tex)
    {
        List<FrameSource> list = new List<FrameSource>();

        int cols = tex.width / frameWidth;
        int rows = tex.height / frameHeight;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Rect r = new Rect(
                    x * frameWidth,
                    tex.height - (y + 1) * frameHeight,
                    frameWidth,
                    frameHeight
                );

                list.Add(new FrameSource(tex, r));
            }
        }

        return list;
    }
    private string GetOutputPath()
    {
        if (outputFolder == null)
        {
            Debug.LogError("No output folder");
            return null;
        }

        string folderPath = AssetDatabase.GetAssetPath(outputFolder);

        string finalName = string.IsNullOrEmpty(outputName)
            ? characterFolder.name
            : outputName;

        string fullPath = Path.Combine(folderPath, finalName);

        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(folderPath, finalName);
        }

        return fullPath;
    }
    // =========================================================
    // Build atlas
    // =========================================================
    private void BuildAll()
    {
        List<Texture2D> packedTexs = new List<Texture2D>();
        List<FrameRuntimeData> runtimeFrames = new List<FrameRuntimeData>();
        List<AnimClip> runtimeClips = new List<AnimClip>();

        foreach (var kv in animFrames)
        {
            AnimClip clip = new AnimClip();
            clip.name = kv.Key;
            clip.startFrame = runtimeFrames.Count;
            clip.frameCount = kv.Value.Count;
            clip.fps = 10f;
            clip.loop = true;
            runtimeClips.Add(clip);

            foreach (var frame in kv.Value)
            {
                var tex = Extract(frame);
                var trim = Trim(tex);

                packedTexs.Add(trim.texture);

                FrameRuntimeData data = new FrameRuntimeData();
                data.offset = trim.offset;
                data.size = new Vector2(trim.texture.width, trim.texture.height);
                data.baseSize = new Vector2(frame.pixelRect.width, frame.pixelRect.height);
                
                runtimeFrames.Add(data);
            }
        }

        if (packedTexs.Count == 0)
        {
            Debug.LogWarning("No animation frames found.");
            return;
        }

        Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Rect[] rects = atlas.PackTextures(packedTexs.ToArray(), 2, 2048);
        PackedAtlasResult packedAtlas = CropAtlas(atlas, rects);

        for (int i = 0; i < runtimeFrames.Count; i++)
        {
            runtimeFrames[i].uv = packedAtlas.rects[i];
        }

        Texture2D savedAtlas = SaveAtlas(packedAtlas.texture);
        SaveData(savedAtlas, runtimeClips, runtimeFrames, centerOffset, shadowOffset);
        previewAtlas = savedAtlas;
        Debug.Log("Anim atlas build complete.");
    }

    private PackedAtlasResult CropAtlas(Texture2D atlas, Rect[] rects)
    {
        int atlasWidth = atlas.width;
        int atlasHeight = atlas.height;
        int minX = atlasWidth;
        int minY = atlasHeight;
        int maxX = 0;
        int maxY = 0;

        for (int i = 0; i < rects.Length; i++)
        {
            Rect pixelRect = ToPixelRect(rects[i], atlasWidth, atlasHeight);
            minX = Mathf.Min(minX, Mathf.FloorToInt(pixelRect.xMin));
            minY = Mathf.Min(minY, Mathf.FloorToInt(pixelRect.yMin));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pixelRect.xMax));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(pixelRect.yMax));
        }

        minX = Mathf.Clamp(minX, 0, atlasWidth - 1);
        minY = Mathf.Clamp(minY, 0, atlasHeight - 1);
        maxX = Mathf.Clamp(maxX, minX + 1, atlasWidth);
        maxY = Mathf.Clamp(maxY, minY + 1, atlasHeight);

        int width = maxX - minX;
        int height = maxY - minY;
        Texture2D cropped = new Texture2D(width, height, TextureFormat.RGBA32, false);
        cropped.SetPixels(atlas.GetPixels(minX, minY, width, height));
        cropped.Apply();

        Rect[] croppedRects = new Rect[rects.Length];
        for (int i = 0; i < rects.Length; i++)
        {
            Rect pixelRect = ToPixelRect(rects[i], atlasWidth, atlasHeight);
            float x = (pixelRect.x - minX) / width;
            float y = (pixelRect.y - minY) / height;
            float w = pixelRect.width / width;
            float h = pixelRect.height / height;
            croppedRects[i] = new Rect(x, y, w, h);
        }

        return new PackedAtlasResult(cropped, croppedRects);
    }

    private Rect ToPixelRect(Rect uvRect, int atlasWidth, int atlasHeight)
    {
        return Rect.MinMaxRect(
            Mathf.Floor(uvRect.xMin * atlasWidth),
            Mathf.Floor(uvRect.yMin * atlasHeight),
            Mathf.Ceil(uvRect.xMax * atlasWidth),
            Mathf.Ceil(uvRect.yMax * atlasHeight)
        );
    }

    // =========================================================
    // Extract source frame
    // =========================================================
    private Texture2D Extract(FrameSource frame)
    {
        Texture2D tex = new Texture2D((int)frame.pixelRect.width, (int)frame.pixelRect.height);
        tex.SetPixels(frame.texture.GetPixels(
            (int)frame.pixelRect.x,
            (int)frame.pixelRect.y,
            (int)frame.pixelRect.width,
            (int)frame.pixelRect.height));
        tex.Apply();
        return tex;
    }

    // =========================================================
    // Trim transparent pixels
    // =========================================================
    private (Texture2D texture, Vector2 offset) Trim(Texture2D tex)
    {
        int minX = tex.width, minY = tex.height;
        int maxX = 0, maxY = 0;

        var pixels = tex.GetPixels();

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                if (pixels[y * tex.width + x].a > 0.01f)
                {
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        int w = maxX - minX + 1;
        int h = maxY - minY + 1;

        if (w <= 0 || h <= 0)
        {
            Texture2D emptyTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            emptyTex.SetPixel(0, 0, Color.clear);
            emptyTex.Apply();
            return (emptyTex, Vector2.zero);
        }

        Texture2D newTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        newTex.SetPixels(tex.GetPixels(minX, minY, w, h));
        newTex.Apply();

        Vector2 offset = new Vector2(minX, minY);

        return (newTex, offset);
    }

    // =========================================================
    // Save atlas texture
    // =========================================================
    private Texture2D SaveAtlas(Texture2D atlas)
    {
        string basePath = GetOutputPath();
        if (string.IsNullOrEmpty(basePath)) return null;

        string filePath = Path.Combine(basePath, "atlas.png");

        File.WriteAllBytes(filePath, atlas.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
    }

    // =========================================================
    // Save atlas data
    // =========================================================
    private void SaveData(Texture2D atlas, List<AnimClip> clips, List<FrameRuntimeData> frames, Vector2 centerOffset, Vector2 shadowOffset)
    {
        AnimAtlasData data = ScriptableObject.CreateInstance<AnimAtlasData>();
        data.atlas = atlas;
        data.centerOffset = centerOffset;
        data.shadowOffset = shadowOffset;
        data.clips = clips;
        data.frames = frames;

        string basePath = GetOutputPath();
        if (string.IsNullOrEmpty(basePath)) return;

        string assetPath = Path.Combine(basePath, "AnimAtlasData.asset");

        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
    }

    private struct PackedAtlasResult
    {
        public Texture2D texture;
        public Rect[] rects;

        public PackedAtlasResult(Texture2D texture, Rect[] rects)
        {
            this.texture = texture;
            this.rects = rects;
        }
    }
}

// =========================================================
// Data structure
// =========================================================

public class FrameSource
{
    public Texture2D texture;
    public Rect pixelRect;

    public FrameSource(Texture2D tex, Rect rect)
    {
        texture = tex;
        pixelRect = rect;
    }
}


