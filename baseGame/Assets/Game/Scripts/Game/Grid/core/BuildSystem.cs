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
    private bool isBuildMode = false;
    private bool isContinuous = false;

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
    }

    void Update()
    {
        if (isBuildMode)
        {
            preview.Tick();
            visualizer.Tick();
        }
    }

    // ================= 对外接口 =================

    public void StartBuild(GameObject prefab, int w, int h, bool continuous)
    {
        if (prefab == null) return;

        isContinuous = continuous;
        isBuildMode = true;

        preview.SetActive(true);
        preview.SetBuilding(prefab, w, h);

        visualizer.SetActive(true);
        UIManager.Instance.camera_scene.GetComponent<CameraController2D>().enabled = false;
    }

    public void StopBuild()
    {
        isBuildMode = false;
        preview.SetActive(false);
        visualizer.SetActive(false);
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
}