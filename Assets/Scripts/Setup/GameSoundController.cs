using UnityEngine;

public class GameSoundController : MonoBehaviour
{
    public static GameSoundController Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    public AudioClip orderRefreshSound;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public AudioClip placeBuildingSound; // Hammer sound
    public AudioClip plantSeedSound;     // Dirt/Rustle sound
    public AudioClip harvestSound;       // Pop/Success sound

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. Load saved volumes
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // 2. Start Background Music
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // --- Volume Control ---
    public void SetMusicVolume(float volume)
    {
        if (musicSource) musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource) sfxSource.volume = volume;
    }

    // --- Play Specific Sounds ---
    public void PlayClick() => PlaySFX(clickSound);
    public void PlayHover() => PlaySFX(hoverSound);
    public void PlayPlaceBuilding() => PlaySFX(placeBuildingSound);
    public void PlayPlant() => PlaySFX(plantSeedSound);
    public void PlayHarvest() => PlaySFX(harvestSound);
    public void PlayOrderRefresh() => PlaySFX(orderRefreshSound);


    // Generic Play Function
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}