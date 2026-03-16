// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-14
// UPDATED : 2026-03-17 (Added HotspotManager)
// DESC    : Core game manager - handles events, choices, stats
// ============================================================

using UnityEngine;
using NoiseOverSilent.Core;
using NoiseOverSilent.Data;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Managers
{
    /// <summary>
    /// GameManager v2.0 - 2026-03-17
    /// Now manages hotspot display for each event
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private JsonLoader jsonLoader;
        [SerializeField] private ImageDisplay imageDisplay;
        [SerializeField] private SlidingPanel slidingPanel;
        [SerializeField] private HotspotManager hotspotManager; // NEW!
        [SerializeField] private int startEpisode = 1;
        [SerializeField] private int startEventId = 1;

        private EpisodeData currentEpisode;
        private GameEvent currentEvent;

        void Start()
        {
            Debug.Log("[GameManager v2.0] Initializing...");
            LoadEpisode(startEpisode); // Will use startEventId internally
        }

        public void LoadEpisode(int episodeNumber, int eventId = -1)
        {
            currentEpisode = jsonLoader.LoadEpisode(episodeNumber);

            if (currentEpisode == null)
            {
                Debug.LogError($"[GameManager v2.0] Failed to load episode {episodeNumber}!");
                return;
            }

            Debug.Log($"[GameManager v2.0] Loaded: {currentEpisode.title}");
            Debug.Log($"[GameManager v2.0] Chapter: {currentEpisode.chapter.title}");
            Debug.Log($"[GameManager v2.0] Events: {currentEpisode.events.Count}");

            // Use provided eventId or default to startEventId
            int targetEvent = (eventId > 0) ? eventId : startEventId;
            ShowEvent(targetEvent);
        }

        public void ShowEvent(int eventId)
        {
            currentEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (currentEvent == null)
            {
                Debug.LogError($"[GameManager v2.0] Event {eventId} not found!");
                return;
            }

            Debug.Log($"[GameManager v2.0] Showing event {eventId}: '{currentEvent.text.Substring(0, System.Math.Min(40, currentEvent.text.Length))}...'");

            // Play soundscape
            if (!string.IsNullOrEmpty(currentEvent.soundscape_mp3))
            {
                Debug.Log($"[GameManager v2.0] Soundscape: {currentEvent.soundscape_mp3}");
                // soundManager.PlaySoundscape(currentEvent.soundscape_mp3);
            }

            // Play voice over
            if (!string.IsNullOrEmpty(currentEvent.voice_over))
            {
                Debug.Log($"[GameManager v2.0] Voice over: {currentEvent.voice_over}");
                // soundManager.PlayVoiceOver(currentEvent.voice_over);
            }

            // Display image
            imageDisplay.DisplayImage(currentEvent.image_link);

            // Show hotspots (NEW!)
            if (hotspotManager != null)
            {
                hotspotManager.ShowHotspots(currentEvent.hotspots);
            }
            else
            {
                Debug.LogWarning("[GameManager v2.0] HotspotManager not assigned!");
            }

            // Show narrative panel
            slidingPanel.Show(currentEvent);
        }

        public void MakeChoice(Choice choice)
        {
            Debug.Log($"[GameManager v2.0] Choice made: '{choice.text}' → Event {choice.next_event}");

            // Hide hotspot popup before transition (NEW!)
            if (hotspotManager != null)
            {
                hotspotManager.HidePopup();
            }

            // Apply stat changes
            if (currentEvent.stats != null)
            {
                Debug.Log($"[GameManager v2.0] Stats: Physical={currentEvent.stats.physical}, Mental={currentEvent.stats.mental}");
            }

            // Apply flag changes
            if (choice.flags != null)
            {
                foreach (var flag in choice.flags)
                {
                    Debug.Log($"[GameManager v2.0] Flag change: {flag.Key} += {flag.Value}");
                }
            }

            // Check for inventory changes
            if (currentEvent.inventory != null)
            {
                if (currentEvent.inventory.items != null && currentEvent.inventory.items.Count > 0)
                {
                    Debug.Log($"[GameManager v2.0] Items: {string.Join(", ", currentEvent.inventory.items)}");
                }
                if (currentEvent.inventory.tapes != null && currentEvent.inventory.tapes.Count > 0)
                {
                    Debug.Log($"[GameManager v2.0] Tapes: {string.Join(", ", currentEvent.inventory.tapes)}");
                }
            }

            // Transition to next event
            if (choice.next_event == 0)
            {
                Debug.Log("[GameManager v2.0] End of chapter!");
                // Handle chapter end
            }
            else
            {
                ShowEvent(choice.next_event);
            }
        }

        // Called by UIBuilder during initialization
        public void SetHotspotManager(HotspotManager manager)
        {
            hotspotManager = manager;
            Debug.Log("[GameManager v2.0] HotspotManager assigned.");
        }
    }
}