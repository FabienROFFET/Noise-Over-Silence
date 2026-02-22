// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SoundManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-21
// VERSION : 1.1
// CHANGES : v1.0 - 2026-02-21 - Initial version
// DESC    : Manages all game audio - SFX and music
// ============================================================

using UnityEngine;

namespace NoiseOverSilent.Managers
{
    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceOverSource;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip landingPageMusic;
        [SerializeField] private float musicVolume = 0.2f;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip panelSlide;
        [SerializeField] private AudioClip typing;

        [Header("Settings")]
        [SerializeField] private float sfxVolume = 0.3f;

        private static SoundManager instance;

        private void Awake()
        {
            // Singleton pattern
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupAudioSources();
        }

        private void Start()
        {
            PlayMusic();
        }

        private void SetupAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (voiceOverSource == null)
            {
                voiceOverSource = gameObject.AddComponent<AudioSource>();
                voiceOverSource.loop = false;
                voiceOverSource.playOnAwake = false;
            }

            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;
            voiceOverSource.volume = 0.8f;
        }

        private void PlayMusic()
        {
            if (backgroundMusic != null && musicSource != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
                Debug.Log("[SoundManager v1.0] Background music started");
            }
        }

        // Public methods for playing sounds
        public static void PlayButtonHover()
        {
            if (instance != null && instance.buttonHover != null)
                instance.sfxSource.PlayOneShot(instance.buttonHover);
        }

        public static void PlayButtonClick()
        {
            if (instance != null && instance.buttonClick != null)
                instance.sfxSource.PlayOneShot(instance.buttonClick);
        }

        public static void PlayPanelSlide()
        {
            if (instance != null && instance.panelSlide != null)
                instance.sfxSource.PlayOneShot(instance.panelSlide);
        }

        public static void PlayTyping()
        {
            if (instance != null && instance.typing != null)
                instance.sfxSource.PlayOneShot(instance.typing);
        }

        // Volume controls
        public static void SetMusicVolume(float volume)
        {
            if (instance != null && instance.musicSource != null)
            {
                instance.musicSource.volume = Mathf.Clamp01(volume);
                instance.musicVolume = volume;
            }
        }

        public static void SetSFXVolume(float volume)
        {
            if (instance != null && instance.sfxSource != null)
            {
                instance.sfxSource.volume = Mathf.Clamp01(volume);
                instance.sfxVolume = volume;
            }
        }

        // Voice Over methods
        public static void PlayVoiceOver(string voPath)
        {
            Debug.Log($"[SoundManager v1.1] PlayVoiceOver called with: '{voPath}'");
            
            if (instance == null)
            {
                Debug.LogError("[SoundManager v1.1] Instance is NULL! Cannot play voice over.");
                return;
            }

            string fullPath = $"Audio/VoiceOvers/{voPath}";
            Debug.Log($"[SoundManager v1.1] Attempting to load from: Resources/{fullPath}");
            
            AudioClip voClip = Resources.Load<AudioClip>(fullPath);
            
            if (voClip != null)
            {
                Debug.Log($"[SoundManager v1.1] Voice over clip loaded successfully! Duration: {voClip.length}s");
                
                if (instance.voiceOverSource == null)
                {
                    Debug.LogError("[SoundManager v1.1] voiceOverSource is NULL!");
                    return;
                }
                
                instance.voiceOverSource.clip = voClip;
                instance.voiceOverSource.Play();
                Debug.Log($"[SoundManager v1.1] Playing voice over: {voPath}");
            }
            else
            {
                Debug.LogWarning($"[SoundManager v1.1] Voice over NOT FOUND at: Resources/{fullPath}");
                Debug.LogWarning($"[SoundManager v1.1] Make sure file exists: Assets/Resources/{fullPath}.mp3 (or .wav/.ogg)");
            }
        }

        public static void StopVoiceOver()
        {
            if (instance != null && instance.voiceOverSource != null)
            {
                instance.voiceOverSource.Stop();
            }
        }

        // Landing page music
        public static void PlayLandingMusic()
        {
            if (instance == null) return;

            if (instance.landingPageMusic != null && instance.musicSource != null)
            {
                instance.musicSource.clip = instance.landingPageMusic;
                instance.musicSource.Play();
                Debug.Log("[SoundManager v1.1] Landing page music started");
            }
        }

        public static void PlayGameMusic()
        {
            if (instance == null) return;

            if (instance.backgroundMusic != null && instance.musicSource != null)
            {
                instance.musicSource.clip = instance.backgroundMusic;
                instance.musicSource.Play();
                Debug.Log("[SoundManager v1.1] Game music started");
            }
        }
    }
}