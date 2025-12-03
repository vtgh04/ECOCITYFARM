using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostOfficeSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText; // Displays "5/10"
    [SerializeField] private Button sellButton;
    [SerializeField] private Image sellButtonImage; // To change color (Green/Red)
    // [SerializeField] private TextMeshProUGUI rewardText; // To show Price

    private ItemData _requiredItem;
    private int _requiredAmount;
    private int _rewardMoney;

    // Call this to setup the row
    public void Setup(ItemData item, int amount)
    {
        _requiredItem = item;
        _requiredAmount = amount;
        
        // Calculate reward (e.g. 1.5x normal sell price as a bonus for doing the order)
        _rewardMoney = item.sellPrice * amount; 

        // Visuals
        if (itemIcon) itemIcon.sprite = item.itemIcon;
        // if (rewardText) rewardText.text = $"${_rewardMoney}";

        // Setup Button Click
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(OnSellClicked);

        RefreshStatus();
    }

    public void RefreshStatus()
    {
        // 1. Check Inventory
        int currentCount = InventoryManager.Instance.GetItemCount(_requiredItem);

        // 2. Update Text (e.g. "3 / 10")
        if (quantityText) quantityText.text = $"{currentCount} / {_requiredAmount}";

        // 3. Handle Button Color
        bool hasEnough = currentCount >= _requiredAmount;

        if (hasEnough)
        {
            sellButton.interactable = true;
            sellButtonImage.color = Color.green; // Ready to sell
        }
        else
        {
            sellButton.interactable = false; // Disable click if not enough
            sellButtonImage.color = Color.red;   // Not enough
        }
    }

    private void OnSellClicked()
    {
        // Double check inventory
        if (InventoryManager.Instance.GetItemCount(_requiredItem) >= _requiredAmount)
        {
            // 1. Remove Items
            InventoryManager.Instance.RemoveItem(_requiredItem, _requiredAmount);

            // 2. Add Money
            PlayerWallet.Instance.AddMoney(_rewardMoney);

            // 3. Notify PostOffice to remove this order from the list (Optional, or just refresh UI)
            // For now, let's just refresh this row to lock the button
            RefreshStatus();
            
            // Disable the row or show "Completed"
            gameObject.SetActive(false); 

            Debug.Log("Order Completed!");
        }
    }
}