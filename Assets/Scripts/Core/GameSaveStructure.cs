using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public TimeData time;
    public PlayerData player;
    public InventoryData inventory;
    public List<BuildingSaveData> buildings;
    public List<CropSaveData> crops;

    public GameData()
    {
        time = new TimeData();
        player = new PlayerData();
        inventory = new InventoryData();
        buildings = new List<BuildingSaveData>();
        crops = new List<CropSaveData>();
    }
}

[System.Serializable]
public class TimeData
{
    public int day;
    public float dayProgress;
}

[System.Serializable]
public class PlayerData
{
    public int money;
}

[System.Serializable]
public class InventoryData
{
    public int maxSlots;
    public List<ItemSlotSave> items = new List<ItemSlotSave>();
}

[System.Serializable]
public struct ItemSlotSave
{
    public string itemName;
    public int quantity;
}

[System.Serializable]
public struct BuildingSaveData
{
    public string buildingID; // Matches BuildingData name
    public Vector3 position;
    public float rotationY;
    public int cost;
}

[System.Serializable]
public struct CropSaveData
{
    public string plantID; // Matches PlantData name
    public Vector3 position;
    public int daysOld;
}