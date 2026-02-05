using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoiseOverSilent.Data;
using NoiseOverSilent.Core;
// Removed: using NoiseOverSilent.Audio; - doesn't exist yet

namespace NoiseOverSilent.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private JsonLoader jsonLoader;
        // Removed UIController, AudioManager, SaveManager - will add when those scripts exist
        
        [Header("Episode Settings")]
        [SerializeField] private string currentEpisode = "episode01";
        
        private EpisodeData currentEpisodeData;
        private int currentEventIndex = 0;
        
        private void Awake()
        {
            if (jsonLoader == null)
            {
                jsonLoader = gameObject.AddComponent<JsonLoader>();
            }
        }
        
        private void Start()
        {
            LoadEpisode(currentEpisode);
        }
        
        public void LoadEpisode(string episodeName)
        {
            currentEpisodeData = jsonLoader.LoadEpisode(episodeName);
            
            if (currentEpisodeData != null && currentEpisodeData.events.Count > 0)
            {
                currentEventIndex = 0;
                DisplayCurrentEvent();
            }
            else
            {
                Debug.LogError($"Failed to load episode: {episodeName}");
            }
        }
        
        private void DisplayCurrentEvent()
        {
            if (currentEventIndex < currentEpisodeData.events.Count)
            {
                GameEvent currentEvent = currentEpisodeData.events[currentEventIndex];
                
                // Display event info in console for now (until UI is connected)
                Debug.Log($"Event: {currentEvent.id}");
                if (!string.IsNullOrEmpty(currentEvent.narrator))
                    Debug.Log($"Narrator: {currentEvent.narrator}");
                if (!string.IsNullOrEmpty(currentEvent.character))
                    Debug.Log($"{currentEvent.character}: {currentEvent.dialogue}");
                
                // TODO: Call uiController.DisplayEvent(currentEvent) when UIController exists
            }
            else
            {
                Debug.Log("Episode completed!");
            }
        }
        
        public void MakeChoice(int choiceIndex)
        {
            if (currentEventIndex >= currentEpisodeData.events.Count) return;
            
            GameEvent currentEvent = currentEpisodeData.events[currentEventIndex];
            
            if (choiceIndex >= 0 && choiceIndex < currentEvent.choices.Count)
            {
                Choice selectedChoice = currentEvent.choices[choiceIndex];
                
                Debug.Log($"Selected: {selectedChoice.text}");
                
                // Move to next event based on choice
                if (!string.IsNullOrEmpty(selectedChoice.nextEvent))
                {
                    // Find event by ID
                    for (int i = 0; i < currentEpisodeData.events.Count; i++)
                    {
                        if (currentEpisodeData.events[i].id == selectedChoice.nextEvent)
                        {
                            currentEventIndex = i;
                            DisplayCurrentEvent();
                            return;
                        }
                    }
                }
                else
                {
                    // Just go to next sequential event
                    currentEventIndex++;
                    DisplayCurrentEvent();
                }
            }
        }
        
        public void ContinueToNextEvent()
        {
            currentEventIndex++;
            DisplayCurrentEvent();
        }
    }
}