using System.Collections.Generic;

namespace NoiseOverSilent.Data
{
    /// <summary>
    /// GameData v2.1 - 2026-03-17
    /// Hotspot now has type ("text" or "choice") and next_event
    /// </summary>

    [System.Serializable]
    public class ChapterInfo
    {
        public int number;
        public string title;
        public string intro_image;
        public float intro_duration;
    }

    [System.Serializable]
    public class Stats
    {
        public int physical;
        public int mental;
    }

    [System.Serializable]
    public class Inventory
    {
        public List<string> items;
        public List<string> tapes;
    }

    [System.Serializable]
    public class Hotspot
    {
        public float x;             // Normalized X (0=left, 1=right)
        public float y;             // Normalized Y (0=bottom, 1=top)
        public string text;         // Popup text (type=text) or label (type=choice)
        public int radius = 20;     // Circle radius in pixels
        public string type = "text"; // "text" = popup only | "choice" = navigate to next_event
        public int next_event = 0;  // Target event id (only used when type = "choice")
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
        public int next_event;
        public Dictionary<string, int> flags;
    }

    [System.Serializable]
    public class GameEvent
    {
        public int id;
        public string location;
        public string soundscape_mp3;
        public string voice_over;
        public string image_prompt;
        public string image_link;
        public Stats stats;
        public Inventory inventory;
        public string text;
        public List<Hotspot> hotspots;
        public List<Choice> choices;
    }

    [System.Serializable]
    public class HiddenFlags
    {
        public int empathy;
        public int obedience;
        public int curiosity;
    }

    [System.Serializable]
    public class EpisodeData
    {
        public ChapterInfo chapter;
        public int episode;
        public string title;
        public HiddenFlags hidden_flags;
        public List<GameEvent> events;
    }
}