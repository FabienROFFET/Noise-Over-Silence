using System;
using System.Collections.Generic;

namespace NoiseOverSilent.Data
{
    [Serializable]
    public class EpisodeData
    {
        public string episodeId;
        public string title;
        public List<GameEvent> events = new List<GameEvent>();
    }

    [Serializable]
    public class GameEvent
    {
        public string id;
        public string narrator;
        public string character;
        public string dialogue;
        public string image;
        public string audio;
        public List<Choice> choices = new List<Choice>();
        public List<Consequence> consequences = new List<Consequence>();
    }

    [Serializable]
    public class Choice
    {
        public string text;
        public string nextEvent;
        public List<Consequence> consequences = new List<Consequence>();
    }

    [Serializable]
    public class Consequence
    {
        public string type;
        public string target;
        public int value;
    }

    [Serializable]
    public class GameState
    {
        public string currentEpisode;
        public string currentEventId;
        public Dictionary<string, int> relationships = new Dictionary<string, int>();
        public List<string> inventory = new List<string>();
        public Dictionary<string, bool> flags = new Dictionary<string, bool>();
    }
}