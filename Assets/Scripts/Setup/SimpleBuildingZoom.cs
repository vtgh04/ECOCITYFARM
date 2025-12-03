using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SimpleBuildingZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomDistance = 15f;
    [SerializeField] private float heightOffset = 5f;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Requirements")]
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private string targetTag = "Building";

    private Camera _cam;
    private SimCityCameraController _mainController;
    
    private bool _isFocused = false;
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Vector3 _originalPos;
    private Quaternion _originalRot;

    // Public property for HUDController to check
    public bool IsFocused => _isFocused;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _mainController = GetComponent<SimCityCameraController>();
    }

    private void Update()
    {
        // Check Click (Only if NOT busy)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            
            // Only allow Zoom if NOT placing buildings and NOT using tools
            if (!IsPlayerBusy()) 
            {
                CheckClick();
            }
        }

        // MOVEMENT LOGIC
        if (_isFocused)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // Return to original position logic
            if (_mainController != null && !_mainController.enabled)
            {
                if (Vector3.Distance(transform.position, _originalPos) > 0.1f)
                {
                    transform.position = Vector3.Lerp(transform.position, _originalPos, Time.deltaTime * transitionSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, _originalRot, Time.deltaTime * transitionSpeed);
                }
                else
                {
                    _mainController.enabled = true; // Re-enable WASD
                }
            }
        }
    }

    private bool IsPlayerBusy()
    {
        if (ToolManager.Instance != null && ToolManager.Instance.CurrentTool != ToolType.None) return true;
        if (BuildingPlacementSystem.Instance != null && BuildingPlacementSystem.Instance.IsPlacingMode) return true;
        return false;
    }

    private void CheckClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                FocusOnObject(hit.transform);
            }
        }
    }

    private void FocusOnObject(Transform target)
    {
        if (_isFocused) return;

        _originalPos = transform.position;
        _originalRot = transform.rotation;

        if (_mainController != null) _mainController.enabled = false;

        Vector3 direction = transform.forward; 
        Vector3 centerPoint = target.position + Vector3.up * heightOffset;
        
        _targetPos = centerPoint - (direction * zoomDistance);
        _targetRot = transform.rotation; 

        _isFocused = true;
    }

    // Public method for HUDController
    public void Unfocus()
    {
        if (!_isFocused) return;
        _isFocused = false; // This triggers the 'else' block in Update to return home
    }
        

}
                