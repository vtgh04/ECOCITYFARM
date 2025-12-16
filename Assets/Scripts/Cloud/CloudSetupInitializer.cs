using UnityEngine;


public class CloudSetupInitializer : MonoBehaviour
{
    [SerializeField] private bool enableCloudFeatures = true;
    [SerializeField] private bool debugMode = true;

    private void Awake()
    {
        if (!enableCloudFeatures)
        {
            Debug.LogWarning("☁️ Cloud Features bị tắt");
            return;
        }

 
        EnsureManagersExist();
    }

    private void Start()
    {
        Debug.Log("☁️ === CLOUD SAVE SYSTEM INITIALIZATION ===");

        // 1. Khởi tạo Authentication
        if (CloudAuthManager.Instance != null)
        {
            CloudAuthManager.Instance.OnAuthenticationSuccess += OnAuthSuccess;
            CloudAuthManager.Instance.OnAuthenticationError += OnAuthError;
            
            // Tự động đăng nhập lần đầu
            CloudAuthManager.Instance.SignInAnonymously();
        }
    }

    private void OnAuthSuccess()
    {
        Debug.Log("✓ Authentication thành công");

        // 2. Khởi tạo Cloud Save Manager
        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.OnSaveSuccess += () => 
            {
                if (debugMode) Debug.Log("✓ Save thành công");
                AnalyticsTracker.Instance?.TrackGameSaved("Cloud");
            };

            CloudSaveManager.Instance.OnSaveError += (error) => 
            {
                Debug.LogError($"✗ Save lỗi: {error}");
            };
        }

        // 3. Khởi tạo Analytics
        if (AnalyticsTracker.Instance != null)
        {
            AnalyticsTracker.Instance?.TrackGameStart("Player");
            Debug.Log("✓ Analytics đã khởi động");
        }

        Debug.Log("✓ === Tất cả Cloud Services sẵn sàng ===");
    }

    private void OnAuthError(string error)
    {
        Debug.LogError($"✗ Authentication lỗi: {error}");
        Debug.LogWarning("⚠️ Sử dụng Local Save thay thế");
    }

    private void EnsureManagersExist()
    {
        // Kiểm tra CloudAuthManager
        if (CloudAuthManager.Instance == null)
        {
            GameObject authGO = new GameObject("CloudAuthManager");
            authGO.AddComponent<CloudAuthManager>();
            Debug.Log("✓ Tạo CloudAuthManager");
        }

        // Kiểm tra CloudSaveManager
        if (CloudSaveManager.Instance == null)
        {
            GameObject saveGO = new GameObject("CloudSaveManager");
            saveGO.AddComponent<CloudSaveManager>();
            Debug.Log("✓ Tạo CloudSaveManager");
        }

        // Kiểm tra AnalyticsTracker
        if (AnalyticsTracker.Instance == null)
        {
            GameObject analyticsGO = new GameObject("AnalyticsTracker");
            analyticsGO.AddComponent<AnalyticsTracker>();
            Debug.Log("✓ Tạo AnalyticsTracker");
        }
    }

    private void OnDestroy()
    {
        if (CloudAuthManager.Instance != null)
        {
            CloudAuthManager.Instance.OnAuthenticationSuccess -= OnAuthSuccess;
            CloudAuthManager.Instance.OnAuthenticationError -= OnAuthError;
        }
    }
}
