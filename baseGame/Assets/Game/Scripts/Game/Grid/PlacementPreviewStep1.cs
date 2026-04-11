using UnityEngine;

public class PlacementPreviewStep1 : MonoBehaviour
{
    public GridSystem grid;
    public Transform preview;

    private Plane groundPlane;

    void Start()
    {
        // y=0 平面（没有Collider）
        groundPlane = new Plane(Vector3.forward, Vector3.zero);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector2Int gridPos = grid.WorldToGrid(hitPoint);
            Vector3 worldPos = grid.GridToWorld(gridPos.x, gridPos.y);

            preview.position = worldPos;
        }
    }
}