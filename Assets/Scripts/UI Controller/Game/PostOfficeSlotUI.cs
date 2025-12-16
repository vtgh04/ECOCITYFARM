using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostOfficeSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText; 
    [SerializeField] private Button sellButton;
    [SerializeField] private Image sellButtonImage;
    

    private ItemData _requiredItem;
    private int _requiredAmount;
    private int _rewardMoney;

    
    public void Setup(ItemData item, int amount)
    {
        _requiredItem = item;
        _requiredAmount = amount;
        
        
        _rewardMoney = item.sellPrice * amount; 

       
        if (itemIcon) itemIcon.sprite = item.itemIcon;
      

    
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
            sellButtonImage.color = Color.green; 
        }
        else
        {
            sellButton.interactable = false; 
            sellButtonImage.color = Color.red;  
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