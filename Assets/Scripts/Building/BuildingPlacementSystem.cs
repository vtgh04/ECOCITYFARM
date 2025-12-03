using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems; 

public class BuildingPlacementSystem : MonoBehaviour
{
    public static BuildingPlacementSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GridSystem _gridSystem;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Material _validMat, _invalidMat;
    
    [Header("Layer Settings")]
    [Tooltip("Layer của mặt đất lớn (Terrain/Plane) - Để đặt Nhà và Ô đất")]
    [SerializeField] private LayerMask terrainLayer; 

    [Tooltip("Layer của Ô Đất (FarmPlot) - Để gieo hạt giống")]
    [SerializeField] private LayerMask farmPlotLayer; 

    [Header("UI")]
    [SerializeField] private GameObject moneyPopupPrefab;

    private GameObject _previewObject;
    private BuildingData _selectedBuilding;
    private PlantData _selectedPlant;       
    private bool _isPlacingMode = false;
    private float _currentRotationY = 0f;

    public bool IsPlacingMode => _isPlacingMode;

    // Cache layer masks (static để reuse toàn app nếu cần) - loại bỏ readonly và khởi tạo ở Awake
    private static int BuildingLayerMask;
    private static int FarmPlotLayerMask;

    // Reuse array cho NonAlloc
    private Collider[] _overlapResults;

    private void Awake() 
    { 
        Instance = this; 
        _overlapResults = new Collider[64]; // Kích thước đủ cho hầu hết các trường hợp

        // Khởi tạo layer masks ở đây để tránh gọi ở initializer
        BuildingLayerMask = LayerMask.GetMask("Building");
        FarmPlotLayerMask = LayerMask.GetMask("Ground"); // Giả sử layer của FarmlandPlot là "FarmPlot" - điều chỉnh nếu khác
    }

    private void Update()
    {
        if (!_isPlacingMode) return;

        if (Mouse.current.rightButton.wasPressedThisFrame) { CancelPlacement(); return; }
        
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            _currentRotationY += 90f;
            if (_currentRotationY >= 360f) _currentRotationY = 0f;
        }

