using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    
    private string savePath;
    private string saveFileName = "farm_save.json";
    private bool _isDataLoaded = false; 

    private void Awake() 
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Đường dẫn chuẩn (Ổ C:/Users/.../AppData/LocalLow/...)
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log("📂 Save Path: " + savePath);
    }

    private void Start() 
    {
        int isNewGame = PlayerPrefs.GetInt("IsNewGame", 0); 

        if (isNewGame == 1)
        {
            Debug.Log("🔄 NEW GAME: Resetting Data...");
            // Xóa file save cũ đi để ép game load lại từ đầu (sạch sẽ hơn là copy đè)
            if (File.Exists(savePath)) File.Delete(savePath);
            
            // Tải dữ liệu mặc định từ Resources
            LoadDefaultSave();
            
            // Reset cờ
            PlayerPrefs.SetInt("IsNewGame", 0);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("▶️ CONTINUE GAME...");
            if (File.Exists(savePath))
            {
                LoadGame();
            }
            else
            {
                Debug.LogWarning("⚠️ Không có file save -> Tự tạo New Game");
                LoadDefaultSave();
            }
        }
    }

    private void OnApplicationQuit() => SaveGame();
    private void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGame(); }

    // --- HÀM LOAD GAME TỪ FILE SAVE ---
    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        try 
        {
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            ApplyDataToGame(data);
            _isDataLoaded = true; // Cho phép save
            Debug.Log("✅ Game Loaded Thành Công!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Lỗi Load Game: " + e.Message);
            // Nếu lỗi file save, load mặc định để cứu game
            LoadDefaultSave();
        }
    }

    // --- HÀM LOAD GAME TỪ RESOURCES (DEFAULT) ---
    private void LoadDefaultSave()
    {
        TextAsset defaultFile = Resources.Load<TextAsset>("default_save");
        if (defaultFile != null)
        {
            GameData data = JsonUtility.FromJson<GameData>(defaultFile.text);
            ApplyDataToGame(data);
            _isDataLoaded = true;
            Debug.Log("✅ Đã Load Default Save!");
            
            // Lưu ngay lập tức ra ổ đĩa để tạo file save
            SaveGame(); 
        }
        else
        {
            Debug.LogError("❌ KHÔNG TÌM THẤY file 'default_save' trong Resources!");
        }
    }

    // --- HÀM ÁP DỤNG DỮ LIỆU (Tách ra để dùng chung) ---
    private void ApplyDataToGame(GameData data)
    {
        if (data == null) return;
        ClearRuntimeObjects(); // Xóa sạch trước khi load

        // 1. Stats
        if (TimeManager.Instance) TimeManager.Instance.SetTimeData(data.time.day, data.time.dayProgress);
        if (PlayerWallet.Instance) PlayerWallet.Instance.SetMoney(data.player.money);
        
        // 2. Inventory
        if (InventoryManager.Instance) {
            InventoryManager.Instance.ClearInventory();
            InventoryManager.Instance.UpgradeCapacity(data.inventory.maxSlots);
            foreach (var item in data.inventory.items) {
                ItemData itemData = GameAssets.Instance.GetItem(item.itemName);
                if (itemData) InventoryManager.Instance.AddItem(itemData, item.quantity);
            }
        }

        // 3. Buildings
        foreach (var b in data.buildings) {
            BuildingData bData = GameAssets.Instance.GetBuilding(b.buildingID);
            if (bData) {
                GameObject obj = Instantiate(bData.buildingPrefab, b.position, Quaternion.Euler(0, b.rotationY, 0));
                
                // Setup Structure Data
                WorldStructure structure = obj.GetComponent<WorldStructure>();
                if (!structure) structure = obj.AddComponent<WorldStructure>();
                structure.buildingCost = b.cost;
                structure.size = bData.size;
                structure.originCoords = GridSystem.Instance.GetGridCoordinate(b.position);

                // Quan trọng: Đổi tên để xóa "(Clone)" đi cho đẹp và dễ quản lý
                obj.name = b.buildingID; 

                BuildingRegistry.Instance.RegisterBuilding(obj.GetInstanceID(), obj, structure.originCoords, structure.size);
                GridSystem.Instance.OccupyArea(structure.GetOccupiedCells());
            }
        }

        Physics.SyncTransforms(); // Cập nhật vật lý ngay lập tức

        // 4. Crops (SỬA LỖI CÂY KHÔNG LÊN)
        foreach (var c in data.crops) {
            PlantData pData = GameAssets.Instance.GetPlant(c.plantID);
            if (pData) {
                // Tìm ô đất
                Collider[] hits = Physics.OverlapSphere(c.position, 0.2f, LayerMask.GetMask("Ground"));
                foreach (var hit in hits) {
                    FarmlandPlot plot = hit.GetComponentInParent<FarmlandPlot>();
                    if (plot) {
                        // Gọi hàm load đặc biệt của Plot
                        plot.LoadCropState(pData, c.daysOld);
                        break;
                    }
                }
            }
        }
    }

    // --- HÀM SAVE ---
    public void SaveGame()
    {
        if (!_isDataLoaded) return;

        GameData data = new GameData();

        // 1. Stats
        if (TimeManager.Instance) { 
            data.time.day = TimeManager.Instance.CurrentDay; 
            data.time.dayProgress = TimeManager.Instance.DayProgress; 
        }
        if (PlayerWallet.Instance) data.player.money = PlayerWallet.Instance.CurrentMoney;

        // 2. Inventory
        if (InventoryManager.Instance) {
            data.inventory.maxSlots = InventoryManager.Instance.GetMaxSlots();
            foreach (var i in InventoryManager.Instance.GetCurrentInventory())
                if(i.Key) data.inventory.items.Add(new ItemSlotSave { itemName = i.Key.itemName, quantity = i.Value });
        }

        // 3. Buildings
        WorldStructure[] structures = FindObjectsOfType<WorldStructure>();
        foreach (var s in structures) {
            // Lưu tất cả object có script WorldStructure (trừ những cái gốc trong scene nếu có)
            // Cách tốt nhất là dựa vào BuildingRegistry hoặc check tên
            // Ở đây mình lưu tất cả những gì instantiate ra (thường có Clone hoặc được đặt tên lại)
            if (s.gameObject.scene.name == null) continue; // Bỏ qua prefab gốc

            // Lấy tên gốc từ Data (Cần BuildingData gắn trên object hoặc suy ra từ tên)
            // Giả sử tên object là "PostOffice" hoặc "PostOffice(Clone)"
            string cleanID = s.gameObject.name.Replace("(Clone)", "").Trim();
            
            data.buildings.Add(new BuildingSaveData {
                buildingID = cleanID,
                position = s.transform.position,
                rotationY = s.transform.rotation.eulerAngles.y,
                cost = s.buildingCost
            });
        }

        // 4. Crops
         data.crops.Clear(); // Xóa dữ liệu cũ trong list trước khi thêm mới
        FarmlandPlot[] plots = FindObjectsOfType<FarmlandPlot>();
        
        foreach (var p in plots) 
        {
            // Kiểm tra kỹ: Đất đã trồng VÀ Dữ liệu cây không được null
            if (p.IsPlanted && p.GetCurrentCrop() != null)
            {
                data.crops.Add(new CropSaveData 
                { 
                    plantID = p.GetCurrentCrop().plantName, // Lấy tên cây
                    position = p.transform.position,        // Lấy vị trí
                    daysOld = p.GetDaysOld()                // Lấy số ngày tuổi
                });
            }
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("✅ Game Saved!");
    }

    private void ClearRuntimeObjects()
    {
        // Xóa nhà (Tìm tất cả WorldStructure)
        foreach (var s in FindObjectsOfType<WorldStructure>()) {
            // Chỉ xóa những cái sinh ra (Clone) hoặc đã được load
            // Tránh xóa nhầm địa hình nếu địa hình lỡ gắn script này
            if (s.transform.parent == null) // Thường nhà nằm ngoài root
            {
                if(GridSystem.Instance) GridSystem.Instance.FreeArea(s.GetOccupiedCells());
                Destroy(s.gameObject);
            }
        }
        if (BuildingRegistry.Instance) BuildingRegistry.Instance.ClearRegistry();

        // Xóa cây
        foreach (var p in FindObjectsOfType<FarmlandPlot>()) p.ClearPlant();
    }
}