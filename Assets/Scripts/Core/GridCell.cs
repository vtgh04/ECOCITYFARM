using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GridCell - Đại diện cho một ô trong lưới
/// </summary>
public class GridCell
{
    public Vector2Int Position { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool IsOccupied { get; set; }
    public int? OccupantID { get; set; }
    public TerrainType TerrainType { get; set; }
    public int ResourceCount { get; set; }
    public List<GridCell> Neighbors { get; private set; }

    public GridCell(Vector2Int position, Vector3 worldPosition)
    {
        Position = position;
        WorldPosition = worldPosition;
        IsOccupied = false;
        TerrainType = TerrainType.Grass;
        ResourceCount = 0;
        OccupantID = null;
        Neighbors = new List<GridCell>();
    }

    public void Reset()
    {
        IsOccupied = false;
        TerrainType = TerrainType.Grass;
        ResourceCount = 0;
        OccupantID = null;
    }

    public int GetManhattanDistance(GridCell other)
    {
        return Mathf.Abs(Position.x - other.Position.x) + Mathf.Abs(Position.y - other.Position.y);
    }

    public float GetEuclideanDistance(GridCell other)
    {
        return Vector2.Distance(Position, other.Position);
    }
}

/// <summary>
/// Loại địa hình
/// </summary>
public enum TerrainType
{
    Grass = 0,
    Water = 1,
    Stone = 2,
    Forest = 3,
    Mountain = 4
}
