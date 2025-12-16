using UnityEngine;
using System;
using System.Collections.Generic;


public class AnalyticsTracker : MonoBehaviour
{
    public static AnalyticsTracker Instance { get; private set; }

    [SerializeField] private bool enableAnalytics = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (enableAnalytics)
        {
            InitializeAnalytics();
            SubscribeToGameEvents();
        }
    }

  
    private void InitializeAnalytics()
    {
        try
        {

            Debug.Log("📊 Analytics đã được khởi tạo");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi khởi tạo Analytics: {ex.Message}");
        }
    }

 
    private void SubscribeToGameEvents()
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnMoneyChanged += TrackMoneyChange;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += TrackInventoryChange;

        if (TimeManager.Instance != null)
            TimeManager.OnDayChanged += TrackDayChange;
    }

    // ==================== EVENT TRACKING ====================

 
    public void TrackGameStart(string playerName)
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "playerName", playerName },
                { "timestamp", System.DateTime.Now.ToString() },
                { "platform", Application.platform.ToString() }
            };

            SendAnalyticsEvent("GameStart", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking GameStart: {ex.Message}");
        }
    }

  
    public void TrackBuildingPlaced(string buildingName, int cost, Vector3 position)
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "buildingName", buildingName },
                { "cost", cost },
                { "posX", (int)position.x },
                { "posY", (int)position.y },
                { "posZ", (int)position.z }
            };

            SendAnalyticsEvent("BuildingPlaced", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking BuildingPlaced: {ex.Message}");
        }
    }

    /// <summary>
    /// Theo dõi khi xóa building
    /// </summary>
    public void TrackBuildingDeleted(string buildingName)
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "buildingName", buildingName }
            };

            SendAnalyticsEvent("BuildingDeleted", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking BuildingDeleted: {ex.Message}");
        }
    }

    /// <summary>
    /// Theo dõi khi thu hoạch cây
    /// </summary>
    public void TrackCropHarvested(string cropName, int quantity, int moneyEarned)
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "cropName", cropName },
                { "quantity", quantity },
                { "moneyEarned", moneyEarned }
            };

            SendAnalyticsEvent("CropHarvested", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking CropHarvested: {ex.Message}");
        }
    }

    /// <summary>
    /// Theo dõi khi mua hàng
    /// </summary>
    public void TrackPurchase(string itemName, int quantity, int cost)
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "itemName", itemName },
                { "quantity", quantity },
                { "cost", cost }
            };

            SendAnalyticsEvent("ItemPurchased", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking ItemPurchased: {ex.Message}");
        }
    }

    /// <summary>
    /// Theo dõi khi save game
    /// </summary>
    public void TrackGameSaved(string saveLocation)
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "saveLocation", saveLocation }, // "Cloud" hoặc "Local"
                { "timestamp", System.DateTime.Now.ToString() }
            };

            SendAnalyticsEvent("GameSaved", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking GameSaved: {ex.Message}");
        }
    }

    /// <summary>
    /// Theo dõi khi load game
    /// </summary>
    public void TrackGameLoaded()
    {
        if (!enableAnalytics) return;

        try
        {
            var eventData = new Dictionary<string, object>
            {
                { "timestamp", System.DateTime.Now.ToString() }
            };

            SendAnalyticsEvent("GameLoaded", eventData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tracking GameLoaded: {ex.Message}");
        }
    }

    // ==================== INTERNAL TRACKING (Auto) ====================

    private void TrackMoneyChange(int newAmount)
    {
        if (!enableAnalytics) return;

        var eventData = new Dictionary<string, object>
        {
            { "newAmount", newAmount }
        };

        SendAnalyticsEvent("MoneyChanged", eventData);
    }

    private void TrackInventoryChange(Dictionary<ItemData, int> inventory)
    {
        if (!enableAnalytics) return;

        var eventData = new Dictionary<string, object>
        {
            { "itemCount", inventory.Count }
        };

        SendAnalyticsEvent("InventoryChanged", eventData);
    }

    private void TrackDayChange()
    {
        if (!enableAnalytics) return;

        if (TimeManager.Instance != null)
        {
            var eventData = new Dictionary<string, object>
            {
                { "currentDay", TimeManager.Instance.CurrentDay }
            };

            SendAnalyticsEvent("DayChanged", eventData);
        }
    }

    // ==================== HELPER ====================

    /// <summary>
    /// Gửi sự kiện lên Analytics
    /// </summary>
    private void SendAnalyticsEvent(string eventName, Dictionary<string, object> eventData = null)
    {
        try
        {
            // TODO: Gửi event thực tế
            // AnalyticsService.Instance.RecordEvent(eventName, eventData);

            Debug.Log($"📊 Analytics Event: {eventName} - Data: {JsonUtility.ToJson(eventData ?? new Dictionary<string, object>())}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Không thể gửi event: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        if (!enableAnalytics) return;

        // Hủy subscribe
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnMoneyChanged -= TrackMoneyChange;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= TrackInventoryChange;

        if (TimeManager.Instance != null)
            TimeManager.OnDayChanged -= TrackDayChange;
    }
}
