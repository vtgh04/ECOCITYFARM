using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public int cost;
    public GameObject buildingPrefab;
    public Sprite icon;
    
    [Header("Grid Settings")]
    public Vector2Int size = new Vector2Int(1, 1); 
    public bool isCropPlant;

    [Header("Unique Setting")]
    [Tooltip("Nếu tích vào, người chơi chỉ được phép đặt 1 cái duy nhất trên bản đồ.")]
    public bool isUnique = false; // <--- Tích vào đây cho Post Office Data

    [Header("Placement Settings")]
    public float heightOffset = 0.0f;
}