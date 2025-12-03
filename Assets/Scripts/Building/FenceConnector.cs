using UnityEngine;

public class FenceConnector : MonoBehaviour
{
    [Header("Visual Parts")]
    [SerializeField] private GameObject connectorUp;    // Local Forward (Z+)
    [SerializeField] private GameObject connectorDown;  // Local Back (Z-)
    [SerializeField] private GameObject connectorLeft;  // Local Left (X-)
    [SerializeField] private GameObject connectorRight; // Local Right (X+)

    [Header("Settings")]
    [SerializeField] private LayerMask buildingLayer; 
    [SerializeField] private string fenceTag = "Decor"; 

    private void Start()
    {
        // Chạy ngay khi sinh ra
        UpdateConnections();
        
        // Báo cho hàng xóm update lại
        UpdateNeighbors();
    }

   public void UpdateNeighbors()
    {
        if (GridSystem.Instance == null) return;
        float gridSize = GridSystem.Instance.GridSize;
        
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        foreach (Vector3 dir in dirs)
        {
            // Tìm hàng xóm
            Collider[] hits = Physics.OverlapSphere(transform.position + (dir * gridSize), 0.4f, buildingLayer);
            foreach (var hit in hits)
            {
                FenceConnector neighbor = hit.GetComponentInParent<FenceConnector>();
                if (neighbor != null && neighbor != this) // Đừng tự update chính mình
                {
                    neighbor.UpdateConnections();
                }
            }
        }
    }

      public void UpdateConnections()
    {
        if (GridSystem.Instance == null) return;
        float gridSize = GridSystem.Instance.GridSize;

        // Check 4 hướng theo trục Local (để hỗ trợ xoay nếu cần)
        // Lưu ý: Nếu hàng rào bạn không xoay, dùng Vector3.forward thay vì transform.forward cũng được.
        bool up = IsFenceAt(transform.position + Vector3.forward * gridSize);
        bool down = IsFenceAt(transform.position + Vector3.back * gridSize);
        bool right = IsFenceAt(transform.position + Vector3.right * gridSize);
        bool left = IsFenceAt(transform.position + Vector3.left * gridSize);

        if (connectorUp) connectorUp.SetActive(up);
        if (connectorDown) connectorDown.SetActive(down);
        if (connectorLeft) connectorLeft.SetActive(left);
        if (connectorRight) connectorRight.SetActive(right);
    }
    private bool IsFenceAt(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, 0.4f, buildingLayer);
        foreach (var hit in hits)
        {
            // Chỉ kết nối nếu vật đó cũng là FenceConnector VÀ Collider của nó đang Bật
            FenceConnector fence = hit.GetComponentInParent<FenceConnector>();
            if (fence != null && hit.enabled) // hit.enabled cực quan trọng để fix lỗi xóa
            {
                return true;
            }
        }
        return false;
    }

    // Vẽ vòng tròn đỏ để bạn biết nó đang tìm ở đâu
    private void OnDrawGizmosSelected()
    {
        if (GridSystem.Instance == null) return;
        float s = GridSystem.Instance.GridSize;
        Gizmos.color = Color.red;
        
        // Vẽ theo hướng xoay của object
        Gizmos.DrawWireSphere(transform.position + transform.forward * s, 0.4f);
        Gizmos.DrawWireSphere(transform.position - transform.forward * s, 0.4f);
        Gizmos.DrawWireSphere(transform.position + transform.right * s, 0.4f);
        Gizmos.DrawWireSphere(transform.position - transform.right * s, 0.4f);
    }
     public void ForceUpdateNeighbors()
    {
        // Tắt Box Collider của chính mình đi trước
        // Để khi hàng xóm check Raycast, nó sẽ không thấy mình nữa -> Nó sẽ ngắt kết nối
        Collider myCollider = GetComponentInChildren<Collider>();
        if (myCollider) myCollider.enabled = false;

        UpdateNeighbors();
    }
}