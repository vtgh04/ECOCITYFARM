using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    [Header("Grid Configuration")]
    [SerializeField] public float gridSize = 1f;
    
    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();
    
    [SerializeField] private List<Vector2Int> debugOccupiedList; 
     public Vector2Int GetGridCoordinate(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / gridSize);
        int z = Mathf.RoundToInt(worldPos.z / gridSize);
        return new Vector2Int(x, z);
    }
    public void OccupyArea(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (!_occupiedCells.Contains(cell))
            {
                _occupiedCells.Add(cell);
            }
        }
     
        debugOccupiedList = new List<Vector2Int>(_occupiedCells);
    }
    public float GridSize => gridSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }



    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        Vector2Int coords = GetGridCoordinate(worldPos);
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
            debugOccupiedList = new List<Vector2Int>(_occupiedCells);
            Debug.Log($"Occupied Grid Cell: {coords}");
        }
    }

    public void ResetGrid()
    {
        _occupiedCells.Clear();
        debugOccupiedList.Clear();
    }

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

  
  
     public void FreeArea(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (_occupiedCells.Contains(cell))
            {
                _occupiedCells.Remove(cell);
            }
        }
   
        debugOccupiedList = new List<Vector2Int>(_occupiedCells);
    }
}