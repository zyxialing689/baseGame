using UnityEngine;

public class BuildAreaVisualizer
{
    private GridSystem grid;
    private Camera cam;

    private Mesh mesh;
    private MeshFilter meshFilter;

    private bool isActive = false;

    // 当前建筑参数
    private int width;
    private int height;

    public void Init(GridSystem grid, Camera cam)
    {
        this.grid = grid;
        this.cam = cam;

        GameObject go = new GameObject("BuildArea");
        meshFilter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetActive(bool value)
    {
        isActive = value;
        meshFilter.gameObject.SetActive(value);
    }

    public void SetSize(int w, int h)
    {
        width = w;
        height = h;
    }

    public void Tick()
    {
        if (!isActive) return;

        Draw();
    }

    void Draw()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (!plane.Raycast(ray, out float enter)) return;

        Vector3 hit = ray.GetPoint(enter);
        Vector2Int origin = grid.WorldToGrid(hit);

        bool canPlace = grid.CanPlace(origin, width, height);

        int count = width * height;

        Vector3[] vertices = new Vector3[count * 4];
        int[] triangles = new int[count * 6];
        Color[] colors = new Color[count * 4];

        int v = 0;
        int t = 0;

        float gap = 0.1f;
        float size = grid.cellSize * (0.5f - gap);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gx = origin.x + x;
                int gy = origin.y + y;

                Vector3 center = grid.GridToWorld(gx, gy);

                Vector3 v0 = center + new Vector3(-size, -size);
                Vector3 v1 = center + new Vector3(-size, size);
                Vector3 v2 = center + new Vector3(size, size);
                Vector3 v3 = center + new Vector3(size, -size);

                vertices[v + 0] = v0;
                vertices[v + 1] = v1;
                vertices[v + 2] = v2;
                vertices[v + 3] = v3;

                Color c = canPlace
                    ? new Color(0, 1, 0, 0.4f)
                    : new Color(1, 0, 0, 0.4f);

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