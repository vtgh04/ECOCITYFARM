using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private SettingsMenuController settingsController; 
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private bool _isPaused = false;

    private void Start()
    {
        if(pausePanel) pausePanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (!pausePanel)
        {
            Debug.LogError("❌ PausePanel is NULL!");
            return;
        }

        bool wasActive = pausePanel.activeSelf;
        bool newState = !wasActive;

        Debug.Log($"🟢 TogglePause: {wasActive} → {newState}");
        
        pausePanel.SetActive(newState);
        _isPaused = newState;

        if (_isPaused)
        {
            // CRITICAL: Bring to front and ensure Canvas is enabled
            pausePanel.transform.SetAsLastSibling();
            
            // Force refresh the Canvas
            Canvas canvas = pausePanel.GetComponent<Canvas>();
            if (canvas) canvas.enabled = true;
            
            Time.timeScale = 0f;
            Debug.Log("⏸ GAME PAUSED - Panel should be visible now");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("▶ GAME RESUMED");
            
            // Close Settings if it was opened from Pause Menu
            if (settingsController && settingsController.gameObject.activeSelf) 
            {
                settingsController.CloseSettings();
            }
        }
    }

    public void ResumeGame()
    {
        _isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        if (settingsController) settingsController.CloseSettings();
    }

    public void OpenSettings()
    {
        if (settingsController)
        {
            settingsController.ToggleSettings();
       
            settingsController.transform.SetAsLastSibling();
        }
    }

    public void SaveAndQuit()
    {
        Time.timeScale = 1f; 
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }

    
    public bool IsPauseOpen()
    {
        return pausePanel != null && pausePanel.activeSelf;
    }
}