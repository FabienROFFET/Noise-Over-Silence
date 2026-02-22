// ============================================================
// PROJECT : Noise Over Silence
// FILE    : LandingPage.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-21
// VERSION : 1.0
// CHANGES : v1.0 - 2026-02-21 - Initial landing page with language selection
// DESC    : Landing page with New Game, Load Episode, Language selection
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoiseOverSilent.Managers;

namespace NoiseOverSilent.UI
{
    public class LandingPage : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject landingCanvas;
        [SerializeField] private Image backgroundImage;
        
        [Header("Settings")]
        [SerializeField] private string selectedLanguage = "en"; // Default: English
        
        private GameManager gameManager;

        private void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            
            // Load saved language preference
            selectedLanguage = PlayerPrefs.GetString("Language", "en");
            
            Debug.Log($"[LandingPage v1.0] Initialized with language: {selectedLanguage}");
        }

        public void OnNewGame()
        {
            Debug.Log($"[LandingPage v1.0] Starting new game in {selectedLanguage}");
            
            // Save language preference
            PlayerPrefs.SetString("Language", selectedLanguage);
            PlayerPrefs.Save();
            
            // Start episode 1
            if (gameManager != null)
            {
                gameManager.LoadEpisode(1, 1);
            }
            
            // Hide landing page
            if (landingCanvas != null)
                landingCanvas.SetActive(false);
        }

        public void OnLoadEpisode()
        {
            Debug.Log($"[LandingPage v1.0] Load episode (not implemented yet)");
            // TODO: Show episode selection screen
        }

        public void OnSelectLanguage(string language)
        {
            selectedLanguage = language;
            Debug.Log($"[LandingPage v1.0] Language changed to: {language}");
            
            // Update UI to show selected language
            // TODO: Highlight selected language button
        }

        public void SetBackgroundImage(Sprite sprite)
        {
            if (backgroundImage != null && sprite != null)
            {
                backgroundImage.sprite = sprite;
            }
        }
    }
}