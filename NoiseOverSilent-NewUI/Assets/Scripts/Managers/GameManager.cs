// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-14
// VERSION : 1.7
// CHANGES : v1.4 - 2026-02-22 - Multi-language support
//           v1.3 - 2026-02-22 - Voice over playback
//           v1.2 - 2026-02-16 - Soundscape handling
//           v1.1 - 2026-02-16 - Added debug logs for event flow
//           v1.0 - 2026-02-14 - Initial version
// ============================================================

using System.Collections;
using UnityEngine;
using NoiseOverSilent.Core;
using NoiseOverSilent.Data;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("References — wired by UIBuilder")]
        [SerializeField] private JsonLoader jsonLoader;
        [SerializeField] private ImageDisplay imageDisplay;
        [SerializeField] private SlidingPanel slidingPanel;
        [SerializeField] private ChapterIntroScreen chapterIntroScreen;

        [Header("Start Settings")]
        [SerializeField] private int startEpisode  = 1; // Used when saving game
        [SerializeField] private string currentLanguage = "en"; // Default: English

        private EpisodeData currentEpisode;

        private void Start()
        {
            // Load language preference from PlayerPrefs
            currentLanguage = PlayerPrefs.GetString("Language", "en");
            
            // Game now starts when PLAY button is clicked from landing page
            // No longer auto-starts here
            Debug.Log($"[GameManager v1.7] Ready. Language: {currentLanguage}. Waiting for landing page...");
        }

        // ══════════════════════════════════════════════════════════════════
        // SAVE / LOAD SYSTEM
        // ══════════════════════════════════════════════════════════════════

        public void SaveGame()
        {
            if (currentEpisode == null)
            {
                Debug.LogWarning("[GameManager v1.7] Cannot save - no episode loaded");
                return;
            }

            // Get current event text for preview
            GameEvent currentEvent = currentEpisode.events.Find(e => e.id == lastShownEventId);
            string previewText = currentEvent != null ? currentEvent.text : "Unknown location";

            SaveLoadSystem.SaveGame(startEpisode, lastShownEventId, currentLanguage, previewText);
        }

        public void LoadSavedGame()
        {
            SaveData data = SaveLoadSystem.LoadGame();
            if (data == null)
            {
                Debug.LogWarning("[GameManager v1.7] No save data found");
                return;
            }

            // Set language from save
            currentLanguage = data.language;
            PlayerPrefs.SetString("Language", currentLanguage);

            // Load the saved episode and event
            LoadEpisode(data.currentEpisode, data.currentEvent);
        }

        private int lastShownEventId = 1; // Track last shown event for saving

        public void LoadEpisode(int episodeNumber, int eventId = 1)
        {
            currentEpisode = jsonLoader.LoadEpisode(episodeNumber, currentLanguage);

            if (currentEpisode != null)
            {
                // Show chapter intro if available, then show first event
                if (currentEpisode.chapter != null && chapterIntroScreen != null && eventId == 1)
                {
                    chapterIntroScreen.ShowChapterIntro(currentEpisode.chapter, () => {
                        StartCoroutine(ShowEventNextFrame(eventId));
                    });
                }
                else
                {
                    // No chapter intro, go straight to event
                    StartCoroutine(ShowEventNextFrame(eventId));
                }
            }
            else
                Debug.LogError($"[GameManager v1.7] Failed to load episode {episodeNumber} in language '{currentLanguage}'.");
        }

        public void SetLanguage(string language)
        {
            currentLanguage = language;
            PlayerPrefs.SetString("Language", language);
            PlayerPrefs.Save();
            Debug.Log($"[GameManager v1.7] Language changed to: {language}");
        }

        private IEnumerator ShowEventNextFrame(int eventId)
        {
            yield return null;
            ShowEvent(eventId);
        }

        public void ShowEvent(int eventId)
        {
            if (currentEpisode == null) return;

            lastShownEventId = eventId; // Track for saving

            GameEvent gameEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (gameEvent == null)
            {
                Debug.LogError($"[GameManager v1.7] Event {eventId} not found.");
                return;
            }

            Debug.Log($"[GameManager v1.7] Showing event {eventId}: '{gameEvent.text?.Substring(0, System.Math.Min(40, gameEvent.text?.Length ?? 0))}...'");

            // Handle soundscape/background music
            if (!string.IsNullOrEmpty(gameEvent.soundscape_mp3))
            {
                Managers.SoundManager.SetMusicVolume(0.15f); // Lower background music
                Debug.Log($"[GameManager v1.7] Soundscape: {gameEvent.soundscape_mp3}");
            }

            // Play voice over if available
            if (!string.IsNullOrEmpty(gameEvent.voice_over))
            {
                Debug.Log($"[GameManager v1.7] Voice over field found: '{gameEvent.voice_over}'");
                Managers.SoundManager.PlayVoiceOver(gameEvent.voice_over);
            }
            else
            {
                Debug.Log($"[GameManager v1.7] No voice over for event {eventId}");
            }

            if (imageDisplay != null)
                imageDisplay.DisplayImage(gameEvent.image_link);

            if (slidingPanel != null)
                slidingPanel.ShowEvent(gameEvent);
        }

        public void MakeChoice(Choice choice)
        {
            Debug.Log($"[GameManager v1.2] MakeChoice called: nextEventId={choice.next_event}");

            // Handle tape unlocking
            if (!string.IsNullOrEmpty(choice.unlock_tape))
            {
                Managers.TapePlayer tapePlayer = Managers.TapePlayer.Instance;
                if (tapePlayer != null)
                {
                    tapePlayer.UnlockTape(choice.unlock_tape);
                    Debug.Log($"[GameManager v1.2] Unlocked tape: {choice.unlock_tape}");
                }
            }

            int nextEventId = choice.next_event;

            if (nextEventId <= 0)
            {
                Debug.Log("[GameManager v1.2] Episode complete (nextEventId <= 0).");
                return;
            }

            if (slidingPanel != null)
            {
                Debug.Log($"[GameManager v1.2] Hiding panel, then showing event {nextEventId}");
                slidingPanel.Hide(() => ShowEvent(nextEventId));
            }
            else
            {
                Debug.LogWarning("[GameManager v1.2] No slidingPanel! Showing event directly.");
                ShowEvent(nextEventId);
            }
        }
    }
}