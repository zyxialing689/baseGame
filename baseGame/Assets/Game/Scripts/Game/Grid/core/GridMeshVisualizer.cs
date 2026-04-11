using UnityEngine;

public class GridMeshVisualizer
{
    private GridSystem grid;
    private Camera cam;

    private Mesh mesh;
    private MeshFilter meshFilter;

    private bool isActive = false;

    // ================= 初始化 =================

    public void Init(GridSystem grid, Camera cam)
    {
        this.grid = grid;
        this.cam = cam;

        GameObject go = new GameObject("GridMesh");
        meshFilter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // ⭐ 材质（一定要透明）
        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetActive(bool value)
    {
        isActive = value;

        if (meshFilter != null)
        {
            meshFilter.gameObject.SetActive(value);
        }
    }

    // ================= 主循环 =================

    public void Tick()
    {
        if (!isActive) return;

        Draw();
    }

    // ================= 绘制 =================

    void Draw()
    {
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        Vector2Int min = grid.WorldToGrid(bottomLeft);
        Vector2Int max = grid.WorldToGrid(topRight);

        min.x = Mathf.Clamp(min.x, 0, grid.Width - 1);
        min.y = Mathf.Clamp(min.y, 0, grid.Height - 1);
        max.x = Mathf.Clamp(max.x, 0, grid.Width - 1);
        max.y = Mathf.Clamp(max.y, 0, grid.Height - 1);

        int width = max.x - min.x + 1;
        int height = max.y - min.y + 1;

        int count = width * height;

        Vector3[] vertices = new Vector3[count * 4];
        int[] triangles = new int[count * 6];
        Color[] colors = new Color[count * 4];

        int v = 0;
        int t = 0;

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3 center = grid.GridToWorld(x, y);
                float gap = 0.05f; // ⭐ 间距（0~0.5）
                float size = grid.cellSize * (0.5f - gap);

                Vector3 v0 = center + new Vector3(-size, -size);
                Vector3 v1 = center + new Vector3(-size, size);
                Vector3 v2 = center + new Vector3(size, size);
                Vector3 v3 = center + new Vector3(size, -size);

                vertices[v + 0] = v0;
                vertices[v + 1] = v1;
                vertices[v + 2] = v2;
                vertices[v + 3] = v3;

                Color c = grid.IsOccupied(x, y)
                    ? new Color(1, 0, 0, 0.25f)
                    : new Color(0, 1, 0, 0.25f);

                colors[v + 0] = c;
                colors[v + 1] = c;
                colors[v + 2] = c;
                colors[v + 3] = c;

                triangles[t + 0] = v + 0;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;

                triangles[t + 3] = v + 0;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;

                v += 4;
                t += 6;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
    }
}