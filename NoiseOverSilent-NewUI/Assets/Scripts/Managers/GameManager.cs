// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-14
// VERSION : 1.1
// CHANGES : v1.1 - 2026-02-16 - Added debug logs for event flow
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
                Debug.LogError($"[GameManager v1.1] Failed to load episode {episodeNumber}.");
        }

        private IEnumerator ShowEventNextFrame(int eventId)
        {
            yield return null;
            ShowEvent(eventId);
        }

        public void ShowEvent(int eventId)
        {
            if (currentEpisode == null) return;

            GameEvent gameEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (gameEvent == null)
            {
                Debug.LogError($"[GameManager v1.1] Event {eventId} not found.");
                return;
            }

            Debug.Log($"[GameManager v1.1] Showing event {eventId}: '{gameEvent.text?.Substring(0, System.Math.Min(40, gameEvent.text?.Length ?? 0))}...'");

            if (imageDisplay != null)
                imageDisplay.DisplayImage(gameEvent.image_link);

            if (slidingPanel != null)
                slidingPanel.ShowEvent(gameEvent);
        }

        public void MakeChoice(int nextEventId)
        {
            Debug.Log($"[GameManager v1.1] MakeChoice called: nextEventId={nextEventId}");

            if (nextEventId <= 0)
            {
                Debug.Log("[GameManager v1.1] Episode complete (nextEventId <= 0).");
                return;
            }

            if (slidingPanel != null)
            {
                Debug.Log($"[GameManager v1.1] Hiding panel, then showing event {nextEventId}");
                slidingPanel.Hide(() => ShowEvent(nextEventId));
            }
            else
            {
                Debug.LogWarning("[GameManager v1.1] No slidingPanel! Showing event directly.");
                ShowEvent(nextEventId);
            }
        }
    }
}