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
        
            FenceConnector fence = hit.GetComponentInParent<FenceConnector>();
            if (fence != null && hit.enabled) // hit.enabled cực quan trọng để fix lỗi xóa
            {
                return true;
            }
        }
        return false;
    }

   
    private void OnDrawGizmosSelected()
    {
        if (GridSystem.Instance == null) return;
        float s = GridSystem.Instance.GridSize;
        Gizmos.color = Color.red;
        
      
        Gizmos.DrawWireSphere(transform.position + transform.forward * s, 0.4f);
        Gizmos.DrawWireSphere(transform.position - transform.forward * s, 0.4f);
        Gizmos.DrawWireSphere(transform.position + transform.right * s, 0.4f);
        Gizmos.DrawWireSphere(transform.position - transform.right * s, 0.4f);
    }
     public void ForceUpdateNeighbors()
    {
        Collider myCollider = GetComponentInChildren<Collider>();
        if (myCollider) myCollider.enabled = false;

        UpdateNeighbors();
    }
}