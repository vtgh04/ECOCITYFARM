using UnityEngine;
using System.Collections.Generic;

public class WorldStructure : MonoBehaviour
{
    public Vector2Int originCoords;
    public Vector2Int size;
    
    [Tooltip("Giá trị lúc mua (để tính hoàn tiền)")]
    public int buildingCost = 0; 

    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        int xOffset = (size.x - 1) / 2;
        int zOffset = (size.y - 1) / 2;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                cells.Add(new Vector2Int(originCoords.x - xOffset + x, originCoords.y - zOffset + y));
            }
        }
        return cells;
    }
}