// --- File: ItemData.cs (Updated with Prices) ---

using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("The name displayed for the item")]
    public string itemName;

    [Tooltip("The icon that represents this item in the UI")]
    public Sprite itemIcon;

    [Header("Economy")]
    [Tooltip("The price a player pays to BUY this item from the market")]
    public int buyPrice;

    [Tooltip("The price a player gets for SELLING this item to the market")]
    public int sellPrice;
}