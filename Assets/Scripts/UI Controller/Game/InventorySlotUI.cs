using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("New Row Layout")]
    [SerializeField] private Image itemIcon;      // Link to Img_ItemIcon
    [SerializeField] private TextMeshProUGUI qtyText; // Link to Txt_Quantity

    public void Setup(ItemData item, int count)
    {
        // 1. Set Icon
        if (item.itemIcon != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemIcon.enabled = true;
            itemIcon.preserveAspect = true;
        }
        else
        {
            itemIcon.enabled = false;
        }

        // 2. Set Text (The Black Bar Text)
        if (qtyText != null)
        {
            qtyText.text = count.ToString("N0"); // N0 adds commas (e.g., 370,462)
        }
    }
}