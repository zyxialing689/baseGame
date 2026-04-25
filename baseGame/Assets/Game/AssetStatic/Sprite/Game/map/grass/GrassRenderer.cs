using UnityEngine;
using System.IO;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GrassRenderer : MonoBehaviour
{
    public TextAsset dataFile;

    public int mapSize = 200;
    public int clusterCount = 3000;

    List<Color> palette = new List<Color>();
    List<Template> templates = new List<Template>();

    Mesh mesh;

    void Start()
    {
        Load(dataFile.bytes);

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        Generate();
    }

    // =========================
    // ⭐ 读取数据
    // =========================
    void Load(byte[] bytes)
    {
        using (BinaryReader br = new BinaryReader(new MemoryStream(bytes)))
        {
            int paletteCount = br.ReadByte();

            for (int i = 0; i < paletteCount; i++)
            {
                float r = br.ReadByte() / 255f;
                float g = br.ReadByte() / 255f;
                float b = br.ReadByte() / 255f;

                palette.Add(new Color(r, g, b));
            }

            int templateCount = br.ReadUInt16();

            for (int i = 0; i < templateCount; i++)
            {
                int pixelCount = br.ReadUInt16();

                Template t = new Template();
                t.pixels = new PixelData[pixelCount];

                for (int j = 0; j < pixelCount; j++)
                {
                    t.pixels[j] = new PixelData
                    {
                        x = br.ReadSByte(),
                        y = br.ReadSByte(),
                        colorIndex = br.ReadByte()
                    };
                }

                templates.Add(t);
            }
        }
    }

    // =========================
    // ⭐ 生成草（重点改这里）
    // =========================
    void Generate()
    {
        List<Vector3> v = new List<Vector3>();
        List<int> t = new List<int>();
        List<Color> c = new List<Color>();

        int index = 0;

        for (int i = 0; i < clusterCount; i++)
        {
            float x = Random.Range(0, mapSize);
            float y = Random.Range(0, mapSize);

            // =========================
            // ⭐ 双层噪声（核心！！）
            // =========================
            float n1 = Mathf.PerlinNoise(x * 0.03f, y * 0.03f); // 大区域
            float n2 = Mathf.PerlinNoise(x * 0.15f, y * 0.15f); // 细节

            float density = n1 * 0.7f + n2 * 0.3f;

            // ⭐ 控制是否生成（空白区域）
            if (density < 0.45f) continue;

            // ⭐ 控制出现概率（避免太满）
            float spawnChance = Mathf.Lerp(0.2f, 1f, density);
            if (Random.value > spawnChance) continue;

            DrawCluster(x, y, density, v, t, c, ref index);
        }

        mesh.SetVertices(v);
        mesh.SetTriangles(t, 0);
        mesh.SetColors(c);
    }

    // =========================
    // ⭐ 草丛（密度控制）
    // =========================
    void DrawCluster(float cx, float cy, float density,
        List<Vector3> v,
        List<int> t,
        List<Color> c,
        ref int index)
    {
        // ⭐ 密度影响草数量
        int count = (int)Mathf.Lerp(5, 40, density);

        bool flip = Random.value > 0.5f;

        for (int i = 0; i < count; i++)
        {
            float ox = Random.Range(-0.6f, 0.6f);
            float oy = Random.Range(-0.6f, 0.6f);

            DrawTemplate(cx + ox, cy + oy, flip, v, t, c, ref index);
        }
    }

    // =========================
    // ⭐ 绘制模板
    // =========================
    void DrawTemplate(float x, float y, bool flip,
        List<Vector3> v,
        List<int> t,
        List<Color> c,
        ref int index)
    {
        Template temp = templates[Random.Range(0, templates.Count)];

        float pixel = Random.Range(0.01f, 0.018f);

        foreach (var p in temp.pixels)
        {
            float px = p.x * pixel * (flip ? -1 : 1);
            float py = p.y * pixel;

            Color col = palette[p.colorIndex];

            // ⭐ 防止颜色死板
            col *= Random.Range(0.95f, 1.05f);

            DrawPixel(v, t, c, ref index,
                x + px,
                y + py,
                pixel,
                col);
        }
    }

    // =========================
    // ⭐ 画像素
    // =========================
    void DrawPixel(List<Vector3> v, List<int> t, List<Color> c, ref int index,
        float x, float y, float size, Color col)
    {
        Vector3 v0 = new Vector3(x, y, 0);
        Vector3 v1 = new Vector3(x + size, y, 0);
        Vector3 v2 = new Vector3(x + size, y + size, 0);
        Vector3 v3 = new Vector3(x, y + size, 0);

        v.Add(v0);
        v.Add(v1);
        v.Add(v2);
        v.Add(v3);

        t.Add(index + 0);
        t.Add(index + 1);
        t.Add(index + 2);
        t.Add(index + 0);
        t.Add(index + 2);
        t.Add(index + 3);

        index += 4;

        c.Add(col);
        c.Add(col);
        c.Add(col);
        c.Add(col);
    }
}

// =========================
// 数据结构
// =========================


public class Template
{
    public PixelData[] pixels;
}