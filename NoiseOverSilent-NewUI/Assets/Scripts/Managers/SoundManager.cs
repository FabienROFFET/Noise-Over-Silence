// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SoundManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-21
// VERSION : 2.0
// CHANGES : v2.0 - 2026-03-17 - Added PlaySoundscape() loading from
//                                StreamingAssets via UnityWebRequest.
//                                Added StopSoundscape().
//           v1.1 - 2026-02-21 - Voice over support
//           v1.0 - 2026-02-21 - Initial version
// DESC    : Manages all game audio — soundscapes, SFX, voice-overs, music.
// ============================================================

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace NoiseOverSilent.Managers
{
    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource soundscapeSource;  // NEW: per-event ambient
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceOverSource;

        [Header("Music (assign in Inspector or via Resources)")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip landingPageMusic;
        [SerializeField] private float musicVolume = 0.2f;

        [Header("UI Sounds (assign in Inspector or via Resources)")]
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip panelSlide;
        [SerializeField] private AudioClip typing;

        [Header("Volumes")]
        [SerializeField] private float sfxVolume       = 0.5f;
        [SerializeField] private float soundscapeVolume = 0.35f;

        private static SoundManager instance;

        // ── LIFECYCLE ──────────────────────────────────────────────────────
        private void Awake()
        {
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
            TryLoadDefaultClipsFromResources();
        }

        private void Start()
        {
            PlayLandingMusic();
        }

        private void SetupAudioSources()
        {
            musicSource      = EnsureSource("MusicSource",      loop: true,  vol: musicVolume);
            soundscapeSource = EnsureSource("SoundscapeSource", loop: true,  vol: soundscapeVolume);
            sfxSource        = EnsureSource("SFXSource",        loop: false, vol: sfxVolume);
            voiceOverSource  = EnsureSource("VOSource",         loop: false, vol: 0.8f);
        }

        private AudioSource EnsureSource(string label, bool loop, float vol)
        {
            GameObject go = new GameObject(label);
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.loop        = loop;
            src.playOnAwake = false;
            src.volume      = vol;
            return src;
        }

        // Try loading UI clips from Resources/Audio/UI/ so no manual wiring needed
        private void TryLoadDefaultClipsFromResources()
        {
            if (buttonHover  == null) buttonHover  = Resources.Load<AudioClip>("Audio/UI/ButtonHover");
            if (buttonClick  == null) buttonClick  = Resources.Load<AudioClip>("Audio/UI/ButtonClick");
            if (panelSlide   == null) panelSlide   = Resources.Load<AudioClip>("Audio/UI/PanelSlide");
            if (typing       == null) typing       = Resources.Load<AudioClip>("Audio/UI/Typing");
            if (backgroundMusic   == null) backgroundMusic   = Resources.Load<AudioClip>("Audio/Music/BackgroundMusic");
            if (landingPageMusic  == null) landingPageMusic  = Resources.Load<AudioClip>("Audio/Music/LandingMusic");
        }

        // ── SOUNDSCAPE (StreamingAssets) ───────────────────────────────────
        /// <summary>
        /// Loads and plays a soundscape from StreamingAssets.
        /// path is relative to StreamingAssets, e.g. "audio/Morning-Hum.mp3"
        /// </summary>
        public static void PlaySoundscape(string relativePath)
        {
            if (instance == null)
            {
                Debug.LogWarning("[SoundManager v2.0] No instance — cannot play soundscape.");
                return;
            }
            instance.StartCoroutine(instance.LoadSoundscapeCoroutine(relativePath));
        }

        public static void StopSoundscape()
        {
            if (instance != null && instance.soundscapeSource != null)
            {
                instance.soundscapeSource.Stop();
                instance.soundscapeSource.clip = null;
            }
        }

        private IEnumerator LoadSoundscapeCoroutine(string relativePath)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

            // Unity requires "file://" prefix on desktop; Android uses plain path
            string url = fullPath;
#if !UNITY_ANDROID || UNITY_EDITOR
            url = "file://" + fullPath;
#endif

            // Determine AudioType from extension
            AudioType audioType = AudioType.UNKNOWN;
            string ext = Path.GetExtension(relativePath).ToLower();
            if (ext == ".mp3")  audioType = AudioType.MPEG;
            else if (ext == ".wav")  audioType = AudioType.WAV;
            else if (ext == ".ogg")  audioType = AudioType.OGGVORBIS;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    soundscapeSource.clip   = clip;
                    soundscapeSource.loop   = true;
                    soundscapeSource.volume = soundscapeVolume;
                    soundscapeSource.Play();
                    Debug.Log($"[SoundManager v2.0] Soundscape playing: {relativePath}");
                }
                else
                {
                    Debug.LogWarning($"[SoundManager v2.0] Soundscape not found: {fullPath}\n{www.error}");
                }
            }
        }

        // ── VOICE OVER (StreamingAssets) ───────────────────────────────────
        public static void PlayVoiceOver(string relativePath)
        {
            if (instance == null) return;
            instance.StartCoroutine(instance.LoadVoiceOverCoroutine(relativePath));
        }

        private IEnumerator LoadVoiceOverCoroutine(string relativePath)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            string url = fullPath;
