using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Layer Settings")]
    [Tooltip("Layer for Houses, Fences, and Plants (The things you want to delete)")]
    [SerializeField] private LayerMask buildingLayer; 
    
    [SerializeField] private LayerMask groundLayer;

    private Camera _mainCamera;
    private const int REMOVE_COST = 200; 

    private void Start() => _mainCamera = Camera.main;

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (ToolManager.Instance != null)
        {
            if (ToolManager.Instance.CurrentTool == ToolType.Harvest) 
                HandleHarvestInput();
            
            else if (ToolManager.Instance.CurrentTool == ToolType.Remove) 
                HandleRemoveInput_Final();
        }
        
        if (ToolManager.Instance == null || ToolManager.Instance.CurrentTool == ToolType.None)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) CheckBuildingInteraction();
        }
    }

   
    private void HandleRemoveInput_Final()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Kiểm tra tiền
            if (PlayerWallet.Instance.CurrentMoney < REMOVE_COST) return;

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
            {
                bool deletedSomething = false;

             
                FarmlandPlot plot = hit.collider.GetComponentInParent<FarmlandPlot>();
                if (plot != null && plot.IsPlanted)
                {
                 
                    plot.ClearPlant();
                    deletedSomething = true;
                }
          
                else
                {
                    WorldStructure structure = hit.collider.GetComponentInParent<WorldStructure>();
                    if (structure != null)
                    {
                        // B. Cập nhật hàng xóm nếu là Fence
                        FenceConnector fence = structure.GetComponent<FenceConnector>();
                        if (fence != null)
                        {
                            fence.ForceUpdateNeighbors();
                        }
                        // --------------------------------

                        GridSystem.Instance.FreeArea(structure.GetOccupiedCells());
                        
                        // Track building deletion to Cloud
                        string buildingName = structure.gameObject.name;
                        
                        Destroy(structure.gameObject);
                        
                        // After destruction, notify Cloud
                        CloudSaveIntegration.OnBuildingDeleted(buildingName);
                        deletedSomething = true;
                    }
                }

                // C. Trừ tiền và Hiệu ứng
                if (deletedSomething)
                {
                    PlayerWallet.Instance.SpendMoney(REMOVE_COST);
                    
                    if (BuildingPlacementSystem.Instance)
                        BuildingPlacementSystem.Instance.SpawnMoneyPopup(hit.point, -REMOVE_COST);

                 
                    if (GameSoundController.Instance) 
                        GameSoundController.Instance.PlayPlaceBuilding(); // Dùng tạm tiếng đặt nhà cho tiếng xóa
                }
            }
        }
    }

    private void CheckBuildingInteraction()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            PostOfficeClickable postOffice = hit.collider.GetComponentInParent<PostOfficeClickable>();
            if (postOffice != null) { postOffice.OnClick(); return; }

            StoreHouseClickable storeHouse = hit.collider.GetComponentInParent<StoreHouseClickable>();
            if (storeHouse != null) { storeHouse.OnClick(); return; }
        }
    }
    
    private void HandleHarvestInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            int mask = LayerMask.GetMask("Ground") | buildingLayer; 
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, mask))
            {
                FarmlandPlot plot = hit.collider.GetComponentInParent<FarmlandPlot>();
                if (plot != null) plot.TryHarvest();
            }
            
        }
        
    }
}