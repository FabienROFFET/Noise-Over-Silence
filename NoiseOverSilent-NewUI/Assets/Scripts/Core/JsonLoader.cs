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
                string json = File.ReadAllText(filePath);
                EpisodeData episode = JsonUtility.FromJson<EpisodeData>(json);

                if (episode != null)
                    Debug.Log($"Loaded '{episodeName}' — {episode.events.Count} events.");
                else
                    Debug.LogError($"Failed to parse: {episodeName}");

                return episode;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading {episodeName}: {e.Message}");
                return null;
            }
        }
    }
}