#if !UNITY_ANDROID || UNITY_EDITOR
            url = "file://" + fullPath;
#endif
            string ext = Path.GetExtension(relativePath).ToLower();
            AudioType audioType = ext == ".mp3" ? AudioType.MPEG
                                : ext == ".ogg" ? AudioType.OGGVORBIS
                                : AudioType.WAV;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    voiceOverSource.clip = DownloadHandlerAudioClip.GetContent(www);
                    voiceOverSource.Play();
                    Debug.Log($"[SoundManager v2.0] VoiceOver playing: {relativePath}");
                }
                else
                {
                    Debug.LogWarning($"[SoundManager v2.0] VoiceOver not found: {fullPath}");
                }
            }
        }

        public static void StopVoiceOver()
        {
            if (instance != null) instance.voiceOverSource?.Stop();
        }

        // ── MUSIC ──────────────────────────────────────────────────────────
        public static void PlayLandingMusic()
        {
            if (instance == null) return;
            AudioClip clip = instance.landingPageMusic ?? instance.backgroundMusic;
            if (clip != null)
            {
                instance.musicSource.clip = clip;
                instance.musicSource.Play();
                Debug.Log("[SoundManager v2.0] Landing music started.");
            }
        }

        public static void PlayGameMusic()
        {
            if (instance == null) return;
            if (instance.backgroundMusic != null)
            {
                instance.musicSource.clip = instance.backgroundMusic;
                instance.musicSource.Play();
                Debug.Log("[SoundManager v2.0] Game music started.");
            }
        }

        public static void StopMusic()
        {
            if (instance != null) instance.musicSource?.Stop();
        }

        // ── UI SFX ─────────────────────────────────────────────────────────
        public static void PlayButtonHover()
        {
            if (instance?.buttonHover != null)
                instance.sfxSource.PlayOneShot(instance.buttonHover);
        }

        public static void PlayButtonClick()
        {
            if (instance?.buttonClick != null)
                instance.sfxSource.PlayOneShot(instance.buttonClick);
        }

        public static void PlayPanelSlide()
        {
            if (instance?.panelSlide != null)
                instance.sfxSource.PlayOneShot(instance.panelSlide);
        }

        public static void PlayTyping()
        {
            if (instance?.typing != null)
                instance.sfxSource.PlayOneShot(instance.typing);
        }

        // ── VOLUME CONTROLS ────────────────────────────────────────────────
        public static void SetMusicVolume(float v)
        {
            if (instance?.musicSource != null)
                instance.musicSource.volume = Mathf.Clamp01(v);
        }

        public static void SetSFXVolume(float v)
        {
            if (instance?.sfxSource != null)
                instance.sfxSource.volume = Mathf.Clamp01(v);
        }

        public static void SetSoundscapeVolume(float v)
        {
            if (instance?.soundscapeSource != null)
                instance.soundscapeSource.volume = Mathf.Clamp01(v);
        }
    }
}