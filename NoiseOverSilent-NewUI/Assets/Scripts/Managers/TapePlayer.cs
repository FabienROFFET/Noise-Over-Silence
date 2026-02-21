// ============================================================
// PROJECT : Noise Over Silence
// FILE    : TapePlayer.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-21
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-21 - Initial version
// DESC    : Manages tape playback - 1980s style cassette player
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.Managers
{
    public class TapePlayer : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource tapeAudioSource;
        [SerializeField] private float volume = 0.3f;

        [Header("Tape Collection")]
        [SerializeField] private List<Tape> allTapes = new List<Tape>();

        private Tape currentTape;
        private int currentTapeIndex = -1;
        private bool isPlaying = false;

        private static TapePlayer instance;

        public static TapePlayer Instance => instance;
        public Tape CurrentTape => currentTape;
        public bool IsPlaying => isPlaying;
        public List<Tape> AllTapes => allTapes;

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

            SetupAudioSource();
            LoadTapeCollection();
        }

        private void SetupAudioSource()
        {
            if (tapeAudioSource == null)
            {
                tapeAudioSource = gameObject.AddComponent<AudioSource>();
                tapeAudioSource.loop = true;
                tapeAudioSource.playOnAwake = false;
                tapeAudioSource.volume = volume;
            }
        }

        private void LoadTapeCollection()
        {
            // Load from JSON or create default tapes
            string json = PlayerPrefs.GetString("TapeCollection", "");
            
            if (string.IsNullOrEmpty(json))
            {
                CreateDefaultTapes();
            }
            else
            {
                TapeCollection collection = JsonUtility.FromJson<TapeCollection>(json);
                allTapes = collection.tapes;
            }

            Debug.Log($"[TapePlayer v1.0] Loaded {allTapes.Count} tapes");
        }

        private void CreateDefaultTapes()
        {
            allTapes = new List<Tape>
            {
                new Tape
                {
                    id = "tape_01",
                    title = "Forgotten Melodies",
                    artist = "Unknown Artist",
                    description = "A haunting melody from before the silence",
                    audioFile = "Audio/Tapes/tape_01",
                    isUnlocked = true // First tape always available
                },
                new Tape
                {
                    id = "tape_02",
                    title = "Winter Dreams",
                    artist = "The Lost Ones",
                    description = "Echoes of a warmer past",
                    audioFile = "Audio/Tapes/tape_02",
                    isUnlocked = false
                },
                new Tape
                {
                    id = "tape_03",
                    title = "City Lights",
                    artist = "Brno Collective",
                    description = "When the city still glowed",
                    audioFile = "Audio/Tapes/tape_03",
                    isUnlocked = false
                }
            };

            SaveTapeCollection();
        }

        private void SaveTapeCollection()
        {
            TapeCollection collection = new TapeCollection { tapes = allTapes };
            string json = JsonUtility.ToJson(collection);
            PlayerPrefs.SetString("TapeCollection", json);
            PlayerPrefs.Save();
        }

        public void PlayTape(int index)
        {
            if (index < 0 || index >= allTapes.Count) return;
            
            Tape tape = allTapes[index];
            if (!tape.isUnlocked)
            {
                Debug.Log("[TapePlayer] Tape is locked!");
                return;
            }

            currentTapeIndex = index;
            currentTape = tape;

            // Load audio clip
            AudioClip clip = Resources.Load<AudioClip>(tape.audioFile);
            
            if (clip != null)
            {
                tapeAudioSource.clip = clip;
                tapeAudioSource.Play();
                isPlaying = true;
                Debug.Log($"[TapePlayer v1.0] Playing: {tape.title}");
            }
            else
            {
                Debug.LogWarning($"[TapePlayer] Audio not found: {tape.audioFile}");
            }
        }

        public void Stop()
        {
            if (tapeAudioSource != null)
            {
                tapeAudioSource.Stop();
                isPlaying = false;
                Debug.Log("[TapePlayer v1.0] Stopped");
            }
        }

        public void Pause()
        {
            if (tapeAudioSource != null && isPlaying)
            {
                tapeAudioSource.Pause();
                isPlaying = false;
            }
        }

        public void Resume()
        {
            if (tapeAudioSource != null && currentTape != null)
            {
                tapeAudioSource.UnPause();
                isPlaying = true;
            }
        }

        public void NextTape()
        {
            int nextIndex = (currentTapeIndex + 1) % allTapes.Count;
            
            // Skip locked tapes
            int attempts = 0;
            while (!allTapes[nextIndex].isUnlocked && attempts < allTapes.Count)
            {
                nextIndex = (nextIndex + 1) % allTapes.Count;
                attempts++;
            }

            if (allTapes[nextIndex].isUnlocked)
                PlayTape(nextIndex);
        }

        public void PreviousTape()
        {
            int prevIndex = currentTapeIndex - 1;
            if (prevIndex < 0) prevIndex = allTapes.Count - 1;

            // Skip locked tapes
            int attempts = 0;
            while (!allTapes[prevIndex].isUnlocked && attempts < allTapes.Count)
            {
                prevIndex--;
                if (prevIndex < 0) prevIndex = allTapes.Count - 1;
                attempts++;
            }

            if (allTapes[prevIndex].isUnlocked)
                PlayTape(prevIndex);
        }

        public void UnlockTape(string tapeId)
        {
            Tape tape = allTapes.Find(t => t.id == tapeId);
            if (tape != null && !tape.isUnlocked)
            {
                tape.isUnlocked = true;
                SaveTapeCollection();
                Debug.Log($"[TapePlayer v1.0] Unlocked tape: {tape.title}");
            }
        }

        public void SetVolume(float vol)
        {
            volume = Mathf.Clamp01(vol);
            if (tapeAudioSource != null)
                tapeAudioSource.volume = volume;
        }
    }
}
