using System.IO;
using UnityEngine;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.Core
{
    public class JsonLoader : MonoBehaviour
    {
        private string episodesPath;

        private void Awake()
        {
            // Path to StreamingAssets/Episodes folder
            episodesPath = Path.Combine(Application.streamingAssetsPath, "Episodes");
        }

        public EpisodeData LoadEpisode(string episodeName)
        {
            string filePath = Path.Combine(episodesPath, episodeName + ".json");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Episode file not found: {filePath}");
                return null;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                EpisodeData episode = JsonUtility.FromJson<EpisodeData>(jsonContent);

                if (episode != null)
                {
                    Debug.Log($"Successfully loaded episode: {episodeName}");
                    Debug.Log($"Episode contains {episode.events.Count} events");
                }
                else
                {
                    Debug.LogError($"Failed to parse episode: {episodeName}");
                }

                return episode;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading episode {episodeName}: {e.Message}");
                return null;
            }
        }

        public bool SaveEpisode(EpisodeData episode, string episodeName)
        {
            string filePath = Path.Combine(episodesPath, episodeName + ".json");

            try
            {
                string jsonContent = JsonUtility.ToJson(episode, true);
                File.WriteAllText(filePath, jsonContent);
                Debug.Log($"Episode saved: {filePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving episode {episodeName}: {e.Message}");
                return false;
            }
        }

        public string[] GetAvailableEpisodes()
        {
            if (!Directory.Exists(episodesPath))
            {
                Debug.LogWarning($"Episodes directory not found: {episodesPath}");
                return new string[0];
            }

            string[] files = Directory.GetFiles(episodesPath, "*.json");
            string[] episodeNames = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                episodeNames[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return episodeNames;
        }
    }
}