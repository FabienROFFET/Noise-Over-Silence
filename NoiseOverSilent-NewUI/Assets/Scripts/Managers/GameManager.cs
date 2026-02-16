// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-14
// AUTHOR  : Noise Over Silence Dev Team
// DESC    : Main game controller. Loads episodes via JsonLoader,
//           navigates events by id, drives ImageDisplay and
//           SlidingPanel. References wired by UIBuilder at runtime.
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

        [Header("Start Settings")]
        [SerializeField] private int startEpisode  = 1;
        [SerializeField] private int startEventId  = 1;

        private EpisodeData currentEpisode;

        private void Start()
        {
            LoadEpisode(startEpisode, startEventId);
        }

        public void LoadEpisode(int episodeNumber, int eventId = 1)
        {
            string name = "episode" + episodeNumber.ToString("D2");
            currentEpisode = jsonLoader.LoadEpisode(name);

            if (currentEpisode != null)
                StartCoroutine(ShowEventNextFrame(eventId));
            else
                Debug.LogError($"[GameManager] Failed to load episode {episodeNumber}.");
        }

        // Wait one frame so all Awake/Start methods finish before showing first event
        private IEnumerator ShowEventNextFrame(int eventId)
        {
            yield return null;
            ShowEvent(eventId);
        }

        /// <summary>Finds event by id and displays it.</summary>
        public void ShowEvent(int eventId)
        {
            if (currentEpisode == null) return;

            GameEvent gameEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (gameEvent == null)
            {
                Debug.LogError($"[GameManager] Event {eventId} not found.");
                return;
            }

            // Show fullscreen image
            if (imageDisplay != null)
                imageDisplay.DisplayImage(gameEvent.image_link);

            // Show panel with text and choices
            if (slidingPanel != null)
                slidingPanel.ShowEvent(gameEvent);
        }

        /// <summary>Called by ChoiceButton when player picks a choice.</summary>
        public void MakeChoice(int nextEventId)
        {
            // next_event: null in JSON becomes 0 — treat as episode end
            if (nextEventId <= 0)
            {
                Debug.Log("[GameManager] Episode complete.");
                return;
            }

            if (slidingPanel != null)
                slidingPanel.Hide(() => ShowEvent(nextEventId));
            else
                ShowEvent(nextEventId);
        }
    }
}