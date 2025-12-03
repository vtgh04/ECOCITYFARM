using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MarketMenuController : MonoBehaviour
{
    [Header("UI Containers")]
    [SerializeField] private GameObject subCategoryContainer; 
    [SerializeField] private GameObject itemHotbarContainer; // The Vertical Menu
    [SerializeField] private Transform itemContentArea;       
    [SerializeField] private Button itemButtonTemplate;       

    [Header("Button Positioning")]
    // Drag your 5 Sub-Buttons here in order (0=House, 1=Crop, etc.)
    [SerializeField] private List<RectTransform> categoryButtons; 
    [SerializeField] private float menuOffsetY = 10f; // Space between button and menu

    [Header("Data Lists")]
    [SerializeField] private List<BuildingData> houseList; 
    [SerializeField] private List<PlantData> cropList;
    [SerializeField] private List<BuildingData> treeList;
    [SerializeField] private List<BuildingData> decorList;
    [SerializeField] private List<BuildingData> pathList; 

    private bool _isMarketOpen = false;
    private int _activeCategoryIndex = -1;

    private void Start()
    {
        CloseMarket();
        if(itemButtonTemplate != null) itemButtonTemplate.gameObject.SetActive(false);
    }

    public void ToggleMarket()
    {
        _isMarketOpen = !_isMarketOpen;
        if (_isMarketOpen)
        {
            if (subCategoryContainer != null) subCategoryContainer.SetActive(true);
        }
        else
        {
            CloseMarket();
        }
    }

    public void CloseMarket()
    {
       _isMarketOpen = false;
        _activeCategoryIndex = -1;
        if (subCategoryContainer != null) subCategoryContainer.SetActive(false);
        if (itemHotbarContainer != null) itemHotbarContainer.SetActive(false);
        BuildingPlacementSystem.Instance.CancelPlacement();
    }

public void OnCategoryClicked(int categoryIndex)
    {
        // 1. Toggle Logic
        if (_activeCategoryIndex == categoryIndex)
        {
            if (itemHotbarContainer != null) itemHotbarContainer.SetActive(false);
            _activeCategoryIndex = -1; 
            return;
        }

        _activeCategoryIndex = categoryIndex;

        // 2. MOVE THE MENU
        if (itemHotbarContainer != null && categoryButtons != null && categoryIndex < categoryButtons.Count)
        {
            itemHotbarContainer.SetActive(true);

            RectTransform targetBtn = categoryButtons[categoryIndex];
            
            // CÁCH FIX: Đặt vị trí trùng khít với nút bấm trước
            itemHotbarContainer.transform.position = targetBtn.transform.position;

            // Sau đó nhích Y lên một đoạn cố định (thay vì tính toán phức tạp)
            // Thử số 120f (tương đương chiều cao cái nút)
            Vector3 fixedOffset = new Vector3(0, 50f, 0); 
            
            itemHotbarContainer.transform.position += fixedOffset;
        }
        
        ClearItems();

        // 3. Generate Items
        switch (categoryIndex)
        {
            case 0: GenerateButtons(houseList); break;
            case 1: GenerateButtons(cropList); break;
            case 2: GenerateButtons(treeList); break;
            case 3: GenerateButtons(decorList); break;
            case 4: GenerateButtons(pathList); break;
        }
    }

 private void GenerateButtons<T>(List<T> items) where T : ScriptableObject
    {
        foreach (var item in items)
        {
            if (item == null) continue;

            // 1. Create the Button
            Button btn = Instantiate(itemButtonTemplate, itemContentArea);
            btn.gameObject.SetActive(true);
            
            // 2. Setup UI Visuals
            Transform iconObj = btn.transform.Find("Img_Icon");
            Transform textObj = btn.transform.Find("Txt_Price");

            Image iconImage = (iconObj != null) ? iconObj.GetComponent<Image>() : null;
            TextMeshProUGUI priceText = (textObj != null) ? textObj.GetComponent<TextMeshProUGUI>() : null;

            // 3. Add Listeners with DEBUG LOGS
            if (item is BuildingData bData)
            {
                if (priceText) priceText.text = $"${bData.cost}";
                if (iconImage && bData.icon != null) iconImage.sprite = bData.icon;

                // --- DEBUG LOG ADDED HERE ---
                btn.onClick.AddListener(() => 
                {
                    Debug.Log($"[Market UI] Clicked Button for Building: {bData.buildingName}");
                    
                    if (BuildingPlacementSystem.Instance == null)
                    {
                        Debug.LogError("[Market UI] Critical Error: BuildingPlacementSystem Instance is NULL! Check GameManagers.");
                    }
                    else
                    {
                        BuildingPlacementSystem.Instance.SelectBuilding(bData);
                    }
                });
            }
            else if (item is PlantData pData)
            {
                if (priceText) priceText.text = $"${pData.buyPrice}";
                if (iconImage && pData.icon != null) iconImage.sprite = pData.icon;

                // --- DEBUG LOG ADDED HERE ---
                btn.onClick.AddListener(() => 
                {
                    Debug.Log($"[Market UI] Clicked Button for Plant: {pData.plantName}");
                    
                    if (BuildingPlacementSystem.Instance == null)
                    {
                        Debug.LogError("[Market UI] Critical Error: BuildingPlacementSystem Instance is NULL!");
                    }
                    else
                    {
                        BuildingPlacementSystem.Instance.SelectPlant(pData);
                    }
                });
            }
        }
    }    private void ClearItems()
    {
        foreach (Transform child in itemContentArea)
        {
            if (child.gameObject != itemButtonTemplate.gameObject)
                Destroy(child.gameObject);
        }
    }
    public bool IsMarketOpen() => _isMarketOpen;
    
}