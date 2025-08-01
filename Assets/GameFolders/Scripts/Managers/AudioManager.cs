using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip[] sfxClips;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // PlayerPrefs keys
    private const string MUSIC_ENABLED_KEY = "MusicEnabled";
    private const string SFX_ENABLED_KEY = "SFXEnabled";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // State variables
    private bool isMusicEnabled = true;
    private bool isSFXEnabled = true;

    // SFX dictionary for easy access
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadAudioSettings();
        PlayBackgroundMusic();
    }

    private void InitializeAudio()
    {
        // Create AudioSources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Initialize SFX dictionary
        InitializeSFXDictionary();
    }

    private void InitializeSFXDictionary()
    {
        // Add SFX clips to dictionary with their names
        foreach (var clip in sfxClips)
        {
            if (clip != null && !sfxDictionary.ContainsKey(clip.name))
            {
                sfxDictionary.Add(clip.name, clip);
            }
        }
    }

    #region Music Controls

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && isMusicEnabled)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        SaveAudioSettings();

        if (isMusicEnabled)
        {
            PlayBackgroundMusic();
            Debug.Log("Music enabled");
        }
        else
        {
            StopBackgroundMusic();
            Debug.Log("Music disabled");
        }
    }

    #endregion

    #region SFX Controls

    public void PlaySFX(string clipName)
    {
        if (!isSFXEnabled) return;

        if (sfxDictionary.ContainsKey(clipName))
        {
            PlaySFX(sfxDictionary[clipName]);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!isSFXEnabled || clip == null) return;

        sfxSource.volume = sfxVolume * masterVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void ToggleSFX()
    {
        isSFXEnabled = !isSFXEnabled;
        SaveAudioSettings();

        Debug.Log(isSFXEnabled ? "SFX enabled" : "SFX disabled");
    }

    #endregion

    #region Volume Controls

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioSources();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveAudioSettings();
    }

    private void UpdateAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        // SFX volume is applied per-clip when playing
    }

    #endregion

    #region Save/Load Settings

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, isMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt(SFX_ENABLED_KEY, isSFXEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
        isSFXEnabled = PlayerPrefs.GetInt(SFX_ENABLED_KEY, 1) == 1;
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, sfxVolume);

        UpdateAudioSources();
    }

    #endregion

    #region Public Getters

    public bool IsMusicEnabled() => isMusicEnabled;
    public bool IsSFXEnabled() => isSFXEnabled;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    #endregion

    #region Button Methods for UI

    public void OnMusicButtonPressed()
    {
        ToggleMusic();
    }

    public void OnAudioButtonPressed()
    {
        ToggleSFX();
    }

    #endregion
}
