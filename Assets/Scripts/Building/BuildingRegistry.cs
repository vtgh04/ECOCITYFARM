using UnityEngine;
using System.Collections.Generic;

public class BuildingRegistry : MonoBehaviour
{
    private static BuildingRegistry _instance;
     public static BuildingRegistry Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<BuildingRegistry>();
            return _instance;
        }
    }

    private Dictionary<int, BuildingPlacementInfo> _placedBuildings = new Dictionary<int, BuildingPlacementInfo>();

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

   public void RegisterBuilding(int buildingID, GameObject building, Vector2Int gridPos, Vector2Int size)
    {
        BuildingPlacementInfo info = new BuildingPlacementInfo
        {
            buildingID = buildingID,
            buildingObject = building,
            gridPosition = gridPos,
            size = size
        };
        
        if (_placedBuildings.ContainsKey(buildingID))
            _placedBuildings[buildingID] = info;
        else
            _placedBuildings.Add(buildingID, info);
    }

 public void UnregisterBuilding(int buildingID)
    {
        if (_placedBuildings.ContainsKey(buildingID))
        {
            _placedBuildings.Remove(buildingID);
        }
    }

    public void ClearRegistry()
    {
        _placedBuildings.Clear();
    }

    // --- FIX: BETTER UNIQUE CHECK ---
     public bool IsBuildingExisting(string nameToCheck)
    {
        string cleanCheck = nameToCheck.Replace(" ", "").ToLower();

        foreach (var info in _placedBuildings.Values)
        {
            if (info.buildingObject == null) continue;

            string existingName = info.buildingObject.name.Replace("(Clone)", "").Replace(" ", "").ToLower();

            if (existingName.Contains(cleanCheck) || cleanCheck.Contains(existingName))
            {
                return true; 
            }
        }
        return false;
    }


       private void Start()
    {
        // Find every building already sitting in the scene
        WorldStructure[] existingStructures = FindObjectsOfType<WorldStructure>();

        foreach (var structure in existingStructures)
        {
            // Register them immediately
            RegisterBuilding(
                structure.gameObject.GetInstanceID(), 
                structure.gameObject, 
                structure.originCoords, 
                structure.size
            );
            
            // Debug.Log($"[Registry] Found existing building: {structure.gameObject.name}");
        }
    }
}

public struct BuildingPlacementInfo
{
    public int buildingID;
    public GameObject buildingObject;
    public Vector2Int gridPosition;
    public Vector2Int size;
}