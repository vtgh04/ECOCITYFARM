using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 
using TMPro;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Top HUD References")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("The 4 UI Managers")]
    [SerializeField] private MarketMenuController marketUI;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private ToolsMenuUI toolsUI;
    [SerializeField] private SettingsMenuController settingsUI; 
    [SerializeField] private PauseMenuController pauseMenu;

    private void Awake() { if (Instance == null) Instance = this; }

    private void Start()
    {
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(() => TimeManager.Instance.AdvanceDay());

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnMoneyChanged += UpdateMoney;
            UpdateMoney(PlayerWallet.Instance.CurrentMoney);
        }
    }

    private void Update()
    {
        if (clockText != null && TimeManager.Instance != null)
            clockText.text = TimeManager.Instance.GetFormattedTime();

        // Check Global ESC
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleGlobalEscape();
        }
    }

    private void HandleGlobalEscape()
    {
        Debug.Log("🔴 ESC Pressed!");

        // 1. ZOOM (Highest Priority)
        SimpleBuildingZoom zoom = Camera.main.GetComponent<SimpleBuildingZoom>();
        if (zoom != null && zoom.IsFocused)
        {
            Debug.Log("→ Step 1: Unfocusing Zoom");
            zoom.Unfocus();
            return;
        }

        // 2. PLACEMENT
        if (BuildingPlacementSystem.Instance != null && BuildingPlacementSystem.Instance.IsPlacingMode)
        {
            Debug.Log("→ Step 2: Canceling Placement");
            BuildingPlacementSystem.Instance.CancelPlacement();
            return;
        }

        // 3. TOOLS
        if (ToolManager.Instance != null && ToolManager.Instance.CurrentTool != ToolType.None)
        {
            Debug.Log("→ Step 3: Deselecting Tool");
            ToolManager.Instance.DeselectTool();
            return;
        }

        // 4. GENERAL UI (Market/Inventory/Tools/Settings)
        // Check each UI individually and only return if actually closed something
        
        if (marketUI != null && marketUI.IsMarketOpen())
        {
            Debug.Log("→ Step 4a: Closing Market");
            marketUI.CloseMarket();
            return;
        }

        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            Debug.Log("→ Step 4b: Closing Inventory");
            inventoryUI.CloseInventory();
            return;
        }

        if (toolsUI != null && toolsUI.IsToolsMenuOpen())
        {
            Debug.Log("→ Step 4c: Closing Tools Menu");
            toolsUI.CloseToolsMenu();
            return;
        }

        // Settings: Check both active AND has some visual indication it's open
        // (Sometimes Settings GameObject is active but panel is hidden)
        bool pauseMenuIsOpen = pauseMenu != null && pauseMenu.GetComponent<PauseMenuController>() != null 
                               && pauseMenu.GetComponent<PauseMenuController>().IsPauseOpen();
        
        if (settingsUI != null && settingsUI.gameObject.activeSelf)
        {
            // Only close if Settings has visible panel
            Transform settingsPanel = settingsUI.transform.Find("SettingsPanel"); // Adjust to your Settings panel name
            bool settingsIsVisible = settingsPanel != null && settingsPanel.gameObject.activeSelf;
            
            if (settingsIsVisible && !pauseMenuIsOpen)
            {
                Debug.Log("→ Step 4d: Closing Settings (standalone)");
                settingsUI.CloseSettings();
                return;
            }
        }
        
        Debug.Log("→ Step 4: No general UI to close, continuing...");

        // 5. BUILDING UI (PostOffice, StoreHouse)
        if (PostOfficeUI.Instance && PostOfficeUI.Instance.gameObject.activeSelf)
        {
            if (PostOfficeUI.Instance.IsPanelOpen()) 
            {
                Debug.Log("→ Step 5: Closing PostOffice");
                PostOfficeUI.Instance.ClosePanel();
                return;
            }
        }

        if (StoreHouseUI.Instance && StoreHouseUI.Instance.gameObject.activeSelf)
        {
            if (StoreHouseUI.Instance.IsPanelOpen())
            {
                Debug.Log("→ Step 5: Closing StoreHouse");
                StoreHouseUI.Instance.ClosePanel();
                return;
            }
        }

        // 6. PAUSE MENU (Lowest Priority)
        // Only toggle if nothing else was handled above
        Debug.Log("→ Step 6: Toggling Pause Menu");
        if (pauseMenu != null)
        {
            pauseMenu.TogglePause();
        }
        else
        {
            Debug.LogError("❌ PauseMenu is NULL!");
        }
    }

    private void UpdateMoney(int amount)
    {
        if (moneyText != null) moneyText.text = $"$ {amount}";
    }

    public bool CloseAllGeneralUI()
    {
        bool somethingWasClosed = false;

        // Add detailed logging for each UI state
        Debug.Log($"[CloseAllGeneralUI] Market: {(marketUI != null ? (marketUI.IsMarketOpen() ? "OPEN" : "closed") : "null")}");
        Debug.Log($"[CloseAllGeneralUI] Inventory: {(inventoryUI != null ? (inventoryUI.IsInventoryOpen() ? "OPEN" : "closed") : "null")}");
        Debug.Log($"[CloseAllGeneralUI] Tools: {(toolsUI != null ? (toolsUI.IsToolsMenuOpen() ? "OPEN" : "closed") : "null")}");
        Debug.Log($"[CloseAllGeneralUI] Settings: {(settingsUI != null ? (settingsUI.gameObject.activeSelf ? "ACTIVE" : "inactive") : "null")}");

        if (marketUI != null && marketUI.IsMarketOpen())
        {
            Debug.Log("Closing Market");
            marketUI.CloseMarket();
            somethingWasClosed = true;
        }

        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            Debug.Log("Closing Inventory");
            inventoryUI.CloseInventory();
            somethingWasClosed = true;
        }

        if (toolsUI != null && toolsUI.IsToolsMenuOpen())
        {
            Debug.Log("Closing Tools Menu");
            toolsUI.CloseToolsMenu();
            somethingWasClosed = true;
        }

        if (settingsUI != null && settingsUI.gameObject.activeSelf)
        {
            Debug.Log("Closing Settings");
            settingsUI.CloseSettings();
            somethingWasClosed = true;
        }

        Debug.Log($"[CloseAllGeneralUI] Result: {somethingWasClosed}");
        return somethingWasClosed;
    }
    
    private void CloseBuildings()
    {
        if(PostOfficeUI.Instance) PostOfficeUI.Instance.ClosePanel();
        if(StoreHouseUI.Instance) StoreHouseUI.Instance.ClosePanel();
    }

    // --- BUTTON FUNCTIONS ---
    public void OnClick_Market()
    {
        if (ToolManager.Instance != null) ToolManager.Instance.DeselectTool();
        CloseBuildings();
        if(inventoryUI) inventoryUI.CloseInventory();
        if(toolsUI) toolsUI.CloseToolsMenu();
        if(marketUI) marketUI.ToggleMarket();
    }

    public void OnClick_Inventory()
    {
        if (ToolManager.Instance != null) ToolManager.Instance.DeselectTool();
        CloseBuildings();
        if(marketUI) marketUI.CloseMarket();
        if(toolsUI) toolsUI.CloseToolsMenu();
        if(inventoryUI) inventoryUI.ToggleInventory();
    }

    public void OnClick_Tools()
    {
        if (ToolManager.Instance != null) ToolManager.Instance.DeselectTool();
        CloseBuildings();
        if(marketUI) marketUI.CloseMarket();
        if(inventoryUI) inventoryUI.CloseInventory();
        if(toolsUI) toolsUI.ToggleToolsMenu();
    }
}