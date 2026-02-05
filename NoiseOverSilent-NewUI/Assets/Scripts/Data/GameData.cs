using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoiseOverSilent.Data
{
    /// <summary>
    /// Main episode container - matches JSON structure exactly
    /// </summary>
    [Serializable]
    public class Episode
    {
        public int episode;
        public string title;
        public List<GameEvent> events;
    }

    /// <summary>
    /// Individual story event/scene
    /// </summary>
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
        public List<Choice> choices;
        public string ending; // Optional - for terminal events
    }

    /// <summary>
    /// Player stats
    /// </summary>
    [Serializable]
    public class Stats
    {
        public int physical = 100;
        public int mental = 85;

        public Stats() { }
        
        public Stats(int phys, int ment)
        {
            physical = phys;
            mental = ment;
        }

        public Stats Clone()
        {
            return new Stats(physical, mental);
        }
    }

    /// <summary>
    /// Player inventory
    /// </summary>
    [Serializable]
    public class Inventory
    {
        public List<string> items = new List<string>();
        public List<string> tapes = new List<string>();

        public Inventory() { }

        public Inventory Clone()
        {
            var clone = new Inventory();
            clone.items = new List<string>(items);
            clone.tapes = new List<string>(tapes);
            return clone;
        }

        public bool HasItem(string itemName)
        {
            return items.Contains(itemName) || tapes.Contains(itemName);
        }

        public void RemoveItem(string itemName)
        {
            items.Remove(itemName);
            tapes.Remove(itemName);
        }

        public void AddItem(string itemName)
        {
            if (!items.Contains(itemName))
                items.Add(itemName);
        }

        public void AddTape(string tapeName)
        {
            if (!tapes.Contains(tapeName))
                tapes.Add(tapeName);
        }
    }

    /// <summary>
    /// Choice option in an event
    /// </summary>
    [Serializable]
    public class Choice
    {
        public string text;
        public int? next_event; // Nullable - null means ending
        public List<string> lose_items;

        public bool IsEnding => !next_event.HasValue;
    }

    /// <summary>
    /// Player save data
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int currentEpisode;
        public int currentEventId;
        public Stats stats;
        public Inventory inventory;
        public List<HistoryEntry> history;
        public string saveTime;
        public int playTimeSeconds;

        public SaveData()
        {
            stats = new Stats();
            inventory = new Inventory();
            history = new List<HistoryEntry>();
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    /// <summary>
    /// History log entry for scrollback
    /// </summary>
    [Serializable]
    public class HistoryEntry
    {
        public int eventId;
        public string location;
        public string text;
        public string choiceTaken; // Which choice was selected
        public string timestamp;

        public HistoryEntry(int id, string loc, string txt, string choice = "")
        {
            eventId = id;
            location = loc;
            text = txt;
            choiceTaken = choice;
            timestamp = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
