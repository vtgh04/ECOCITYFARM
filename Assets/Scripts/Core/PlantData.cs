  // --- File: PlantData.cs ---
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Plant Data")]
public class PlantData : ScriptableObject
{
    public string plantName;
    public ItemData seedItem; // The item returned if we dig it up (optional)
    public int buyPrice;    // Chỉnh lại cho giống với giá Seed Item
    
    public int daysToGrow;
    public GameObject[] growthStagePrefabs;
    public ItemData harvestedCropItem; // The item going into inventory
    public int harvestYield = 1;
    public Sprite icon; // <--- ADD THIS
}