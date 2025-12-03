using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; // Add this if you use Shift+Click

public class InventoryUI : MonoBehaviour
{
    // --- ADD THIS SINGLETON PART ---
    public static InventoryUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    // -------------------------------

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform listContent;
    [SerializeField] private InventorySlotUI rowTemplate;

    private bool _isOpen = false;

    private void Start()
    {
        if(inventoryPanel) inventoryPanel.SetActive(false);
        if(rowTemplate) rowTemplate.gameObject.SetActive(false);
        InventoryManager.Instance.OnInventoryChanged += RefreshUI;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        if(inventoryPanel) inventoryPanel.SetActive(_isOpen);
        if (_isOpen) RefreshUI(InventoryManager.Instance.GetCurrentInventory());
    }

    public void CloseInventory()
    {
        _isOpen = false;
        if(inventoryPanel) inventoryPanel.SetActive(false);
    }

    private void RefreshUI(Dictionary<ItemData, int> inventory)
    {
        foreach (Transform child in listContent)
        {
            if (child.gameObject != rowTemplate.gameObject)
                Destroy(child.gameObject);
        }

        foreach (var itemPair in inventory)
        {
            InventorySlotUI newRow = Instantiate(rowTemplate, listContent);
            
            newRow.transform.localScale = Vector3.one; 
            newRow.transform.localPosition = new Vector3(newRow.transform.localPosition.x, newRow.transform.localPosition.y, 0);

            // Note: We use 'null' for the click action here because standard inventory 
            // might not do anything on click, or you can add a Consume function later.
            newRow.Setup(itemPair.Key, itemPair.Value); 
            
            newRow.gameObject.SetActive(true);
        }
    }
    public bool IsInventoryOpen() => inventoryPanel.activeSelf;

}