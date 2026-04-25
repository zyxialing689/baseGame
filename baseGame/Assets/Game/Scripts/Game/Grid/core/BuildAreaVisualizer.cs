using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class BuildAreaVisualizer
{
    private GridSystem grid;
    private Camera cam;

    private Mesh mesh;
    private MeshFilter meshFilter;

    private bool isActive = false;
    private GameObject buildingPrefab;
    private int buildingWidth;
    private int buildingHeight;
    private Vector2Int lastPlacedOrigin = new Vector2Int(int.MinValue, int.MinValue);
    // Current building parameters

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
        canPlace = false;
        lastPlacedOrigin = new Vector2Int(int.MinValue, int.MinValue);
        if (!value && BuildSystem.Instance != null)
        {
            BuildSystem.Instance.CancelBuildBatch();
        }
        isActive = value;
        meshFilter.gameObject.SetActive(value);
    }

    public void SetBuilding(GameObject prefab, int w, int h)
    {
        buildingPrefab = prefab;
        buildingWidth = w;
        buildingHeight = h;
    }

    public void Tick()
    {
        if (!isActive) return;

        Draw();
    }
    bool canPlace = false;
    Vector2Int origin = Vector2Int.zero;
    void Draw()
    {
        bool mouseHeld = Input.GetMouseButton(0);
        bool mousePressed = Input.GetMouseButtonDown(0);
        bool mouseReleased = Input.GetMouseButtonUp(0);
        bool continuous = BuildSystem.Instance != null && BuildSystem.Instance.IsContinuous();

        if (continuous && mousePressed && BuildSystem.Instance != null)
        {
            BuildSystem.Instance.BeginBuildBatch();
        }

        if (!mouseHeld)
        {
            if (mouseReleased)
            {
                if (!continuous && canPlace)
                {
                    Place(origin);
                    var gpInfo = EventGP_palceInfo.AutoCreate();
                    gpInfo.origin = origin;
                    gpInfo.width = buildingWidth;
                    gpInfo.height = buildingHeight;
                    EventManager.Instance.Dispatch(gpInfo);
                    Debug.Log("Can place current position.");
                }

                if (continuous && BuildSystem.Instance != null)
                {
                    BuildSystem.Instance.CommitBuildBatch();
                }
            }
            lastPlacedOrigin = new Vector2Int(int.MinValue, int.MinValue);
            mesh.Clear();
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (!plane.Raycast(ray, out float enter)) return;

        Vector3 hit = ray.GetPoint(enter);
        origin = grid.WorldToGrid(hit);

        canPlace = grid.CanPlace(origin, buildingWidth, buildingHeight);

        if (continuous && canPlace && origin != lastPlacedOrigin && (mousePressed || mouseHeld))
        {
            if (BuildSystem.Instance != null && !BuildSystem.Instance.IsBuildBatchOpen())
            {
                BuildSystem.Instance.BeginBuildBatch();
            }

            Place(origin);
            lastPlacedOrigin = origin;

            var gpInfo = EventGP_palceInfo.AutoCreate();
            gpInfo.origin = origin;
            gpInfo.width = buildingWidth;
            gpInfo.height = buildingHeight;
            EventManager.Instance.Dispatch(gpInfo);
            Debug.Log("Can place current position.");
        }

        int count = buildingWidth * buildingHeight;

        Vector3[] vertices = new Vector3[count * 4];
        int[] triangles = new int[count * 6];
        Color[] colors = new Color[count * 4];

        int v = 0;
        int t = 0;

        float gap = 0.1f;
        float size = grid.cellSize * (0.5f - gap);

        for (int x = 0; x < buildingWidth; x++)
        {
            for (int y = 0; y < buildingHeight; y++)
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

    void Place(Vector2Int origin)
    {
        var config = buildingPrefab.GetComponent<BuildingConfig>();

        BuildingData data = BuildSystem.Instance.SaveSystem.AddBuilding(
            config.prefabId,
            origin,
            buildingWidth,
            buildingHeight
        );

        int id = data.instanceId;

        grid.SetOccupied(origin, buildingWidth, buildingHeight, true, id);

        Vector3 world = grid.GridToWorld(origin.x, origin.y);
        world.x += (buildingWidth - 1) * grid.cellSize * 0.5f;
        world.y += (buildingHeight - 1) * grid.cellSize * 0.5f;

        GameObject go = Object.Instantiate(buildingPrefab);
        go.transform.SetParent(BuildSystem.Instance.transform);
        go.transform.position = world;
        go.GetComponent<BuildingConfig>().SetOrder(world);
        BuildSystem.Instance.RegisterBuilding(id, go);

        // Block grass
        var grass = BuildSystem.Instance.grass;

        if (grass != null)
        {
            Vector2 center = new Vector2(world.x, world.y);

            Vector2 size = new Vector2(
                buildingWidth * grid.cellSize,
                buildingHeight * grid.cellSize
            );

            grass.AddBlock(center, size);
        }

        // Update A*
        Bounds bounds = new Bounds(
            world,
            new Vector3(buildingWidth * grid.cellSize, buildingHeight * grid.cellSize, 1)
        );

        var guo = new GraphUpdateObject(bounds);
        guo.modifyWalkability = true;
        guo.setWalkability = false;

        AstarPath.active.UpdateGraphs(guo);

        if (BuildSystem.Instance != null)
        {
            BuildSystem.Instance.RegisterPlacedBuilding(id);
            BuildSystem.Instance.OnPlaceSuccess();
        }
    }
}
