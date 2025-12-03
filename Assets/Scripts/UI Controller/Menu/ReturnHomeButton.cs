using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnHomeButton : MonoBehaviour
{
    [Tooltip("Tên chính xác của Scene Menu (Ví dụ: MainMenuScene)")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene"; 

    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        
        if (_btn != null)
        {
            _btn.onClick.AddListener(OnReturnHomeClicked);
        }
    }

    private void OnReturnHomeClicked()
    {
        // Có thể thêm âm thanh click ở đây nếu muốn
        Debug.Log("Returning to Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}