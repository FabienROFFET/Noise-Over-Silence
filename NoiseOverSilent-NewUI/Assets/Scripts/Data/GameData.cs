// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameData.cs
// PATH    : Assets/Scripts/Data/
// CREATED : 2026-02-14
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-14 - Initial version
// DESC    : Data structures matching episode01.json exactly.
//           EpisodeData, GameEvent, Choice, Stats, Inventory.
// ============================================================

using System;
using System.Collections.Generic;

namespace NoiseOverSilent.Data
{
    [Serializable]
    public class EpisodeData
    {
        public int episode;
        public string title;
        public List<GameEvent> events = new List<GameEvent>();
    }

    [Serializable]
    public class GameEvent
    {
        public int id;
        public string location;
        public string soundscape_mp3;
        public string image_prompt;
        public string image_link;
        public Stats stats;
        public Inventory inventory;
        public string text;
        public string text_position;  // "left", "right", "center"
        public float panel_width;     // 0.25 to 0.5 (default 0.33)
        public List<Choice> choices = new List<Choice>();
    }

    [Serializable]
    public class Choice
    {
        public string text;
        public int next_event;        // 0 = end of episode
    }

    [Serializable]
    public class Stats
    {
        public int physical;
        public int mental;
    }

    [Serializable]
    public class Inventory
    {
        public List<string> items = new List<string>();
        public List<string> tapes = new List<string>();
    }
}
