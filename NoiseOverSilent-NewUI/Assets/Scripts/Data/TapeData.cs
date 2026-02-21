// ============================================================
// PROJECT : Noise Over Silence
// FILE    : TapeData.cs
// PATH    : Assets/Scripts/Data/
// CREATED : 2026-02-21
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-21 - Initial version
// DESC    : Data structure for tape cassettes
// ============================================================

using System;
using System.Collections.Generic;

namespace NoiseOverSilent.Data
{
    [Serializable]
    public class Tape
    {
        public string id;           // "tape_01"
        public string title;        // "Forgotten Melodies"
        public string artist;       // "Unknown"
        public string description;  // "A haunting melody from before..."
        public string audioFile;    // "Audio/Tapes/tape_01"
        public bool isUnlocked;     // Player has found this tape
    }

    [Serializable]
    public class TapeCollection
    {
        public List<Tape> tapes = new List<Tape>();
    }
}