        UpdatePreview();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return; 
            TryExecutePlacement();
        }
    }

    public void SelectBuilding(BuildingData data) { CancelPlacement(); _selectedBuilding = data; _selectedPlant = null; StartPlacement(data.buildingPrefab); }
    public void SelectPlant(PlantData data) { CancelPlacement(); _selectedPlant = data; _selectedBuilding = null; StartPlacement(data.growthStagePrefabs[0]); }
    
    private void StartPlacement(GameObject prefab) 
    { 
        if(!prefab) return; 
        _isPlacingMode=true; _currentRotationY=0f; 
        _previewObject=Instantiate(prefab); 
        foreach(var c in _previewObject.GetComponentsInChildren<MonoBehaviour>()) Destroy(c); 
        foreach(var c in _previewObject.GetComponentsInChildren<Collider>()) Destroy(c); 
        _previewObject.transform.rotation=Quaternion.Euler(0,_currentRotationY,0); 
    }
    
    public void CancelPlacement() 
    { 
        _isPlacingMode=false; _selectedBuilding=null; _selectedPlant=null; _currentRotationY=0f; 
        if(_previewObject) Destroy(_previewObject); 
    }
    
    // --- LẤY LAYER MASK PHÙ HỢP ---
    private LayerMask GetTargetLayer()
    {
        // Nếu đang xây nhà hoặc xây ô đất -> Bắn vào Terrain
        if (_selectedBuilding != null) return terrainLayer;
        
        // Nếu đang trồng cây -> Bắn vào FarmPlot (Ground)
        if (_selectedPlant != null) return farmPlotLayer;

        return terrainLayer; // Mặc định
    }

    private void UpdatePreview()
    {
        if (_previewObject == null || !_isPlacingMode) return;
        
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        // Layer mask linh hoạt: Terrain cho building, FarmPlot cho plant
        LayerMask targetLayer = _selectedBuilding != null ? terrainLayer : farmPlotLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, targetLayer))
        {
            // Smooth rotation (giữ nguyên, nhưng clamp để tránh overflow)
            _previewObject.transform.rotation = Quaternion.Lerp(
                _previewObject.transform.rotation, 
                Quaternion.Euler(0, _currentRotationY, 0), 
                Time.deltaTime * 20f
            );
            
            // Tính size theo rotation (giữ nguyên)
            Vector2Int currentSize = (_selectedBuilding != null) 
                ? GetRotatedSize(_selectedBuilding.size) 
                : new Vector2Int(1, 1);
            
            // Grid snapping chính xác hơn: Dùng GetGridCoordinate thay vì Floor riêng
            Vector2Int gridPos = _gridSystem.GetGridCoordinate(hit.point);

            // World pos từ grid (chính xác, tránh lệch do hit.point)
            Vector3 snappedPos = _gridSystem.SnapToGrid(hit.point);

            // Điều chỉnh cho plant: Nếu plant, lấy trực tiếp từ hit (vì FarmPlot có vị trí riêng)
            if (_selectedPlant != null)
            {
                // Lấy pos từ FarmlandPlot để preview khớp chính xác
                FarmlandPlot plot = hit.collider.GetComponentInParent<FarmlandPlot>();
                if (plot != null && plot.plantMountPoint != null)
                {
                    snappedPos = plot.plantMountPoint.position;
                }
                else
                {
                    // Fallback nếu không hit plot (không nên xảy ra)
                    snappedPos.y += 0.1f;
                }
            }
            else
            {
                // Building: Áp dụng height offset
                snappedPos.y += _selectedBuilding?.heightOffset ?? 0f;
            }

            _previewObject.transform.position = snappedPos;
            
            // Validate và set material (gọi CheckValidity với gridPos chính xác)
            bool isValid = CheckValidity(hit, gridPos, currentSize);
            SetPreviewMaterial(isValid);
        }
        else
        {
            // Nếu không hit layer → Ẩn preview hoặc set invalid
            SetPreviewMaterial(false);
        }
    }

    private void TryExecutePlacement()
    {
        if (_previewObject == null || !_previewObject.activeSelf || !_isPlacingMode) return;
        
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        LayerMask targetLayer = _selectedBuilding != null ? terrainLayer : farmPlotLayer;
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, targetLayer))
        {
            Vector2Int currentSize = (_selectedBuilding != null) ? GetRotatedSize(_selectedBuilding.size) : new Vector2Int(1, 1);
            
            Vector2Int gridPos = _gridSystem.GetGridCoordinate(hit.point);
            
            if (!CheckValidity(hit, gridPos, currentSize)) return;

            Vector3 snappedPos = _gridSystem.SnapToGrid(hit.point);

            // Plant specific: Lấy pos từ plot
            FarmlandPlot plot = null;
            if (_selectedPlant != null)
            {
                plot = hit.collider.GetComponentInParent<FarmlandPlot>();
                if (plot == null) return;
                snappedPos = plot.plantMountPoint?.position ?? plot.transform.position; // Sử dụng property và fallback an toàn
            }
            else
            {
                snappedPos.y += _selectedBuilding.heightOffset;
            }

            // 1. PLACE BUILDING (Xây lên Terrain)
            if (_selectedBuilding != null)
            {
                snappedPos.y += _selectedBuilding.heightOffset;
                PlayerWallet.Instance.SpendMoney(_selectedBuilding.cost);
                SpawnMoneyPopup(snappedPos, -_selectedBuilding.cost);

                GameObject newObj = Instantiate(_selectedBuilding.buildingPrefab, snappedPos, Quaternion.Euler(0, _currentRotationY, 0));
                
                WorldStructure structure = newObj.GetComponent<WorldStructure>();
                if (structure == null) structure = newObj.AddComponent<WorldStructure>();
                structure.size = currentSize;
                structure.originCoords = gridPos;
                structure.buildingCost = _selectedBuilding.cost;

                BuildingRegistry.Instance.RegisterBuilding(newObj.GetInstanceID(), newObj, gridPos, currentSize);
                List<Vector2Int> occupiedArea = CalculateCells(gridPos, currentSize);
                _gridSystem.OccupyArea(occupiedArea);

                GameSoundController.Instance?.PlayPlaceBuilding();
            }
            // 2. PLANT SEED (Trồng lên FarmPlot)
            else if (_selectedPlant != null)
            {
                if (plot != null)
                {
                    PlayerWallet.Instance.SpendMoney(_selectedPlant.buyPrice);
                    SpawnMoneyPopup(plot.transform.position, -_selectedPlant.buyPrice);
                    plot.Plant(_selectedPlant);
                    
                    GameSoundController.Instance?.PlayPlant();
                }
            }
        }
    }

    private bool CheckValidity(RaycastHit hit, Vector2Int centerGridPos, Vector2Int currentSize)
    {
        // === 1. Kiểm tra nhanh điều kiện cơ bản (rất rẻ, không allocation) ===
        if ((_selectedBuilding == null && _selectedPlant == null) || 
            PlayerWallet.Instance.CurrentMoney < (_selectedBuilding?.cost ?? _selectedPlant.buyPrice))
            return false;

        // === 2. XỬ LÝ ĐẶT NHÀ ===
        if (_selectedBuilding != null)
        {
            // Unique building check (rất nhanh nếu BuildingRegistry dùng Dictionary)
            if (_selectedBuilding.isUnique && BuildingRegistry.Instance.IsBuildingExisting(_selectedBuilding.buildingName))
                return false;

            // Tính occupied cells (reuse list nếu có pool, nhưng tạm dùng new vì ít gọi)
            List<Vector2Int> occupiedCells = CalculateCells(centerGridPos, currentSize);

            // Kiểm tra lưới chiếm chỗ – nhanh với HashSet
            if (_gridSystem.IsAreaOccupied(occupiedCells))
                return false;

            // === Tối ưu OverlapBox để tránh overlap vật lý ===
            // - Shrink 0.85f để tránh rìa, height 5f để bao quát
            // - Nâng center lên để tránh chạm đất (dùng hit.point.y + height/2)
            const float BOX_SHRINK = 0.85f;
            const float BOX_HEIGHT = 5f; 
            

            Vector3 worldCenter = _gridSystem.SnapToGrid(hit.point) + new Vector3(0, BOX_HEIGHT * 0.5f, 0);

            Vector3 boxHalfExtents = new Vector3(
                currentSize.x * _gridSystem.GridSize * BOX_SHRINK * 0.5f,
                BOX_HEIGHT * 0.5f,
                currentSize.y * _gridSystem.GridSize * BOX_SHRINK * 0.5f);

            // NonAlloc để zero GC
            int hitCount = Physics.OverlapBoxNonAlloc(
                worldCenter, 
                boxHalfExtents, 
                _overlapResults, 
                Quaternion.identity, 
                BuildingLayerMask);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];
                // Bỏ qua preview và child của nó (nhanh, không allocation)
                if (col.gameObject == _previewObject || col.transform.IsChildOf(_previewObject.transform))
                    continue;

                // Nếu có collider khác (Building/Default) → invalid
                return false;
            }

            return true;
        }

        // === 3. XỬ LÝ TRỒNG CÂY (Plant) ===
        if (_selectedPlant != null)
        {
            // Raycast xuống để tìm chính xác FarmlandPlot (từ hit.point + up, khoảng cách ngắn)
            Vector3 rayOrigin = hit.point + Vector3.up * 3f; // Đủ cao tránh địa hình nhỏ

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit plotHit, 6f, FarmPlotLayerMask))
            {
                FarmlandPlot plot = plotHit.collider.GetComponentInParent<FarmlandPlot>();
                if (plot == null || plot.IsPlanted)
                    return false;

                // Check vật cản phía trên plot (Sphere nhỏ + NonAlloc)
                // Nâng center lên 0.5f để tránh va chạm với chính plot
                int blockerCount = Physics.OverlapSphereNonAlloc(
                    plot.transform.position + Vector3.up * 0.5f, 
                    0.3f,  // Radius nhỏ để chính xác
                    _overlapResults,
                    BuildingLayerMask);

                for (int i = 0; i < blockerCount; i++)
                {
                    var col = _overlapResults[i];
                    // Bỏ qua chính plot và child của nó
                    if (col.gameObject == plot.gameObject || col.transform.IsChildOf(plot.transform))
                        continue;

                    return false; // Có building che phía trên → invalid
                }

                return true;
            }
        }

        return false;
    }
    
    private List<Vector2Int> CalculateCells(Vector2Int centerGridPos, Vector2Int size) { List<Vector2Int> cells = new List<Vector2Int>(); int xOffset = (size.x - 1) / 2; int zOffset = (size.y - 1) / 2; for (int x = 0; x < size.x; x++) for (int y = 0; y < size.y; y++) cells.Add(new Vector2Int(centerGridPos.x - xOffset + x, centerGridPos.y - zOffset + y)); return cells; }
    private Vector2Int GetRotatedSize(Vector2Int originalSize) { return (Mathf.Abs(_currentRotationY % 180) > 1f) ? new Vector2Int(originalSize.y, originalSize.x) : originalSize; }
    private void SetPreviewMaterial(bool valid) { foreach (var r in _previewObject.GetComponentsInChildren<Renderer>()) r.material = valid ? _validMat : _invalidMat; }
    public void SpawnMoneyPopup(Vector3 pos, int amt) { if(moneyPopupPrefab) { var p = Instantiate(moneyPopupPrefab, pos + Vector3.up*2, Quaternion.identity).GetComponent<MoneyPopup>(); if(p) p.Setup(amt); } }
}