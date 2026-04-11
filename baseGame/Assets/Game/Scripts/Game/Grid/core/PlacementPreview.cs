using UnityEngine;
using Pathfinding;

public class PlacementPreview
{
    private GridSystem grid;
    private Camera cam;

    private GameObject buildingPrefab;
    private int buildingWidth;
    private int buildingHeight;

    private bool isActive = false;
    private Plane groundPlane;

    private int buildingIdCounter = 1;
    private Vector2Int lastPlacePos = new Vector2Int(-9999, -9999);
    // ================= 初始化 =================

    public void Init(GridSystem grid, Camera cam)
    {
        this.grid = grid;
        this.cam = cam;

        groundPlane = new Plane(Vector3.forward, Vector3.zero);
    }

    public void SetActive(bool value)
    {
        isActive = value;
    }

    public void SetBuilding(GameObject prefab, int w, int h)
    {
        buildingPrefab = prefab;
        buildingWidth = w;
        buildingHeight = h;
    }

    // ================= 主循环 =================

    public void Tick()
    {
        if (!isActive) return;

        UpdatePreview();
    }

    void UpdatePreview()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!groundPlane.Raycast(ray, out float enter)) return;

        Vector3 hit = ray.GetPoint(enter);
        Vector2Int origin = grid.WorldToGrid(hit);

        bool canPlace = grid.CanPlace(origin, buildingWidth, buildingHeight);

        // ⭐ 单次建造（建筑）
        if (Input.GetMouseButtonDown(0) && !BuildSystem.Instance.IsContinuous())
        {
            if (canPlace)
            {
                Place(origin);
                BuildSystem.Instance.OnPlaceSuccess();
            }
        }

        // ⭐ 连续建造（树木）
        if (Input.GetMouseButton(0) && BuildSystem.Instance.IsContinuous())
        {
            // ⭐ 避免重复放同一个格子
            if (origin != lastPlacePos)
            {
                if (canPlace)
                {
                    Place(origin);
                    lastPlacePos = origin;
                }
            }
        }

        // ⭐ 松开鼠标时重置
        if (Input.GetMouseButtonUp(0))
        {
            lastPlacePos = new Vector2Int(-9999, -9999);
        }
    }

    // ================= 放置 =================

    void Place(Vector2Int origin)
    {
        int id = buildingIdCounter++;

        grid.SetOccupied(origin, buildingWidth, buildingHeight, true, id);

        Vector3 world = grid.GridToWorld(origin.x, origin.y);
        world.x += (buildingWidth - 1) * grid.cellSize * 0.5f;
        world.y += (buildingHeight - 1) * grid.cellSize * 0.5f;

        GameObject go = Object.Instantiate(buildingPrefab);
        go.transform.position = world;

        // ⭐ 更新A*
        Bounds bounds = new Bounds(
            world,
            new Vector3(buildingWidth * grid.cellSize, buildingHeight * grid.cellSize, 1)
        );

        var guo = new GraphUpdateObject(bounds);
        guo.modifyWalkability = true;
        guo.setWalkability = false;

        AstarPath.active.UpdateGraphs(guo);
    }
}