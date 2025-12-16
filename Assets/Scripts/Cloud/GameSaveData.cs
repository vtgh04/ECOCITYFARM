using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable data structure cho Cloud Save
/// Chứa tất cả game state cần lưu trữ
/// </summary>
[System.Serializable]
public class GameSaveData
{
    // --- PLAYER STATS ---
    public int currentMoney;
    public int maxInventorySlots;
    public int currentDay;
    public float currentTime;

    // --- INVENTORY DATA ---
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public int quantity;
    }
    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    // --- BUILDING DATA ---
    [System.Serializable]
    public class BuildingData
    {
        public int buildingID;
        public string buildingName;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;
        public int gridX, gridY;
        public int sizeX, sizeY;
        public float scaleX, scaleY, scaleZ;
    }
    public List<BuildingData> placedBuildings = new List<BuildingData>();

    // --- FARM PLOT DATA (Crops) ---
    [System.Serializable]
    public class FarmPlotData
    {
        public int plotID;
        public float posX, posY, posZ;
        public string cropName;
        public int currentGrowthStage;
        public int dayPlanted;
        public bool isHarvested;
    }
    public List<FarmPlotData> farmPlots = new List<FarmPlotData>();

    // --- METADATA ---
    public string playerName;
    public long lastSaveTimestamp;
    public string saveVersion = "1.0";

    public GameSaveData()
    {
        lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
