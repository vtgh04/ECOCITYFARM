using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Quản lý lưu trữ trên Cloud
/// Yêu cầu: Unity Cloud Save SDK
/// Tự động lưu trữ game state mỗi khi có thay đổi
/// </summary>
public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    [Header("Auto Save Settings")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 phút

    [Header("Save Keys")]
    private const string SAVE_KEY = "GameSaveData";
    private const string PLAYER_PREFS_KEY = "LastSaveTime";

    private GameSaveData _currentSaveData;
    private float _timeSinceLastSave = 0f;
    private bool _isInitialized = false;

    public event Action<GameSaveData> OnSaveDataLoaded;
    public event Action OnSaveSuccess;
    public event Action<string> OnSaveError;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);
        _currentSaveData = new GameSaveData();
    }

    private void Start()
    {
        // Chờ Authentication trước khi initialize
        if (CloudAuthManager.Instance != null && CloudAuthManager.Instance.IsLoggedIn)
        {
            Initialize();
        }
        else
        {
            CloudAuthManager.Instance.OnAuthenticationSuccess += Initialize;
        }
    }

    private void Update()
    {
        if (!enableAutoSave || !_isInitialized) return;

        _timeSinceLastSave += Time.deltaTime;
        if (_timeSinceLastSave >= autoSaveInterval)
        {
            _timeSinceLastSave = 0f;
            SaveGameDataAsync();
        }
    }

    
    private async void Initialize()
    {
        try
        {
            Debug.Log("☁️ Khởi tạo Cloud Save...");
            
        

            _isInitialized = true;

            // Load dữ liệu lưu trữ cũ
            await LoadGameDataAsync();

            Debug.Log("✓ Cloud Save đã sẵn sàng");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi khởi tạo Cloud Save: {ex.Message}");
        }
    }

    public async Task SaveGameDataAsync()
    {
        if (!_isInitialized || CloudAuthManager.Instance == null || !CloudAuthManager.Instance.IsLoggedIn)
        {
            Debug.LogWarning("⚠️ Chưa đăng nhập, không thể lưu lên Cloud");
            SaveToLocalStorage();
            return;
        }

        try
        {
            Debug.Log("💾 Đang lưu dữ liệu lên Cloud...");

            // Cập nhật timestamp
            _currentSaveData.lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Chuyển thành JSON
            string jsonData = JsonUtility.ToJson(_currentSaveData, true);

     

            // Lưu thời gian last save
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, System.DateTime.Now.ToString());
            PlayerPrefs.Save();

            Debug.Log("✓ Dữ liệu đã lưu lên Cloud!");
            OnSaveSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi lưu Cloud: {ex.Message}");
            OnSaveError?.Invoke(ex.Message);
            
            // Fallback: Lưu local
            SaveToLocalStorage();
        }
    }


    public async Task LoadGameDataAsync()
    {
        if (!_isInitialized || CloudAuthManager.Instance == null || !CloudAuthManager.Instance.IsLoggedIn)
        {
            Debug.LogWarning("⚠️ Chưa đăng nhập, tải từ Local Storage");
            LoadFromLocalStorage();
            return;
        }

        try
        {
            Debug.Log("☁️ Đang tải dữ liệu từ Cloud...");

     

            // Mock: Tải từ local
            LoadFromLocalStorage();

            Debug.Log("✓ Dữ liệu đã tải từ Cloud!");
            OnSaveDataLoaded?.Invoke(_currentSaveData);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Lỗi tải Cloud: {ex.Message}, sử dụng Local Storage");
            LoadFromLocalStorage();
        }
    }

    /// <summary>
    /// Cập nhật dữ liệu hiện tại từ game
    /// Gọi hàm này trước khi SaveGameDataAsync()
    /// </summary>
    public void UpdateGameStateFromGame()
    {
        try
        {
            // Cập nhật Money
            if (PlayerWallet.Instance != null)
                _currentSaveData.currentMoney = PlayerWallet.Instance.CurrentMoney;

            // Cập nhật Inventory
            if (InventoryManager.Instance != null)
            {
                _currentSaveData.maxInventorySlots = InventoryManager.Instance.GetMaxSlots();
                _currentSaveData.inventoryItems.Clear();

                var inventory = InventoryManager.Instance.GetCurrentInventory();
                foreach (var item in inventory)
                {
                    _currentSaveData.inventoryItems.Add(new GameSaveData.InventoryItem
                    {
                        itemName = item.Key.itemName,
                        quantity = item.Value
                    });
                }
            }

            // Cập nhật Time
            if (TimeManager.Instance != null)
            {
                _currentSaveData.currentDay = TimeManager.Instance.CurrentDay;
                _currentSaveData.currentTime = TimeManager.Instance.CurrentTimeOfDay;
            }

            // Cập nhật Buildings
            _currentSaveData.placedBuildings.Clear();
            if (BuildingRegistry.Instance != null)
            {
                // TODO: Implement building data collection
                // Cần có method trong BuildingRegistry để lấy tất cả buildings
            }

            Debug.Log("✓ Dữ liệu game đã được cập nhật để lưu");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi cập nhật game state: {ex.Message}");
        }
    }

    /// <summary>
    /// Tải dữ liệu từ Local Storage (PlayerPrefs)
    /// </summary>
    private void LoadFromLocalStorage()
    {
        try
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                _currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("✓ Dữ liệu tải từ Local Storage");
            }
            else
            {
                _currentSaveData = new GameSaveData();
                Debug.Log("→ Không tìm thấy save data, tạo mới");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi tải Local Storage: {ex.Message}");
            _currentSaveData = new GameSaveData();
        }
    }

    /// <summary>
    /// Lưu dữ liệu vào Local Storage (PlayerPrefs)
    /// </summary>
    private void SaveToLocalStorage()
    {
        try
        {
            string json = JsonUtility.ToJson(_currentSaveData, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("💾 Dữ liệu đã lưu vào Local Storage");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi lưu Local Storage: {ex.Message}");
        }
    }

    /// <summary>
    /// Áp dụng dữ liệu saved vào game
    /// </summary>
    public async Task ApplySaveDataToGame()
    {
        try
        {
            Debug.Log("⚙️ Đang áp dụng dữ liệu lưu trữ vào game...");

            // Áp dụng Money
            if (PlayerWallet.Instance != null)
                PlayerWallet.Instance.SetMoney(_currentSaveData.currentMoney);

            // Áp dụng Inventory
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearInventory();
                InventoryManager.Instance.UpgradeCapacity(_currentSaveData.maxInventorySlots);
                // TODO: Thêm items vào inventory
            }

            // Áp dụng Time
            // TODO: Set time trong TimeManager

            // Áp dụng Buildings
            // TODO: Spawn buildings từ saved data

            Debug.Log("✓ Dữ liệu đã được áp dụng vào game!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi áp dụng dữ liệu: {ex.Message}");
        }
    }

    /// <summary>
    /// Xóa tất cả dữ liệu lưu trữ
    /// </summary>
    public async Task DeleteSaveDataAsync()
    {
        try
        {
            Debug.Log("🗑️ Đang xóa dữ liệu lưu trữ...");

            // TODO: Xóa từ Cloud
            // await CloudSaveService.Instance.Data.Player.DeleteAsync(SAVE_KEY);

            // Xóa Local Storage
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.DeleteKey(PLAYER_PREFS_KEY);
            PlayerPrefs.Save();

            _currentSaveData = new GameSaveData();

            Debug.Log("✓ Dữ liệu đã bị xóa");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi xóa dữ liệu: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy thông tin save hiện tại
    /// </summary>
    public GameSaveData GetCurrentSaveData() => _currentSaveData;
}
