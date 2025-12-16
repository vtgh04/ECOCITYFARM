  // --- File: PlantData.cs ---
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Plant Data")]
public class PlantData : ScriptableObject
{
    public string plantName;
    public ItemData seedItem; 
    public int buyPrice;    
    
    public int daysToGrow;
    public GameObject[] growthStagePrefabs;
    public ItemData harvestedCropItem; 
    public int harvestYield = 1;
    public Sprite icon; 
}