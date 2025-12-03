using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SimCityCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 40f;
    public float movementSmoothing = 10f;

    [Header("Height Control (Space/Shift)")]
    public float heightChangeSpeed = 20f;
    public float minHeight = 5f;
    public float maxHeight = 60f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f;
    public float pitchSensitivity = 1f;
    public float minPitch = 45f; // Đã tăng minPitch để tránh nhìn xuống void
    public float maxPitch = 85f;

    [Header("Zoom Settings")]
    public float zoomStepSize = 10f;
    public float minHeight_Zoom = 5f;
    public float maxHeight_Zoom = 60f;

    [Header("Boundary Object")]
    public Renderer boundaryRenderer; // Kéo thả đối tượng giới hạn (Plane/Mesh) vào đây
    public float boundaryPadding = 5f; // Khoảng đệm an toàn khỏi mép map

    [Header("Collision Detection")]
    public LayerMask buildingLayer;
    public float collisionCheckRadius = 0.5f;

    // Biến nội bộ cho ranh giới
    private Vector3 _worldMin;
    private Vector3 _worldMax;

    private Vector3 _targetPosition;
    private Vector3 _lastValidPosition;
    private Quaternion _targetRotation;
    
    // Internal variables to track rotation
    private float _pitch;
    private float _yaw;
    
    void Start()
    {
        _targetPosition = transform.position;
        _lastValidPosition = transform.position;
        _targetRotation = transform.rotation;

        // 1. Đồng bộ hóa góc quay hiện tại
        Vector3 angles = transform.eulerAngles;
        _pitch = angles.x;
        _yaw = angles.y;
        
        if (_pitch > 180) _pitch -= 360; 
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        
        // 2. Thiết lập ranh giới của đối tượng
        SetupBoundary();
    }

    private void SetupBoundary()
    {
        if (boundaryRenderer != null)
        {
            Bounds bounds = boundaryRenderer.bounds;
            
            _worldMin = bounds.min;
            _worldMax = bounds.max;
            
            // Giới hạn chỉ áp dụng cho mặt phẳng XZ
            _worldMin.y = float.MinValue; 
            _worldMax.y = float.MaxValue; 
        }
    }

    void Update()
    {
        HandleMovement();
        HandleHeightControl();
        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    private void HandleMovement()
    {
        Vector3 inputDir = Vector3.zero;

        // WASD Input
        if (Keyboard.current.wKey.isPressed) inputDir += transform.forward;
        if (Keyboard.current.sKey.isPressed) inputDir -= transform.forward;
        if (Keyboard.current.aKey.isPressed) inputDir -= transform.right;
        if (Keyboard.current.dKey.isPressed) inputDir += transform.right;

        // Keep movement flat (ignore Y)
        inputDir.y = 0;
        inputDir.Normalize();

        // Tốc độ di chuyển động (Giống game building: nhanh hơn khi bay cao)
        float currentHeight = transform.position.y;
        float heightRatio = Mathf.InverseLerp(minHeight, maxHeight, currentHeight);
        float dynamicSpeed = moveSpeed * Mathf.Lerp(0.8f, 1.5f, heightRatio); // Tốc độ tăng từ 80% đến 150%

        if (inputDir != Vector3.zero)
        {
            Vector3 potentialPos = _targetPosition + inputDir * dynamicSpeed * Time.deltaTime;
            
            potentialPos = ClampToBoundary(potentialPos); // Áp dụng giới hạn

            if (!IsPositionBlocked(potentialPos))
            {
                _targetPosition = potentialPos;
            }
        }
    }

    private void HandleHeightControl()
    {
        float heightInput = 0f;

        if (Keyboard.current.spaceKey.isPressed)
            heightInput = 1f;  
        else if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            heightInput = -1f; 

        if (heightInput != 0f)
        {
            Vector3 potentialPos = _targetPosition + Vector3.up * heightInput * heightChangeSpeed * Time.deltaTime;

            // Giới hạn chiều cao
            potentialPos.y = Mathf.Clamp(potentialPos.y, minHeight, maxHeight);

            potentialPos = ClampToBoundary(potentialPos); // Áp dụng giới hạn XZ

            if (!IsPositionBlocked(potentialPos))
            {
                _targetPosition = potentialPos;
            }
        }
    }

    private void HandleRotation()
    {
        // Right Click to Rotate
        if (Mouse.current.rightButton.isPressed && !IsPointerOverUI())
        {
            float deltaX = Mouse.current.delta.x.ReadValue() * rotationSpeed;
            float deltaY = Mouse.current.delta.y.ReadValue() * pitchSensitivity;
            
            _yaw += deltaX;
            _pitch -= deltaY;
            
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch); // Giới hạn góc nghiêng
            
            UpdateRotationFromAngles();
        }
    }

    private void UpdateRotationFromAngles()
    {
        _targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void HandleZoom()
    {
        if (IsPointerOverUI()) return;

        float scrollValue = Mouse.current.scroll.y.ReadValue();

        if (scrollValue != 0)
        {
            Vector3 zoomDir = transform.forward;
            Vector3 moveAmount = zoomDir * (scrollValue > 0 ? 1 : -1) * zoomStepSize;
            Vector3 potentialPos = _targetPosition + moveAmount;

            // Giới hạn chiều cao khi zoom
            if (potentialPos.y < minHeight_Zoom || potentialPos.y > maxHeight_Zoom)
            {
                return; 
            }

            potentialPos = ClampToBoundary(potentialPos); // Áp dụng giới hạn XZ

            if (!IsPositionBlocked(potentialPos))
            {
                _targetPosition = potentialPos;
            }
        }
        if (scrollValue > 0) scrollValue = 1;
        if (scrollValue < 0) scrollValue = -1;
    }

    private void UpdateCameraPosition()
    {
        // Smooth transition
        Vector3 newPos = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * movementSmoothing);
        
        // Kiểm tra vị trí mới
        if (!IsPositionBlocked(newPos))
        {
            _lastValidPosition = newPos;
        }
        
        transform.position = _lastValidPosition;
        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * movementSmoothing);
    }

    private Vector3 ClampToBoundary(Vector3 position)
    {
        if (boundaryRenderer == null) return position;

        // Áp dụng Padding: Min + Padding, Max - Padding
        position.x = Mathf.Clamp(position.x, _worldMin.x + boundaryPadding, _worldMax.x - boundaryPadding);
        position.z = Mathf.Clamp(position.z, _worldMin.z + boundaryPadding, _worldMax.z - boundaryPadding);
        
        return position;
    }

    private bool IsPositionBlocked(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, collisionCheckRadius, buildingLayer);
        return colliders.Length > 0;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    // Các hàm phụ trợ
    public void FocusOnTarget(Transform target)
    {
        if (!target) return;

        Vector3 offset = new Vector3(-10f, 15f, -10f); 
        _targetPosition = target.position + offset;
        
        Vector3 direction = target.position - _targetPosition;
        _targetRotation = Quaternion.LookRotation(direction);
        
        Vector3 angles = _targetRotation.eulerAngles;
        _pitch = angles.x;
        _yaw = angles.y;
    }

    public void ResetRotation()
    {
        _pitch = 45f;
        _yaw = 0f;
        UpdateRotationFromAngles();
    }
}