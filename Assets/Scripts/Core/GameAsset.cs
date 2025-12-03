using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance { get; private set; }

    [Header("Drag ALL ScriptableObjects Here")]
    public List<ItemData> allItems;
    public List<BuildingData> allBuildings;
    public List<PlantData> allPlants;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public ItemData GetItem(string name) => allItems.FirstOrDefault(i => i.itemName == name);
    
    // Check both buildingName and name to be safe
    public BuildingData GetBuilding(string name) 
    {
        return allBuildings.FirstOrDefault(b => 
            b.buildingName == name ||           // Check internal ID
            b.name == name ||                   // Check file name
            b.buildingPrefab.name == name       // Check prefab name (Smartest)
        );
    }
    
    public PlantData GetPlant(string name) => allPlants.FirstOrDefault(p => p.plantName == name || p.name == name);
}