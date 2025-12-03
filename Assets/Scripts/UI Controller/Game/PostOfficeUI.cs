using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PostOfficeUI : MonoBehaviour
{
    public static PostOfficeUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform listContent;
    [SerializeField] private PostOfficeSlotUI rowTemplate; 

    [Header("Settings")]
    [SerializeField] private List<ItemData> possibleCrops; 
    [SerializeField] private int minOrders = 3;
    [SerializeField] private int maxOrders = 6;
    [SerializeField] private int minDaysToRefresh = 5;
    [SerializeField] private int maxDaysToRefresh = 10;

    private List<OrderData> _currentOrders = new List<OrderData>();
    private int _nextRefreshDay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (panel) panel.SetActive(false);
        if (rowTemplate) rowTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            GenerateNewOrders();
            SetNextRefreshDay();
            TimeManager.OnDayChanged += CheckForRefresh;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null) TimeManager.OnDayChanged -= CheckForRefresh;
    }

    private void Update()
    {
        if (panel != null && panel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
        }
    }

    // --- HÀM BẬT/TẮT PANEL (Đã cập nhật) ---
    public void TogglePanel()
    {
        if (panel != null)
        {
            bool isActive = panel.activeSelf;
            
            if (!isActive) // Nếu đang chuẩn bị MỞ lên
            {
                // 1. Đóng UI Chung (Market/Inventory...)
                if (HUDController.Instance != null) HUDController.Instance.CloseAllGeneralUI();

                // 2. Đóng StoreHouse (tránh 2 nhà mở cùng lúc)
                if (StoreHouseUI.Instance != null) StoreHouseUI.Instance.ClosePanel();

                RenderOrders();
            }

            panel.SetActive(!isActive);
        }
    }

    public void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    // ... (Các hàm Logic Game giữ nguyên: GenerateNewOrders, CheckForRefresh...) ...
    
     private void CheckForRefresh()
    {
        if (TimeManager.Instance.CurrentDay >= _nextRefreshDay)
        {
            GenerateNewOrders();
            SetNextRefreshDay();
            
            // If panel is open, refresh UI immediately
            if (panel.activeSelf) RenderOrders(); 

            // --- ADD SOUND TRIGGER HERE ---
            if (GameSoundController.Instance != null)
            {
                GameSoundController.Instance.PlayOrderRefresh();
                Debug.Log("🔔 Post Office Orders Updated! (Sound Played)");
            }
            // ------------------------------
        }
    }

    private void SetNextRefreshDay()
    {
        int daysToAdd = Random.Range(minDaysToRefresh, maxDaysToRefresh + 1);
        _nextRefreshDay = TimeManager.Instance.CurrentDay + daysToAdd;
    }

    private void GenerateNewOrders()
    {
        _currentOrders.Clear();
        if (possibleCrops.Count == 0) return;
        int orderCount = Random.Range(minOrders, maxOrders + 1);
        for (int i = 0; i < orderCount; i++)
        {
            ItemData randomCrop = possibleCrops[Random.Range(0, possibleCrops.Count)];
            int randomAmount = Random.Range(5, 21);
            _currentOrders.Add(new OrderData(randomCrop, randomAmount));
        }
    }

    private void RenderOrders()
    {
        foreach (Transform child in listContent)
            if (child.gameObject != rowTemplate.gameObject) Destroy(child.gameObject);

        foreach (var order in _currentOrders)
        {
            PostOfficeSlotUI newRow = Instantiate(rowTemplate, listContent);
            newRow.transform.localScale = Vector3.one;
            newRow.gameObject.SetActive(true);
            newRow.Setup(order.item, order.amount);
        }
    }
    public bool IsPanelOpen()
{
    return panel != null && panel.activeSelf;
}
    
}

[System.Serializable]
public class OrderData
{
    public ItemData item;
    public int amount;
    public OrderData(ItemData i, int a) { item = i; amount = a; }
}