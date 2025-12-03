using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Start()
    {
            if (settingsPanel) settingsPanel.SetActive(false);

        float savedMusic = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        if (musicSlider)
        {
            musicSlider.value = savedMusic;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider)
        {
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    // ... (ToggleSettings and CloseSettings remain the same) ...
    public void ToggleSettings()
    {
        // Copy your existing Toggle code here...
        if (settingsPanel) settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save();
        
        // --- LINK TO SOUND CONTROLLER ---
        if (GameSoundController.Instance != null)
            GameSoundController.Instance.SetMusicVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();

        // --- LINK TO SOUND CONTROLLER ---
        if (GameSoundController.Instance != null)
            GameSoundController.Instance.SetSFXVolume(value);
    }

    internal void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }
}