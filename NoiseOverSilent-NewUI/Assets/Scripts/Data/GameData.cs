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
        public string text_position = "right";
        public float panel_width = 0.33f;
        public List<Choice> choices = new List<Choice>();
    }

    [Serializable]
    public class Choice
    {
        public string text;
        public int next_event;
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
