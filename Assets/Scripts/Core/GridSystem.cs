using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    [Header("Grid Configuration")]
    [SerializeField] public float gridSize = 1f; // Your Cell Size (e.g. 1 or 2)
    
    // We use Vector2Int (X, Z) to ignore height differences!
    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();
    
    // Debug list to see occupied spots in Inspector (Optional)
    [SerializeField] private List<Vector2Int> debugOccupiedList; 

    public float GridSize => gridSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Converts a World Position (Float) to a Grid Coordinate (Int)
    /// Example: (10.5, 5, 10.2) -> (10, 10)
    /// </summary>
    public Vector2Int GetGridCoordinate(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / gridSize);
        int z = Mathf.RoundToInt(worldPos.z / gridSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Snaps a world point to the center of the nearest grid cell
    /// </summary>
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        Vector2Int coords = GetGridCoordinate(worldPos);
        // Keep the original Y, or reset to 0 depending on preference
        return new Vector3(coords.x * gridSize, worldPos.y, coords.y * gridSize);
    }

    public bool IsPositionOccupied(Vector3 worldPos)
    {
        Vector2Int coords = GetGridCoordinate(worldPos);
        return _occupiedCells.Contains(coords);
    }

    public void OccupyPosition(Vector3 worldPos)
    {
        Vector2Int coords = GetGridCoordinate(worldPos);
        if (!_occupiedCells.Contains(coords))
        {
            _occupiedCells.Add(coords);
            // Update debug list for Inspector visibility
            debugOccupiedList = new List<Vector2Int>(_occupiedCells);
            Debug.Log($"Occupied Grid Cell: {coords}");
        }
    }

    public void ResetGrid()
    {
        _occupiedCells.Clear();
        debugOccupiedList.Clear();
    }
// ... Existing code ...

    /// <summary>
    /// Checks if ANY cell in the list is already taken
    /// </summary>
    public bool IsAreaOccupied(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (_occupiedCells.Contains(cell))
            {
                return true; // Blocked!
            }
        }
        return false; // All clear
    }

    /// <summary>
    /// Marks a list of cells as taken
    /// </summary>
    public void OccupyArea(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (!_occupiedCells.Contains(cell))
            {
                _occupiedCells.Add(cell);
            }
        }
        // Update Debug list
        debugOccupiedList = new List<Vector2Int>(_occupiedCells);
    }
     public void FreeArea(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (_occupiedCells.Contains(cell))
            {
                _occupiedCells.Remove(cell);
            }
        }
        // Update debug list
        debugOccupiedList = new List<Vector2Int>(_occupiedCells);
    }
}