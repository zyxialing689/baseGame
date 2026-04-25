using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem Instance;

    public bool useGrassSystem = true;
    public Material material;
    public GrassAtlasData atlasData;
    public DistributionType distribution = DistributionType.Cluster;
    public int mapSeed;
    public int targetCount = 20000;
    public float density = 1.0f;
    [HideInInspector]
    public GrassIndirectRenderer grass;
    private Camera cam;

    // Core build systems
    private GridSystem grid;
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
    private enum UndoActionType
    {
        Place,
        Delete
    }

    private class UndoRecord
    {
        public UndoActionType actionType;
        public int instanceId;
        public BuildingData data;
    }

    private Stack<List<UndoRecord>> buildHistory = new Stack<List<UndoRecord>>();
    private List<UndoRecord> currentBuildBatch = new List<UndoRecord>();
    private bool buildBatchOpen = false;
    void Awake()
    {
        Instance = this;
        cam = UIManager.Instance.camera_scene;
        // Init core systems
        grid = new GridSystem();
        grid.Init(200, 200, 1f);

        visualizer = new GridMeshVisualizer();
        visualizer.Init(grid, cam);
        visualizer.SetActive(false);

        areaVisualizer = new BuildAreaVisualizer();
        areaVisualizer.Init(grid, cam);
        areaVisualizer.SetActive(false);

        saveSystem = new BuildSaveSystem();
        saveSystem.Load();

        if (useGrassSystem)
        {
            grass = gameObject.AddComponent<GrassIndirectRenderer>();
            grass.Init(distribution, mapSeed, material, atlasData, targetCount, density);
        }
        LoadAll();
    }

    void Update()
    {
        bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();

        if ((isBuildMode || isDeleteMode) && !pointerOverUI && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastBuild();
            return;
        }

        if (isBuildMode)
        {
            visualizer.Tick();
            if (pointerOverUI)
            {
                if (isContinuous && Input.GetMouseButtonUp(0))
                {
                    CommitBuildBatch();
                }
                return;
            }
            areaVisualizer.Tick();
        }
        else if (isDeleteMode)
        {
            visualizer.Tick();
            if (pointerOverUI)
            {
                if (deleteContinuous && Input.GetMouseButtonUp(0))
                {
                    CommitBuildBatch();
                    lastDeleteId = -1;
                }
                return;
            }

            HandleDelete();
        }

    }
    // ================= Public API =================

    public void StartBuild(GameObject prefab)
    {
        if (prefab == null) return;
        var config = prefab.GetComponent<BuildingConfig>();
        if (config == null)
        {
            Debug.LogError("Prefab missing BuildingConfig.");
            return;
        }
        CommitBuildBatch();
        isContinuous = config.isContinuous;
        isBuildMode = true;
        isDeleteMode = false;
        lastDeleteId = -1;
        currentBuildBatch.Clear();
        buildBatchOpen = false;
        visualizer.SetActive(true);
        areaVisualizer.SetActive(true);
        areaVisualizer.SetBuilding(prefab,config.width, config.height);

        // EventManager.Instance.Dispatch()
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = false;
    }

    public void StopBuild()
    {
        bool wasEditing = isBuildMode;
        CommitBuildBatch();
        isBuildMode = false;
        visualizer.SetActive(false);
        areaVisualizer.SetActive(false);
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = true;
        if (wasEditing)
        {
            FinishBuildSession();
        }
    }

    public void StartDelete(bool continuous)
    {
        CommitBuildBatch();
        isBuildMode = false;
        isDeleteMode = true;
        deleteContinuous = continuous;
        currentBuildBatch.Clear();
        buildBatchOpen = false;
        areaVisualizer.SetActive(false);
        visualizer.SetActive(true);
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = false;
    }

    public void StopDelete()
    {
        bool wasEditing = isDeleteMode;
        isDeleteMode = false;
        CommitBuildBatch();
        visualizer.SetActive(false);
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = true;
        if (wasEditing)
        {
            FinishBuildSession();
        }
    }

    IEnumerator EnableCameraNextFrame()
    {
        yield return null;

        UIManager.Instance.camera_scene
            .GetComponent<CameraController2D>()
            .enabled = true;
    }
    public void OnPlaceSuccess()
    {
    }
    public bool IsContinuous()
    {
        return isContinuous;
    }

    public bool IsBuildBatchOpen()
    {
        return buildBatchOpen;
    }

    public void BeginBuildBatch()
    {
        currentBuildBatch.Clear();
        buildBatchOpen = true;
    }

    public void RegisterPlacedBuilding(int instanceId)
    {
        if (instanceId <= 0)
        {
            return;
        }

        UndoRecord record = new UndoRecord()
        {
            actionType = UndoActionType.Place,
            instanceId = instanceId
        };

        if (buildBatchOpen)
        {
            currentBuildBatch.Add(record);
            return;
        }

        buildHistory.Push(new List<UndoRecord> { record });
    }

    public void RegisterDeletedBuilding(BuildingData data)
    {
        if (data == null)
        {
            return;
        }

        UndoRecord record = new UndoRecord()
        {
            actionType = UndoActionType.Delete,
            instanceId = data.instanceId,
            data = CloneBuildingData(data)
        };

        if (buildBatchOpen)
        {
            currentBuildBatch.Add(record);
            return;
        }

        buildHistory.Push(new List<UndoRecord> { record });
    }

    public void CommitBuildBatch()
    {
        if (!buildBatchOpen)
        {
            return;
        }

        if (currentBuildBatch.Count > 0)
        {
            buildHistory.Push(new List<UndoRecord>(currentBuildBatch));
        }

        currentBuildBatch.Clear();
        buildBatchOpen = false;
    }

    public void CancelBuildBatch()
    {
        currentBuildBatch.Clear();
        buildBatchOpen = false;
    }

    public void UndoLastBuild()
    {
        if (!isBuildMode && !isDeleteMode)
        {
            return;
        }

        if (buildBatchOpen)
        {
            return;
        }

        if (buildHistory.Count == 0)
        {
            return;
        }

        List<UndoRecord> batch = buildHistory.Pop();
        for (int i = batch.Count - 1; i >= 0; i--)
        {
            UndoRecord record = batch[i];
            if (record.actionType == UndoActionType.Place)
            {
                RemoveBuildingInternal(record.instanceId);
            }
            else if (record.actionType == UndoActionType.Delete)
            {
                RestoreBuilding(record.data);
            }
        }
    }

    private void FinishBuildSession()
    {
        CommitBuildBatch();
        bool hasChanges = buildHistory.Count > 0;
        buildHistory.Clear();
        currentBuildBatch.Clear();
        buildBatchOpen = false;
        if (hasChanges)
        {
            saveSystem.Save();
        }
    }

    void LoadAll()
    {
        var list = saveSystem.GetAll();

        foreach (var data in list)
        {
            // Load prefab
            GameObject prefab = Resources.Load<GameObject>(data.prefabId);

            if (prefab == null)
            {
                Debug.LogError("Can not find prefab: " + data.prefabId);
                continue;
            }

            // Calculate world position
            Vector3 world = grid.GridToWorld(data.x, data.y);
            world.x += (data.width - 1) * grid.cellSize * 0.5f;
            world.y += (data.height - 1) * grid.cellSize * 0.5f;

            // Block grass
            if (grass != null)
            {
                Vector2 center = new Vector2(world.x, world.y);

                Vector2 size = new Vector2(
                    data.width * grid.cellSize,
                    data.height * grid.cellSize
                );

                grass.AddBlock(center, size);
            }

            // Create building object
            GameObject go = Instantiate(prefab);
            go.transform.position = world;
            RegisterBuilding(data.instanceId, go);
            // Occupy grid cells
            grid.SetOccupied(
                new Vector2Int(data.x, data.y),
                data.width,
                data.height,
                true,
                data.instanceId
            );
            // Update A*
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
            Debug.LogWarning("Duplicate building registration: " + instanceId);
            buildingMap[instanceId] = go;
        }
        else
        {
            buildingMap.Add(instanceId, go);
        }
    }


    void HandleDelete()
    {
        if (deleteContinuous && Input.GetMouseButtonDown(0))
        {
            BeginBuildBatch();
        }

        // Single delete
        if (!deleteContinuous && Input.GetMouseButtonDown(0))
        {
            TryDeleteOne();
        }

        // Continuous delete
        if (deleteContinuous && Input.GetMouseButton(0))
        {
            TryDeleteContinuous();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (deleteContinuous)
            {
                CommitBuildBatch();
            }
            lastDeleteId = -1;
        }
    }
    void TryDeleteOne()
    {
        int id = GetBuildingAtMouse();

        if (id != 0)
        {
            RemoveBuilding(id);
        }
    }
    void TryDeleteContinuous()
    {
        int id = GetBuildingAtMouse();

        if (id != 0 && id != lastDeleteId)
        {
            if (!buildBatchOpen)
            {
                BeginBuildBatch();
            }

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

        return id;
    }
    public void RemoveBuilding(int instanceId)
    {
        // 1. Find building object
        if (!buildingMap.TryGetValue(instanceId, out var go))
            return;

        // 2. Find saved data
        var data = saveSystem.GetAll().Find(d => d.instanceId == instanceId);
        if (data == null) return;

        RegisterDeletedBuilding(data);

        RemoveBuildingInternal(instanceId);
    }

    private void RemoveBuildingInternal(int instanceId)
    {
        if (!buildingMap.TryGetValue(instanceId, out var go))
            return;

        var data = saveSystem.GetAll().Find(d => d.instanceId == instanceId);
        if (data == null) return;

        // ================= Delete Flow =================

        // 3. Remove scene object
        Destroy(go);
        buildingMap.Remove(instanceId);

        // 4. Clear grid occupancy
        grid.SetOccupied(
            new Vector2Int(data.x, data.y),
            data.width,
            data.height,
            false,
            0
        );

        // 5. Restore A*
        Vector3 world = grid.GridToWorld(data.x, data.y);
        world.x += (data.width - 1) * grid.cellSize * 0.5f;
        world.y += (data.height - 1) * grid.cellSize * 0.5f;

        Bounds bounds = new Bounds(
            world,
            new Vector3(data.width * grid.cellSize, data.height * grid.cellSize, 1)
        );

        var guo = new Pathfinding.GraphUpdateObject(bounds);
        guo.modifyWalkability = true;
        guo.setWalkability = true;

        AstarPath.active.UpdateGraphs(guo);

        // 6. Remove saved data
        saveSystem.RemoveBuilding(instanceId);

        // 7. Optional immediate save
        // saveSystem.Save();

        Debug.Log($"Remove building success: {instanceId}");
    }

    private void RestoreBuilding(BuildingData source)
    {
        if (source == null)
        {
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(source.prefabId);

        if (prefab == null)
        {
            Debug.LogError("Can not find prefab: " + source.prefabId);
            return;
        }

        BuildingData data = saveSystem.AddBuilding(source);

        Vector3 world = grid.GridToWorld(data.x, data.y);
        world.x += (data.width - 1) * grid.cellSize * 0.5f;
        world.y += (data.height - 1) * grid.cellSize * 0.5f;

        GameObject go = Instantiate(prefab);
        go.transform.position = world;
        RegisterBuilding(data.instanceId, go);

        grid.SetOccupied(
            new Vector2Int(data.x, data.y),
            data.width,
            data.height,
            true,
            data.instanceId
        );

        if (grass != null)
        {
            Vector2 center = new Vector2(world.x, world.y);
            Vector2 size = new Vector2(
                data.width * grid.cellSize,
                data.height * grid.cellSize
            );
            grass.AddBlock(center, size);
        }

        Bounds bounds = new Bounds(
            world,
            new Vector3(data.width * grid.cellSize, data.height * grid.cellSize, 1)
        );

        var guo = new Pathfinding.GraphUpdateObject(bounds);
        guo.modifyWalkability = true;
        guo.setWalkability = false;

        AstarPath.active.UpdateGraphs(guo);
    }

    private BuildingData CloneBuildingData(BuildingData source)
    {
        return new BuildingData()
        {
            instanceId = source.instanceId,
            prefabId = source.prefabId,
            prefabIntId = source.prefabIntId,
            x = source.x,
            y = source.y,
            width = source.width,
            height = source.height
        };
    }

    
}

