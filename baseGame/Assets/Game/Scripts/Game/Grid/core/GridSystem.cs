using UnityEngine;

public class GridSystem
{
    public float cellSize = 1f;

    private GridCell[,] cells;

    public int Width => cells.GetLength(0);
    public int Height => cells.GetLength(1);

    // ⭐ 初始化
    public void Init(int width, int height, float cellSize)
    {
        this.cellSize = cellSize;

        cells = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new GridCell();
            }
        }
    }

    // ================= 坐标 =================

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(
            x * cellSize + cellSize * 0.5f,
            y * cellSize + cellSize * 0.5f,
            0
        );
    }

    // ================= 占用 =================

    public bool IsOccupied(int x, int y)
    {
        return cells[x, y].occupied;
    }

    public bool CanPlace(Vector2Int origin, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gx = origin.x + x;
                int gy = origin.y + y;

                if (!InBounds(gx, gy))
                    return false;

                if (cells[gx, gy].occupied)
                    return false;
            }
        }

        return true;
    }

    public void SetOccupied(Vector2Int origin, int width, int height, bool value, int buildingId)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gx = origin.x + x;
                int gy = origin.y + y;

                if (!InBounds(gx, gy)) continue;

                cells[gx, gy].occupied = value;
                cells[gx, gy].buildingId = value ? buildingId : 0;
            }
        }
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public int GetBuildingId(int x, int y)
    {
        if (!InBounds(x, y)) return 0;
        return cells[x, y].buildingId;
    }
}

// ⭐ 单元数据
public class GridCell
{
    public bool occupied;
    public int buildingId;
}