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

        private void Start()
        {
            // JsonLoader added by UIBuilder — get it here
            if (jsonLoader == null)
                jsonLoader = GetComponent<JsonLoader>();

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

            GameEvent gameEvent = currentEpisode.events.Find(e => e.id == eventId);

            if (gameEvent == null)
            {
                Debug.LogError($"Event {eventId} not found.");
                return;
            }

            if (imageDisplay != null)
                imageDisplay.DisplayImage(gameEvent.image_link);

            if (slidingPanel != null)
                slidingPanel.ShowEvent(gameEvent);
        }

        public void MakeChoice(int nextEventId)
        {
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
