using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public event Action<Dictionary<ItemData, int>> OnInventoryChanged;

    [SerializeField] private int _maxInventorySlots = 50; 
    private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();
    private int _totalItemCount = 0;

    public int GetMaxSlots()
    {
    return _maxInventorySlots;
    }
    public void UpgradeCapacity(int newCapacity)
    {   
        _maxInventorySlots = newCapacity;
    Debug.Log($"Kho đã được nâng cấp lên {_maxInventorySlots} ô!");
    }
    private void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
    }

    public bool CheckCapacity(int amountToAdd)
    {
        return (_totalItemCount + amountToAdd) <= _maxInventorySlots;
    }

    public bool AddItem(ItemData item, int quantity)
    {
        if (!CheckCapacity(quantity)) return false;

        if (_inventory.ContainsKey(item))
            _inventory[item] += quantity;
        else
            _inventory.Add(item, quantity);

        _totalItemCount += quantity;
        OnInventoryChanged?.Invoke(_inventory);
        return true;
    }

    public bool RemoveItem(ItemData item, int quantity)
    {
        if (_inventory.ContainsKey(item) && _inventory[item] >= quantity)
        {
            _inventory[item] -= quantity;
            _totalItemCount -= quantity;
            
            if (_inventory[item] <= 0)
                _inventory.Remove(item);

            OnInventoryChanged?.Invoke(_inventory);
            return true;
        }
        return false;
    }
    
    public Dictionary<ItemData, int> GetCurrentInventory() => new Dictionary<ItemData, int>(_inventory);

    // --- BẠN ĐANG THIẾU ĐOẠN NÀY, HÃY COPY VÀO ---
    public int GetItemCount(ItemData item)
    {
        if (_inventory != null && _inventory.ContainsKey(item))
        {
            return _inventory[item];
        }
        return 0;
    }
     public void ClearInventory()
    {
        _inventory.Clear();
        _totalItemCount = 0;
        OnInventoryChanged?.Invoke(_inventory);
    }
    // ----------------------------------------------
}