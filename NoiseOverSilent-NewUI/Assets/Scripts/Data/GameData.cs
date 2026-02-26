// ============================================================
// PROJECT : Noise Over Silence
// FILE    : GameData.cs
// PATH    : Assets/Scripts/Data/
// CREATED : 2026-02-14
// VERSION : 1.3
// CHANGES : v1.3 - 2026-02-22 - Added ChapterInfo for chapter intro screens
//           v1.2 - 2026-02-21 - Added voice_over support
//           v1.1 - 2026-02-21 - Added unlock_tape to Choice
//           v1.0 - 2026-02-14 - Initial version
// DESC    : Data structures matching episode01.json exactly.
//           EpisodeData, GameEvent, Choice, Stats, Inventory, ChapterInfo.
// ============================================================

using System;
using System.Collections.Generic;

namespace NoiseOverSilent.Data
{
    [Serializable]
    public class EpisodeData
    {
        public ChapterInfo chapter;       // NEW: Chapter intro information
        public int episode;
        public string title;
        public List<GameEvent> events = new List<GameEvent>();
    }

    [Serializable]
    public class ChapterInfo
    {
        public int number;                // Chapter number (1, 2, 3...)
        public string title;              // Chapter title (e.g., "The Day Still Starts")
        public string intro_image;        // Image path (e.g., "images/chapters/chapter1_intro.png")
        public float intro_duration = 3f; // How long to show intro (seconds, default 3)
    }

    [Serializable]
    public class GameEvent
    {
        public int id;
        public string location;
        public string soundscape_mp3;
        public string voice_over;     // NEW: path to voice over audio file (e.g. "vo/ep1_event1_en")
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
        public string unlock_tape;    // "tape_02" - unlocks tape when choice is selected
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