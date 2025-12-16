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

        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log("📂 Save Path: " + savePath);
    }

    private void Start() 
    {
        int isNewGame = PlayerPrefs.GetInt("IsNewGame", 0); 

        if (isNewGame == 1)
        {
            Debug.Log("🔄 NEW GAME: Resetting Data...");
            if (File.Exists(savePath)) File.Delete(savePath);
            
            LoadDefaultSave();
            
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
            
            SaveGame(); 
        }
        else
        {
            Debug.LogError("❌ KHÔNG TÌM THẤY file 'default_save' trong Resources!");
        }
    }

    private void ApplyDataToGame(GameData data)
    {
        if (data == null) return;
        ClearRuntimeObjects(); 

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

                obj.name = b.buildingID; 

                BuildingRegistry.Instance.RegisterBuilding(obj.GetInstanceID(), obj, structure.originCoords, structure.size);
                GridSystem.Instance.OccupyArea(structure.GetOccupiedCells());
            }
        }

        Physics.SyncTransforms(); 
 
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
        
            if (s.gameObject.scene.name == null) continue; 

         
            string cleanID = s.gameObject.name.Replace("(Clone)", "").Trim();
            
            data.buildings.Add(new BuildingSaveData {
                buildingID = cleanID,
                position = s.transform.position,
                rotationY = s.transform.rotation.eulerAngles.y,
                cost = s.buildingCost
            });
        }

        // 4. Crops
         data.crops.Clear(); 
        FarmlandPlot[] plots = FindObjectsOfType<FarmlandPlot>();
        
        foreach (var p in plots) 
        {
            if (p.IsPlanted && p.GetCurrentCrop() != null)
            {
                data.crops.Add(new CropSaveData 
                { 
                    plantID = p.GetCurrentCrop().plantName, 
                    position = p.transform.position,       
                    daysOld = p.GetDaysOld()              
                });
            }
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("✅ Game Saved!");
    }

    private void ClearRuntimeObjects()
    {
  
        foreach (var s in FindObjectsOfType<WorldStructure>()) {
      
           
            if (s.transform.parent == null)
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