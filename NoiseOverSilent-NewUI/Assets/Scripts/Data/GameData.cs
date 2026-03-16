using System.Collections.Generic;

namespace NoiseOverSilent.Data
{
    /// <summary>
    /// GameData v2.0 - 2026-03-17
    /// Added Hotspot support for clickable areas on images
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
        public float x;           // Normalized X position (0-1, where 0=left, 1=right)
        public float y;           // Normalized Y position (0-1, where 0=bottom, 1=top)
        public string text;       // Text to display when clicked
        public int radius = 20;   // Circle radius in pixels
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
        public List<Hotspot> hotspots;  // NEW: Clickable areas on the image
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