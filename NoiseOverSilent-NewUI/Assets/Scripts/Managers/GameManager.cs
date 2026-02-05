using System.Collections.Generic;
using UnityEngine;
using NoiseOverSilent.Data;
using NoiseOverSilent.UI;
using NoiseOverSilent.Audio;

namespace NoiseOverSilent.Managers
{
    /// <summary>
    /// Central game controller - manages state, progression, and coordinates all systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private JsonLoader jsonLoader;
        [SerializeField] private UIController uiController;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveManager saveManager;

        [Header("Game State")]
        [SerializeField] private int currentEpisodeNumber = 1;
        [SerializeField] private int currentEventId = 1;

        // Runtime data
        private Episode currentEpisode;
        private GameEvent currentEvent;
        private Stats playerStats;
        private Inventory playerInventory;
        private List<HistoryEntry> eventHistory;

        // Singleton
        public static GameManager Instance { get; private set; }

        // Public accessors
        public Stats PlayerStats => playerStats;
        public Inventory PlayerInventory => playerInventory;
        public GameEvent CurrentEvent => currentEvent;
        public List<HistoryEntry> EventHistory => eventHistory;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize
            playerStats = new Stats();
            playerInventory = new Inventory();
            eventHistory = new List<HistoryEntry>();

            Debug.Log("[GameManager] Initialized");
        }

        private void Start()
        {
            // Auto-load first episode and start game
            LoadEpisode(currentEpisodeNumber);
            GoToEvent(currentEventId);
        }

        /// <summary>
        /// Load an episode by number
        /// </summary>
        public void LoadEpisode(int episodeNumber)
        {
            currentEpisode = jsonLoader.LoadEpisode(episodeNumber);
            
            if (currentEpisode == null)
            {
                Debug.LogError($"[GameManager] Failed to load episode {episodeNumber}");
                return;
            }

            currentEpisodeNumber = episodeNumber;
            Debug.Log($"[GameManager] Episode loaded: {currentEpisode.title}");
        }

        /// <summary>
        /// Navigate to a specific event
        /// </summary>
        public void GoToEvent(int eventId)
        {
            if (currentEpisode == null)
            {
                Debug.LogError("[GameManager] No episode loaded!");
                return;
            }

            GameEvent newEvent = jsonLoader.GetEvent(currentEpisode, eventId);
            
            if (newEvent == null)
            {
                Debug.LogError($"[GameManager] Event {eventId} not found!");
                return;
            }

            currentEvent = newEvent;
            currentEventId = eventId;

            // Update stats if event has them
            if (newEvent.stats != null)
            {
                playerStats = newEvent.stats.Clone();
            }

            // Update inventory if event has it
            if (newEvent.inventory != null)
            {
                playerInventory = newEvent.inventory.Clone();
            }

            // Add to history
            AddToHistory(newEvent);

            // Update UI
            uiController.DisplayEvent(newEvent, playerStats, playerInventory);

            // Play soundscape
            if (!string.IsNullOrEmpty(newEvent.soundscape_mp3))
            {
                audioManager.PlaySoundscape(newEvent.soundscape_mp3);
            }

            Debug.Log($"[GameManager] Now at Event {eventId}: {newEvent.location}");
        }

        /// <summary>
        /// Player makes a choice
        /// </summary>
        public void MakeChoice(Choice choice)
        {
            if (choice == null)
            {
                Debug.LogError("[GameManager] Choice is null!");
                return;
            }

            // Remove items if needed
            if (choice.lose_items != null && choice.lose_items.Count > 0)
            {
                foreach (string item in choice.lose_items)
                {
                    playerInventory.RemoveItem(item);
                    Debug.Log($"[GameManager] Lost item: {item}");
                }
            }

            // Update history with choice
            if (eventHistory.Count > 0)
            {
                eventHistory[eventHistory.Count - 1].choiceTaken = choice.text;
            }

            // Navigate to next event
            if (choice.next_event.HasValue)
            {
                GoToEvent(choice.next_event.Value);
            }
            else
            {
                // Ending reached
                HandleEnding();
            }
        }

        /// <summary>
        /// Handle game ending
        /// </summary>
        private void HandleEnding()
        {
            Debug.Log("[GameManager] Ending reached!");
            uiController.ShowEnding(currentEvent);
            
            // Stop music
            audioManager.StopAllAudio();
        }

        /// <summary>
        /// Add event to history log
        /// </summary>
        private void AddToHistory(GameEvent gameEvent)
        {
            HistoryEntry entry = new HistoryEntry(
                gameEvent.id,
                gameEvent.location,
                gameEvent.text
            );
            eventHistory.Add(entry);

            // Limit history size (prevent memory issues)
            if (eventHistory.Count > 100)
            {
                eventHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Save game state
        /// </summary>
        public void SaveGame(int slotNumber)
        {
            SaveData saveData = new SaveData
            {
                currentEpisode = currentEpisodeNumber,
                currentEventId = currentEventId,
                stats = playerStats.Clone(),
                inventory = playerInventory.Clone(),
                history = new List<HistoryEntry>(eventHistory)
            };

            saveManager.SaveGame(saveData, slotNumber);
            Debug.Log($"[GameManager] Game saved to slot {slotNumber}");
        }

        /// <summary>
        /// Load game state
        /// </summary>
        public void LoadGame(int slotNumber)
        {
            SaveData saveData = saveManager.LoadGame(slotNumber);
            
            if (saveData == null)
            {
                Debug.LogWarning($"[GameManager] No save data in slot {slotNumber}");
                return;
            }

            // Restore state
            LoadEpisode(saveData.currentEpisode);
            playerStats = saveData.stats;
            playerInventory = saveData.inventory;
            eventHistory = saveData.history;
            
            GoToEvent(saveData.currentEventId);

            Debug.Log($"[GameManager] Game loaded from slot {slotNumber}");
        }

        /// <summary>
        /// Restart current episode from beginning
        /// </summary>
        public void RestartEpisode()
        {
            playerStats = new Stats();
            playerInventory = new Inventory();
            eventHistory.Clear();
            
            // Find first event ID in episode
            if (currentEpisode != null && currentEpisode.events.Count > 0)
            {
                GoToEvent(currentEpisode.events[0].id);
            }
            
            Debug.Log("[GameManager] Episode restarted");
        }

        /// <summary>
        /// Quit to main menu
        /// </summary>
        public void QuitToMenu()
        {
            // Save before quitting (auto-save)
            SaveGame(0); // Slot 0 is auto-save
            
            Debug.Log("[GameManager] Quitting to menu");
            // Load main menu scene here
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
