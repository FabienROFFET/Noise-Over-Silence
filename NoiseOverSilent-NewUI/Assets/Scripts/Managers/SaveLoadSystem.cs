// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SaveLoadSystem.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-02-22
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-22 - Initial save/load system
// ============================================================

using System;
using System.IO;
using UnityEngine;

namespace NoiseOverSilent.Managers
{
    [Serializable]
    public class SaveData
    {
        public int currentEpisode;
        public int currentEvent;
        public string language;
        public string timestamp;
        public string previewText; // First 50 chars of current event text
    }

    public static class SaveLoadSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        /// <summary>
        /// Save current game state
        /// </summary>
        public static void SaveGame(int episode, int eventId, string language, string previewText)
        {
            SaveData data = new SaveData
            {
                currentEpisode = episode,
                currentEvent = eventId,
                language = language,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                previewText = previewText.Length > 50 ? previewText.Substring(0, 50) + "..." : previewText
            };

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveLoadSystem v1.0] Game saved! Episode {episode}, Event {eventId}");
                Debug.Log($"[SaveLoadSystem v1.0] Save location: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadSystem v1.0] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Load saved game state
        /// </summary>
        public static SaveData LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("[SaveLoadSystem v1.0] No save file found.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveLoadSystem v1.0] Game loaded! Episode {data.currentEpisode}, Event {data.currentEvent}");
                Debug.Log($"[SaveLoadSystem v1.0] Saved on: {data.timestamp}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadSystem v1.0] Load failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public static bool SaveExists()
        {
            return File.Exists(SavePath);
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveLoadSystem v1.0] Save file deleted.");
            }
        }
    }
}