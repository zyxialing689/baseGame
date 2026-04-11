using System.Collections.Generic;
using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem Instance;

    [Header("相机")]
    public Camera cam;

    // ⭐ 所有系统
    private GridSystem grid;
    private PlacementPreview preview;
    private GridMeshVisualizer visualizer;
    private BuildAreaVisualizer areaVisualizer;
    private BuildSaveSystem saveSystem;
    private bool isBuildMode = false;
    private bool isContinuous = false;
    private bool isDeleteMode = false;
    private bool deleteContinuous = false;
    public BuildSaveSystem SaveSystem => saveSystem;
    private Dictionary<int, GameObject> buildingMap = new Dictionary<int, GameObject>();
    private int lastDeleteId = -1;
    void Awake()
    {
        Instance = this;
        cam = UIManager.Instance.camera_scene;
        // ⭐ 初始化所有系统
        grid = new GridSystem();
        grid.Init(200, 200, 1f);

        preview = new PlacementPreview();
        preview.Init(grid, cam);

        visualizer = new GridMeshVisualizer();
        visualizer.Init(grid, cam);
        visualizer.SetActive(false);

        areaVisualizer = new BuildAreaVisualizer();
        areaVisualizer.Init(grid, cam);
        areaVisualizer.SetActive(false);

        saveSystem = new BuildSaveSystem();
        saveSystem.Load();
        LoadAll();
    }

    void Update()
    {
        if (isBuildMode)
        {
            preview.Tick();
            visualizer.Tick();
            areaVisualizer.Tick();
        }
        else if (isDeleteMode)
        {
            HandleDelete();
        }

    }

    // ================= 对外接口 =================

    public void StartBuild(GameObject prefab)
    {
        if (prefab == null) return;
        var config = prefab.GetComponent<BuildingConfig>();
        if (config == null)
        {
            Debug.LogError("Prefab没有BuildingConfig！");
            return;
        }
        isContinuous = config.isContinuous;
        isBuildMode = true;

        preview.SetActive(true);
        preview.SetBuilding(prefab, config.width, config.height);

        visualizer.SetActive(true);

        areaVisualizer.SetActive(true);
        areaVisualizer.SetSize(config.width, config.height);

        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = false;
    }

    public void StopBuild()
    {
        isBuildMode = false;
        preview.SetActive(false);
        visualizer.SetActive(false);
        areaVisualizer.SetActive(false);
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = true;
    }

    public void StartDelete(bool continuous)
    {
        isDeleteMode = true;
        deleteContinuous = continuous;

        StopBuild(); // ⭐ 进入删除时退出建造
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = false;
    }

    public void StopDelete()
    {
        isDeleteMode = false;
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = true;
    }
    public void OnPlaceSuccess()
    {
        if (!isContinuous)
        {
            StopBuild();
        }
    }
    public bool IsContinuous()
    {
        return isContinuous;
    }

    void LoadAll()
    {
        var list = saveSystem.GetAll();

        foreach (var data in list)
        {
            // ⭐ 加载Prefab
            GameObject prefab = Resources.Load<GameObject>(data.prefabId);

            if (prefab == null)
            {
                Debug.LogError("找不到Prefab：" + data.prefabId);
                continue;
            }

            // ⭐ 计算位置
            Vector3 world = grid.GridToWorld(data.x, data.y);
            world.x += (data.width - 1) * grid.cellSize * 0.5f;
            world.y += (data.height - 1) * grid.cellSize * 0.5f;

            // ⭐ 创建
            GameObject go = Instantiate(prefab);
            go.transform.position = world;
            RegisterBuilding(data.instanceId, go);
            // ⭐ 占用Grid
            grid.SetOccupied(
                new Vector2Int(data.x, data.y),
                data.width,
                data.height,
                true,
                data.instanceId
            );
            // ⭐ 更新A*
            Bounds bounds = new Bounds(
                world,
                new Vector3(data.width * grid.cellSize, data.height * grid.cellSize, 1)
            );

            var guo = new Pathfinding.GraphUpdateObject(bounds);
            guo.modifyWalkability = true;
            guo.setWalkability = false;

            AstarPath.active.UpdateGraphs(guo);
        }
    }
    public void RegisterBuilding(int instanceId, GameObject go)
    {
        if (buildingMap.ContainsKey(instanceId))
        {
            Debug.LogWarning("重复注册 building：" + instanceId);
            buildingMap[instanceId] = go;
        }
        else
        {
            buildingMap.Add(instanceId, go);
        }
    }


    void HandleDelete()
    {
        // ⭐ 单次删除
        if (!deleteContinuous && Input.GetMouseButtonDown(0))
        {
            TryDeleteOne();
        }

        // ⭐ 连续删除
        if (deleteContinuous && Input.GetMouseButton(0))
        {
            TryDeleteContinuous();
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastDeleteId = -1;
        }
    }
    void TryDeleteOne()
    {
        int id = GetBuildingAtMouse();

        if (id != -1)
        {
            RemoveBuilding(id);
        }
    }
    void TryDeleteContinuous()
    {
        int id = GetBuildingAtMouse();

        if (id != -1 && id != lastDeleteId)
        {
            RemoveBuilding(id);
            lastDeleteId = id;
        }
    }
    int GetBuildingAtMouse()
    {
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;

        Vector2Int gridPos = grid.WorldToGrid(world);

        int id = grid.GetBuildingId(gridPos.x, gridPos.y);

        return id; // 没有就是0
    }
    public void RemoveBuilding(int instanceId)
    {
        // ⭐ 1. 找到物体
        if (!buildingMap.TryGetValue(instanceId, out var go))
            return;

        // ⭐ 2. 找存档数据
        var data = saveSystem.GetAll().Find(d => d.instanceId == instanceId);
        if (data == null) return;

        // ================= 删除流程 =================

        // ⭐ 3. 删除场景物体
        Destroy(go);
        buildingMap.Remove(instanceId);

        // ⭐ 4. 清除Grid占用
        grid.SetOccupied(
            new Vector2Int(data.x, data.y),
            data.width,
            data.height,
            false,
            0
        );

        // ⭐ 5. 恢复A*（关键）
        Vector3 world = grid.GridToWorld(data.x, data.y);
        world.x += (data.width - 1) * grid.cellSize * 0.5f;
        world.y += (data.height - 1) * grid.cellSize * 0.5f;

        Bounds bounds = new Bounds(
            world,
            new Vector3(data.width * grid.cellSize, data.height * grid.cellSize, 1)
        );

        var guo = new Pathfinding.GraphUpdateObject(bounds);
        guo.modifyWalkability = true;
        guo.setWalkability = true; // ⭐ 恢复可走

        AstarPath.active.UpdateGraphs(guo);

        // ⭐ 6. 删除存档
        saveSystem.RemoveBuilding(instanceId);

        // ⭐ 7. 可选：立即保存（测试阶段建议开）
        // saveSystem.Save();

        Debug.Log($"删除建筑成功：{instanceId}");
    }
}