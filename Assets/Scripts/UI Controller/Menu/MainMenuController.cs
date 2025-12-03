using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO; // Cần thư viện này để kiểm tra file save

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider loadingSlider;       
    [SerializeField] private GameObject buttonsContainer;
    
    // Thêm tham chiếu đến nút Continue để ẩn/hiện nếu chưa có file save
    [SerializeField] private Button continueButton; 

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Audio Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    // Đường dẫn file save (phải giống hệt bên SaveLoadManager)
    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // Thiết lập đường dẫn file save
        saveFilePath = Application.persistentDataPath + "/farm_save.json";
    }

    private void Start()
    {
        if (loadingSlider != null) 
        {
            loadingSlider.value = 0;
            loadingSlider.gameObject.SetActive(false);
        }
        
        if (buttonsContainer != null) buttonsContainer.SetActive(true);

        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }

        // --- KIỂM TRA FILE SAVE ---
        // Nếu có nút Continue, kiểm tra xem có file save không
        if (continueButton != null)
        {
            if (File.Exists(saveFilePath))
            {
                continueButton.interactable = true; // Có save -> Cho bấm
            }
            else
            {
                // Không có save -> Tắt nút Continue (hoặc ẩn đi)
                continueButton.interactable = false; 
                // Hoặc: continueButton.gameObject.SetActive(false);
            }
        }
    }

    // --- ÂM THANH ---
    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null) sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    // --- CHỨC NĂNG NÚT BẤM ---

    // 1. Nút CONTINUE (Chơi tiếp)
    public void OnContinueClicked()
    {
        PlayClickSound();
        // Đặt tín hiệu: 0 = Load Game cũ
        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();
        
        StartCoroutine(LoadGameSequence());
    }

    // 2. Nút NEW GAME (Chơi mới)
    public void OnNewGameClicked()
    {
        PlayClickSound();
        // Đặt tín hiệu: 1 = Tạo Game mới
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();

        StartCoroutine(LoadGameSequence());
    }

    public void OnExitClicked()
    {
        PlayClickSound();
        Debug.Log("Exiting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator LoadGameSequence()
    {
        if (loadingSlider != null)
        {
            loadingSlider.gameObject.SetActive(true);
            loadingSlider.value = 0;
        }

        float duration = 2.0f; // Giảm xuống 2s cho nhanh, 30s lâu quá :D
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            if (loadingSlider != null) loadingSlider.value = progress;
            yield return null;
        }

        if (loadingSlider != null) loadingSlider.value = 1;
        SceneManager.LoadScene(gameSceneName);
    }
}