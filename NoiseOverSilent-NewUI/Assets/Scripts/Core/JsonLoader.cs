// ============================================================
// PROJECT : Noise Over Silence
// FILE    : JsonLoader.cs
// PATH    : Assets/Scripts/Core/
// CREATED : 2026-02-14
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-14 - Initial version
// DESC    : Loads episode JSON files from StreamingAssets/Episodes/.
//           Called by GameManager at startup.
// ============================================================

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

        /// <summary>
        /// Loads an episode by name (e.g. "episode01") from StreamingAssets/Episodes/.
        /// Returns null if file not found or parse fails.
        /// </summary>
        public EpisodeData LoadEpisode(string episodeName)
        {
            string filePath = Path.Combine(episodesPath, episodeName + ".json");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[JsonLoader] File not found: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                EpisodeData episode = JsonUtility.FromJson<EpisodeData>(json);

                if (episode != null)
                    Debug.Log($"[JsonLoader] Loaded '{episodeName}' — {episode.events.Count} events.");
                else
                    Debug.LogError($"[JsonLoader] Failed to parse: {episodeName}");

                return episode;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JsonLoader] Error loading {episodeName}: {e.Message}");
                return null;
            }
        }
    }
}
