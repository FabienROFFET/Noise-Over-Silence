// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-14
// UPDATED : 2026-03-17 (v2.0.6)
// CHANGES : v2.0.6 - Hide SlidingPanel when event has choice hotspots
//                    Added explicit SaveCurrentEvent() called by Save button
//           v2.0.4 - SaveGame() called at end of every ShowEvent()
//           v2.0.3 - Removed auto-start from Start()
//           v2.0.2 - Added ChapterIntroScreen support
//           v2.0.1 - Fixed slidingPanel.Show → ShowEvent
// ============================================================

using UnityEngine;
using NoiseOverSilent.Core;
using NoiseOverSilent.Data;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private JsonLoader         jsonLoader;
        [SerializeField] private ImageDisplay       imageDisplay;
        [SerializeField] private SlidingPanel       slidingPanel;
        [SerializeField] private HotspotManager     hotspotManager;
        [SerializeField] private ChapterIntroScreen chapterIntroScreen;
        [SerializeField] private int                startEventId = 1;

        private EpisodeData currentEpisode;
        private GameEvent   currentEvent;
        private string      currentLanguage = "en";

        void Start()
        {
            currentLanguage = PlayerPrefs.GetString("Language", "en");
            Debug.Log("[GameManager v2.0.4] Ready — waiting for PLAY.");
        }

        // showIntro = true  → new game (show chapter title card)
        // showIntro = false → loading a save (skip straight to the event)
        public void LoadEpisode(int episodeNumber, int eventId = -1, bool showIntro = true)
        {
            currentEpisode = jsonLoader.LoadEpisode(episodeNumber, currentLanguage);

            if (currentEpisode == null)
            {
                Debug.LogError($"[GameManager v2.0.4] Failed to load episode {episodeNumber}!");
                return;
            }

            Debug.Log($"[GameManager v2.0.4] Loaded: {currentEpisode.title}");
            Debug.Log($"[GameManager v2.0.4] Chapter: {currentEpisode.chapter.title}");
            Debug.Log($"[GameManager v2.0.4] Events: {currentEpisode.events.Count}");

            int targetEvent = (eventId > 0) ? eventId : startEventId;

            if (showIntro && chapterIntroScreen != null && currentEpisode.chapter != null)
            {
                chapterIntroScreen.ShowChapterIntro(currentEpisode.chapter, () =>
                {
                    ShowEvent(targetEvent);
                });
            }
            else
            {
                ShowEvent(targetEvent);
            }
        }

        public void ShowEvent(int eventId)
        {
            currentEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (currentEvent == null)
            {
                Debug.LogError($"[GameManager v2.0.4] Event {eventId} not found!");
                return;
            }

            Debug.Log($"[GameManager v2.0.4] Showing event {eventId}: '{currentEvent.text.Substring(0, System.Math.Min(40, currentEvent.text.Length))}...'");

            if (!string.IsNullOrEmpty(currentEvent.soundscape_mp3))
                Debug.Log($"[GameManager v2.0.4] Soundscape: {currentEvent.soundscape_mp3}");

            if (!string.IsNullOrEmpty(currentEvent.voice_over))
                Debug.Log($"[GameManager v2.0.4] Voice over: {currentEvent.voice_over}");

            imageDisplay.DisplayImage(currentEvent.image_link);

            if (hotspotManager != null)
                hotspotManager.ShowHotspots(currentEvent.hotspots);
            else
                Debug.LogWarning("[GameManager v2.0.4] HotspotManager not assigned!");

            // If choice hotspots are on screen, the image drives navigation.
            // Show only the bottom text — keep the choice panel off-screen.
            bool hasChoiceHotspots = currentEvent.hotspots != null &&
                                     currentEvent.hotspots.Exists(h => h.type == "choice");

            if (hasChoiceHotspots)
            {
                slidingPanel.SetTextOnly(currentEvent); // text shown, panel stays hidden
            }
            else
            {
                slidingPanel.ShowEvent(currentEvent);   // text + choices + panel slides in
            }

            Debug.Log($"[GameManager v2.0.6] Panel {(hasChoiceHotspots ? "hidden (choice hotspots active)" : "shown")}.");

            // NOTE: No auto-save here. Player saves explicitly via the Save button.
        }

        // Called by the Save icon button — saves exactly where the player is right now
        public void SaveCurrentEvent()
        {
            if (currentEpisode == null || currentEvent == null)
            {
                Debug.LogWarning("[GameManager v2.0.5] Nothing to save yet.");
                return;
            }

            SaveLoadSystem.SaveGame(
                currentEpisode.episode,
                currentEvent.id,
                currentLanguage,
                currentEvent.text
            );
            Debug.Log($"[GameManager v2.0.5] Manually saved: Episode {currentEpisode.episode}, Event {currentEvent.id}");
        }

        public void MakeChoice(Choice choice)
        {
            Debug.Log($"[GameManager v2.0.4] Choice: '{choice.text}' → Event {choice.next_event}");

            hotspotManager?.HidePopup();

            if (currentEvent.stats != null)
                Debug.Log($"[GameManager v2.0.4] Stats: Physical={currentEvent.stats.physical}, Mental={currentEvent.stats.mental}");

            if (choice.flags != null)
                foreach (var flag in choice.flags)
                    Debug.Log($"[GameManager v2.0.4] Flag: {flag.Key} += {flag.Value}");

            if (currentEvent.inventory != null)
            {
                if (currentEvent.inventory.items?.Count > 0)
                    Debug.Log($"[GameManager v2.0.4] Items: {string.Join(", ", currentEvent.inventory.items)}");
                if (currentEvent.inventory.tapes?.Count > 0)
                    Debug.Log($"[GameManager v2.0.4] Tapes: {string.Join(", ", currentEvent.inventory.tapes)}");
            }

            if (choice.next_event == 0)
                Debug.Log("[GameManager v2.0.4] End of chapter!");
            else
                ShowEvent(choice.next_event);
        }

        public void SetHotspotManager(HotspotManager manager)
        {
            hotspotManager = manager;
            Debug.Log("[GameManager v2.0.4] HotspotManager assigned.");
        }

        // Called by the Load icon button. Finds and hides the landing page,
        // then jumps straight to the saved event with no chapter intro.
        public bool LoadFromSave()
        {
            SaveData save = SaveLoadSystem.LoadGame();

            if (save == null)
            {
                Debug.LogWarning("[GameManager v2.0.4] LoadFromSave: no save file found.");
                return false;
            }

            Debug.Log($"[GameManager v2.0.4] LoadFromSave: Episode {save.currentEpisode}, Event {save.currentEvent}");

            // Hide landing page if it is still showing
            GameObject landingPage = GameObject.Find("LandingPage");
            if (landingPage != null && landingPage.activeSelf)
                landingPage.SetActive(false);

            // Use the language from the save file
            currentLanguage = save.language ?? "en";

            // Load episode and jump to event — no chapter intro
            LoadEpisode(save.currentEpisode, save.currentEvent, false);
            return true;
        }
    }
}