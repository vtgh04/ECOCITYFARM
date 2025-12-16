using UnityEngine;

public static class CloudSaveIntegration
{

      public static void OnBuildingPlaced(string buildingName, int cost, Vector3 position)
    {
        AnalyticsTracker.Instance?.TrackBuildingPlaced(buildingName, cost, position);

        TriggerAutoSave();
    }


    public static void OnBuildingDeleted(string buildingName)
    {
       
        AnalyticsTracker.Instance?.TrackBuildingDeleted(buildingName);

    
        TriggerAutoSave();
    }

 
    public static void OnCropHarvested(string cropName, int quantity, int moneyEarned)
    {
     
        AnalyticsTracker.Instance?.TrackCropHarvested(cropName, quantity, moneyEarned);

       
        TriggerAutoSave();
    }

   
    public static void OnItemPurchased(string itemName, int quantity, int cost)
    {
        // Track Analytics
        AnalyticsTracker.Instance?.TrackPurchase(itemName, quantity, cost);

        // Trigger autosave
        TriggerAutoSave();
    }

  
    public static async void ManualSaveGame()
    {
        if (CloudSaveManager.Instance == null) return;

        Debug.Log("💾 Lưu game...");
        CloudSaveManager.Instance.UpdateGameStateFromGame();
        await CloudSaveManager.Instance.SaveGameDataAsync();
    }

    public static async void ManualLoadGame()
    {
        if (CloudSaveManager.Instance == null) return;

        Debug.Log("📂 Tải game...");
        await CloudSaveManager.Instance.LoadGameDataAsync();
        await CloudSaveManager.Instance.ApplySaveDataToGame();
        AnalyticsTracker.Instance?.TrackGameLoaded();
    }


    public static async void DeleteAllSaveData()
    {
        if (CloudSaveManager.Instance == null) return;

        Debug.Log("🗑️ Xóa tất cả dữ liệu...");
        await CloudSaveManager.Instance.DeleteSaveDataAsync();
    }

    // ==================== INTERNAL ====================

    private static float _lastAutoSaveTime = 0f;
    private const float AUTO_SAVE_COOLDOWN = 10f; // Tránh save quá thường xuyên

    private static void TriggerAutoSave()
    {
        if (Time.time - _lastAutoSaveTime < AUTO_SAVE_COOLDOWN) return;

        _lastAutoSaveTime = Time.time;

        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.UpdateGameStateFromGame();
        }
    }
}
