using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// Quản lý Authentication với Unity Cloud
/// Yêu cầu: Unity Authentication SDK
/// </summary>
public class CloudAuthManager : MonoBehaviour
{
    public static CloudAuthManager Instance { get; private set; }

    [SerializeField] private bool isLoggedIn = false;
    public bool IsLoggedIn => isLoggedIn;

    public string CurrentPlayerID { get; private set; }
    public event Action OnAuthenticationSuccess;
    public event Action<string> OnAuthenticationError;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Kiểm tra nếu đã đăng nhập từ trước
        if (IsAlreadyAuthenticated())
        {
            Debug.Log("✓ Player đã được xác thực từ trước");
            isLoggedIn = true;
        }
        else
        {
            Debug.Log("→ Cần đăng nhập lần đầu");
        }
    }


    public async void SignInAnonymously()
    {
        try
        {
            Debug.Log("🔐 Đang đăng nhập ẩn danh...");
            
          

            // MOCK Implementation cho test
            CurrentPlayerID = "player_" + System.Guid.NewGuid().ToString();
            isLoggedIn = true;

            Debug.Log($"✓ Đăng nhập thành công! Player ID: {CurrentPlayerID}");
            OnAuthenticationSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi đăng nhập: {ex.Message}");
            OnAuthenticationError?.Invoke(ex.Message);
        }
    }


    public async void SignInWithProvider(string providerName)
    {
        try
        {
            Debug.Log($"🔐 Đang đăng nhập bằng {providerName}...");
            
        
            CurrentPlayerID = "player_" + System.Guid.NewGuid().ToString();
            isLoggedIn = true;

            Debug.Log($"✓ Đăng nhập {providerName} thành công!");
            OnAuthenticationSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi đăng nhập: {ex.Message}");
            OnAuthenticationError?.Invoke(ex.Message);
        }
    }

    public void SignOut()
    {
        try
        {
            // TODO: Gọi SignOut từ Authentication Service
            isLoggedIn = false;
            CurrentPlayerID = null;
            Debug.Log("✓ Đã đăng xuất");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi đăng xuất: {ex.Message}");
        }
    }

    private bool IsAlreadyAuthenticated()
    {
        return PlayerPrefs.HasKey("PlayerID");
    }

  
    public async Task<string> GetAccessToken()
    {
        try
        {
            return "mock_token_" + System.Guid.NewGuid().ToString();
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Lỗi lấy token: {ex.Message}");
            return null;
        }
    }
}
