// ============================================================
// PROJECT : Noise Over Silence
// FILE    : JsonLoader.cs
// PATH    : Assets/Scripts/Core/
// CREATED : 2026-02-14
// VERSION : 1.2
// CHANGES : v1.2 - 2026-02-22 - Language support (episodeXX_lang.json)
//           v1.1 - 2026-02-16 - Path fixes
//           v1.0 - 2026-02-14 - Initial version
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
        /// Loads an episode by number and language (e.g. episode 1, "en") from StreamingAssets/Episodes/.
        /// Returns null if file not found or parse fails.
        /// </summary>
        public EpisodeData LoadEpisode(int episodeNumber, string language = "en")
        {
            string episodeName = $"episode{episodeNumber:D2}_{language}";
            string filePath = Path.Combine(episodesPath, episodeName + ".json");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[JsonLoader v1.2] File not found: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                EpisodeData episode = JsonUtility.FromJson<EpisodeData>(json);

                if (episode != null)
                    Debug.Log($"[JsonLoader v1.2] Loaded '{episodeName}' — {episode.events.Count} events.");
                else
                    Debug.LogError($"[JsonLoader v1.2] Failed to parse: {episodeName}");

                return episode;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JsonLoader v1.2] Error loading {episodeName}: {e.Message}");
                return null;
            }
        }
    }
}