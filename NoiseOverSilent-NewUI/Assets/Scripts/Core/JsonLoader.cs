using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.Core
{
    /// <summary>
    /// Loads and manages episode JSON files from StreamingAssets
    /// </summary>
    public class JsonLoader : MonoBehaviour
    {
        private Dictionary<int, Episode> loadedEpisodes = new Dictionary<int, Episode>();

        /// <summary>
        /// Load an episode from StreamingAssets/Episodes/
        /// </summary>
        public Episode LoadEpisode(int episodeNumber)
        {
            // Check cache first
            if (loadedEpisodes.ContainsKey(episodeNumber))
            {
                Debug.Log($"[JsonLoader] Episode {episodeNumber} loaded from cache");
                return loadedEpisodes[episodeNumber];
            }

            string fileName = $"episode{episodeNumber:D2}.json";
            string filePath = Path.Combine(Application.streamingAssetsPath, "Episodes", fileName);

            Debug.Log($"[JsonLoader] Loading: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[JsonLoader] Episode file not found: {filePath}");
                return null;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                Episode episode = JsonUtility.FromJson<Episode>(jsonContent);

                if (episode == null || episode.events == null || episode.events.Count == 0)
                {
                    Debug.LogError($"[JsonLoader] Failed to parse episode {episodeNumber} or it's empty");
                    return null;
                }

                // Cache it
                loadedEpisodes[episodeNumber] = episode;

                Debug.Log($"[JsonLoader] Successfully loaded Episode {episode.episode}: {episode.title} ({episode.events.Count} events)");
                return episode;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JsonLoader] Error loading episode {episodeNumber}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find a specific event by ID in an episode
        /// </summary>
        public GameEvent GetEvent(Episode episode, int eventId)
        {
            if (episode == null || episode.events == null)
            {
                Debug.LogError("[JsonLoader] Episode or events list is null");
                return null;
            }

            GameEvent foundEvent = episode.events.Find(e => e.id == eventId);
            
            if (foundEvent == null)
            {
                Debug.LogError($"[JsonLoader] Event ID {eventId} not found in episode {episode.episode}");
            }

            return foundEvent;
        }

        /// <summary>
        /// Get all available episode numbers
        /// </summary>
        public List<int> GetAvailableEpisodes()
        {
            List<int> episodes = new List<int>();
            string episodesPath = Path.Combine(Application.streamingAssetsPath, "Episodes");

            if (!Directory.Exists(episodesPath))
            {
                Debug.LogError($"[JsonLoader] Episodes directory not found: {episodesPath}");
                return episodes;
            }

            string[] files = Directory.GetFiles(episodesPath, "episode*.json");
            
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                // Extract number from "episode01", "episode02", etc.
                if (fileName.Length >= 9 && int.TryParse(fileName.Substring(7), out int episodeNum))
                {
                    episodes.Add(episodeNum);
                }
            }

            episodes.Sort();
            Debug.Log($"[JsonLoader] Found {episodes.Count} episodes");
            return episodes;
        }

        /// <summary>
        /// Clear episode cache (useful for development)
        /// </summary>
        public void ClearCache()
        {
            loadedEpisodes.Clear();
            Debug.Log("[JsonLoader] Episode cache cleared");
        }

        /// <summary>
        /// Preload all episodes
        /// </summary>
        public void PreloadAllEpisodes()
        {
            List<int> episodeNumbers = GetAvailableEpisodes();
            foreach (int episodeNum in episodeNumbers)
            {
                LoadEpisode(episodeNum);
            }
            Debug.Log($"[JsonLoader] Preloaded {episodeNumbers.Count} episodes");
        }
    }
}
