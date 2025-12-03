using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Layer Settings")]
    [Tooltip("Layer for Houses, Fences, and Plants (The things you want to delete)")]
    [SerializeField] private LayerMask buildingLayer; 
    
    // We keep this for planting logic
    [SerializeField] private LayerMask groundLayer;

    private Camera _mainCamera;
    private const int REMOVE_COST = 200; 

    private void Start() => _mainCamera = Camera.main;

    private void Update()
    {
        // 1. Chặn click xuyên UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 2. Logic Tool (Harvest / Remove)
        if (ToolManager.Instance != null)
        {
            if (ToolManager.Instance.CurrentTool == ToolType.Harvest) 
                HandleHarvestInput();
            
            else if (ToolManager.Instance.CurrentTool == ToolType.Remove) 
                HandleRemoveInput_Final();
        }
        
        // 3. Logic Tương tác (Mở Panel nhà)
        if (ToolManager.Instance == null || ToolManager.Instance.CurrentTool == ToolType.None)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) CheckBuildingInteraction();
        }
    }

    // --- HÀM XÓA ĐÃ CẬP NHẬT LOGIC HÀNG RÀO ---
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

                // A. Kiểm tra xem có phải Công Trình (Nhà, Hàng Rào) không
                WorldStructure structure = hit.collider.GetComponentInParent<WorldStructure>();
                if (structure != null)
                {
                    // --- ĐOẠN MỚI: XỬ LÝ HÀNG RÀO ---
                    // Nếu vật bị xóa là Hàng Rào, báo cho hàng xóm update lại
                    FenceConnector fence = structure.GetComponent<FenceConnector>();
                    if (fence != null)
                    {
                        fence.ForceUpdateNeighbors();
                    }
                    // --------------------------------

                    // Giải phóng đất và Xóa
                    GridSystem.Instance.FreeArea(structure.GetOccupiedCells());
                    Destroy(structure.gameObject);
                    deletedSomething = true;
                }
                // B. Kiểm tra xem có phải Cây Trồng không
                else
                {
                    FarmlandPlot plot = hit.collider.GetComponentInParent<FarmlandPlot>();
                    if (plot != null && plot.IsPlanted)
                    {
                        plot.ClearPlant();
                        deletedSomething = true;
                    }
                }

                // C. Trừ tiền và Hiệu ứng
                if (deletedSomething)
                {
                    PlayerWallet.Instance.SpendMoney(REMOVE_COST);
                    
                    if (BuildingPlacementSystem.Instance)
                        BuildingPlacementSystem.Instance.SpawnMoneyPopup(hit.point, -REMOVE_COST);

                    // Thêm âm thanh xóa (nếu có)
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