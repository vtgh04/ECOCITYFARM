using UnityEngine;

public class FarmlandPlot : MonoBehaviour
{
    [SerializeField] public Transform plantMountPoint;
    
  
    [SerializeField] private PlantData _currentCrop; 
    [SerializeField] private int _daysOld;
    
    private GameObject _visual;
    public bool IsPlanted { get; private set; } = false;

    // --- HÀM CHO SAVE SYSTEM GỌI ---  
    public PlantData GetCurrentCrop()
    {
        return _currentCrop;
    }

    public int GetDaysOld()
    {
        return _daysOld;
    }
    // -------------------------------

    private void OnEnable()
    {
        if (TimeManager.Instance != null) TimeManager.OnDayChanged += Grow;
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null) TimeManager.OnDayChanged -= Grow;
    }

    public void Plant(PlantData crop)
    {
        _currentCrop = crop;
        _daysOld = 0;
        IsPlanted = true;
        UpdateVisual();
        
        Debug.Log($"Đã trồng: {crop.plantName}");
    }

    private void Grow()
    {
        if (!IsPlanted) return;
        _daysOld++;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_visual != null) Destroy(_visual);
        
        if (_currentCrop == null || _currentCrop.growthStagePrefabs == null) return;

        float growthPercent = (float)_daysOld / _currentCrop.daysToGrow;
        int stageIndex = Mathf.FloorToInt(growthPercent * _currentCrop.growthStagePrefabs.Length);
        stageIndex = Mathf.Clamp(stageIndex, 0, _currentCrop.growthStagePrefabs.Length - 1);

        if (plantMountPoint != null)
        {
            _visual = Instantiate(_currentCrop.growthStagePrefabs[stageIndex], plantMountPoint.position, Quaternion.identity, plantMountPoint);
            
           
            _visual.layer = LayerMask.NameToLayer("Building"); 
        }
    }

    public void TryHarvest()
    {
        if (!IsPlanted) return;

        if (_daysOld >= _currentCrop.daysToGrow)
        {
            if (InventoryManager.Instance.CheckCapacity(_currentCrop.harvestYield))
            {
                InventoryManager.Instance.AddItem(_currentCrop.harvestedCropItem, _currentCrop.harvestYield);
                if (GameSoundController.Instance) GameSoundController.Instance.PlayHarvest();
                
                // Cloud Analytics Tracking
                if (AnalyticsTracker.Instance != null)
                {
                    int rewardValue = _currentCrop.harvestedCropItem.sellPrice * _currentCrop.harvestYield;
                    AnalyticsTracker.Instance.TrackCropHarvested(_currentCrop.plantName, _currentCrop.harvestYield, rewardValue);
                }
                
                ClearPlant(); 
            }
            else
            {
                Debug.Log("Inventory Full!");
            }
        }
    }

    public void ClearPlant()
    {
        if (_visual != null) Destroy(_visual);
        if (plantMountPoint != null)
        {
            foreach(Transform child in plantMountPoint) Destroy(child.gameObject);
        }

        IsPlanted = false;
        _currentCrop = null;
        _daysOld = 0;
    }

    // --- HÀM CHO LOAD GAME GỌI ---
    public void LoadCropState(PlantData data, int days)
    {
        if (data == null) return;
        _currentCrop = data;
        _daysOld = days;
        IsPlanted = true;
        UpdateVisual(); 
    }
    
}