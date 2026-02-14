using UnityEngine;
using NoiseOverSilent.Core;
using NoiseOverSilent.Data;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private JsonLoader jsonLoader;
        [SerializeField] private ImageDisplay imageDisplay;
        [SerializeField] private SlidingPanel slidingPanel;

        [Header("Start Settings")]
        [SerializeField] private int startEpisode = 1;
        [SerializeField] private int startEventId = 1;

        private EpisodeData currentEpisode;

        private void Awake()
        {
            if (jsonLoader == null)
                jsonLoader = gameObject.AddComponent<JsonLoader>();
        }

        private void Start()
        {
            LoadEpisode(startEpisode, startEventId);
        }

        public void LoadEpisode(int episodeNumber, int eventId = 1)
        {
            string episodeName = "episode" + episodeNumber.ToString("D2");
            currentEpisode = jsonLoader.LoadEpisode(episodeName);

            if (currentEpisode != null)
                ShowEvent(eventId);
            else
                Debug.LogError($"Failed to load episode {episodeNumber}.");
        }

        public void ShowEvent(int eventId)
        {
            if (currentEpisode == null) return;

            // Find event by id (not array index)
            GameEvent gameEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (gameEvent == null)
            {
                Debug.LogError($"Event id {eventId} not found in episode.");
                return;
            }

            // Show fullscreen image
            if (imageDisplay != null)
                imageDisplay.DisplayImage(gameEvent.image_link);

            // Show panel with text and choices
            if (slidingPanel != null)
                slidingPanel.ShowEvent(gameEvent);
        }

        public void MakeChoice(int nextEventId)
        {
            // next_event: null in JSON becomes 0 in C# int, treat 0 as end of episode
            if (nextEventId <= 0)
            {
                Debug.Log("Episode complete.");
                return;
            }

            if (slidingPanel != null)
                slidingPanel.Hide(() => ShowEvent(nextEventId));
            else
                ShowEvent(nextEventId);
        }
    }
}
