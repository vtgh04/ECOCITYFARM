using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoreHouseUI : MonoBehaviour
{
    public static StoreHouseUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI txtSlotsInfo;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private Button btnUpgrade;

    private int[] _capacityLevels = { 50, 100, 150, 300, 500, 1000 };
    private int _costPerSlot = 20;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (panel) panel.SetActive(false);
        if (btnUpgrade) btnUpgrade.onClick.AddListener(OnUpgradeClicked);
    }

    // --- HÀM MỞ PANEL (Đã cập nhật) ---
    public void ShowPanel()
    {
        if (panel)
        {
            // 1. Đóng UI Chung
            if (HUDController.Instance != null) HUDController.Instance.CloseAllGeneralUI();

            // 2. Đóng PostOffice
            if (PostOfficeUI.Instance != null) PostOfficeUI.Instance.ClosePanel();

            panel.SetActive(true);
            RefreshUI();
        }
    }

    public void ClosePanel()
    {
        if (panel) panel.SetActive(false);
    }

    private void RefreshUI()
    {
        int currentMax = InventoryManager.Instance.GetMaxSlots();
        int nextLevelIndex = -1;

        for (int i = 0; i < _capacityLevels.Length; i++)
        {
            if (_capacityLevels[i] > currentMax)
            {
                nextLevelIndex = i;
                break;
            }
        }

        if (nextLevelIndex != -1)
        {
            int nextMax = _capacityLevels[nextLevelIndex];
            int slotsAdded = nextMax - currentMax;
            int cost = slotsAdded * _costPerSlot;

            txtSlotsInfo.text = $"{currentMax}  →  {nextMax} Slots";
            txtPrice.text = $"${cost}";
            
            bool enoughMoney = PlayerWallet.Instance.CurrentMoney >= cost;
            btnUpgrade.interactable = enoughMoney;
            txtPrice.color = enoughMoney ? Color.green : Color.red;
        }
        else
        {
            txtSlotsInfo.text = $"{currentMax} (MAX)";
            txtPrice.text = "---";
            btnUpgrade.interactable = false;
        }
    }

    private void OnUpgradeClicked()
    {
        int currentMax = InventoryManager.Instance.GetMaxSlots();
        int nextLevel = -1;
        foreach (int lvl in _capacityLevels) {
            if (lvl > currentMax) { nextLevel = lvl; break; }
        }

        if (nextLevel != -1)
        {
            int cost = (nextLevel - currentMax) * _costPerSlot;
            if (PlayerWallet.Instance.SpendMoney(cost))
            {
                InventoryManager.Instance.UpgradeCapacity(nextLevel);
                RefreshUI();
            }
        }
    }
    public bool IsPanelOpen()
{
    return panel != null && panel.activeSelf;
}
}