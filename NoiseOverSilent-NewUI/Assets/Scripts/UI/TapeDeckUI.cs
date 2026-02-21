// ============================================================
// PROJECT : Noise Over Silence
// FILE    : TapeDeckUI.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-21
// VERSION : 1.4
// CHANGES : v1.3 - 2026-02-21 - Added debug logs and Start() to ensure hidden state
//           v1.2 - 2026-02-21 - Cassette slides from BOTTOM-LEFT, arrow on cassette = next tape
//           v1.1 - 2026-02-21 - Pull tab from RIGHT side with pixel art support
//           v1.0 - 2026-02-21 - Initial version
// DESC    : 1980s-style tape deck UI with sliding panel
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoiseOverSilent.Managers;
using NoiseOverSilent.Data;

namespace NoiseOverSilent.UI
{
    public class TapeDeckUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private Button pullTab; // Pull tab to open/close
        [SerializeField] private TextMeshProUGUI tapeTitle;
        [SerializeField] private TextMeshProUGUI tapeArtist;
        [SerializeField] private TextMeshProUGUI tapeDescription;
        [SerializeField] private Button playButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI playButtonText;

        [Header("Animation")]
        [SerializeField] private float slideSpeed = 0.4f;

        private bool isOpen = false;
        private TapePlayer tapePlayer;

        private void Awake()
        {
            tapePlayer = TapePlayer.Instance;
            
            // Wire up buttons
            if (pullTab != null)
            {
                pullTab.onClick.AddListener(Toggle); // Pull tab toggles open/close
                Debug.Log("[TapeDeckUI v1.3] Pull tab button wired!");
            }
            else
            {
                Debug.LogError("[TapeDeckUI v1.3] Pull tab is NULL!");
            }
            
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayPauseClicked);
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopClicked);
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (prevButton != null)
                prevButton.onClick.AddListener(OnPrevClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void Start()
        {
            // Start hidden below screen
            if (panelRect != null)
            {
                panelRect.anchoredPosition = new Vector2(0f, -400f);
                Debug.Log("[TapeDeckUI v1.3] Panel hidden at start: " + panelRect.anchoredPosition);
            }
            isOpen = false;
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (tapePlayer == null) return;

            Tape current = tapePlayer.CurrentTape;
            
            if (current != null)
            {
                if (tapeTitle != null) tapeTitle.text = current.title;
                if (tapeArtist != null) tapeArtist.text = current.artist;
                if (tapeDescription != null) tapeDescription.text = current.description;
            }
            else
            {
                if (tapeTitle != null) tapeTitle.text = "No Tape";
                if (tapeArtist != null) tapeArtist.text = "---";
                if (tapeDescription != null) tapeDescription.text = "Insert a tape to play";
            }

            // Update play button text
            if (playButtonText != null)
            {
                playButtonText.text = tapePlayer.IsPlaying ? "||" : ">";
            }
        }

        public void Open()
        {
            if (isOpen) return;
            Debug.Log("[TapeDeckUI v1.3] Opening cassette player...");
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(SlideIn());
        }

        public void Close()
        {
            if (!isOpen) return;
            Debug.Log("[TapeDeckUI v1.3] Closing cassette player...");
            isOpen = false;
            StopAllCoroutines();
            StartCoroutine(SlideOut());
        }

        public void Toggle()
        {
            Debug.Log($"[TapeDeckUI v1.3] Toggle called! isOpen={isOpen}");
            if (isOpen) Close();
            else Open();
        }

        private IEnumerator SlideIn()
        {
            Vector2 startPos = new Vector2(0f, -400f); // Hidden below screen
            Vector2 endPos = new Vector2(0f, 0f); // Slide up to bottom-left
            float elapsed = 0f;

            panelRect.anchoredPosition = startPos;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = endPos;
        }

        private IEnumerator SlideOut()
        {
            Vector2 startPos = panelRect.anchoredPosition;
            Vector2 endPos = new Vector2(0f, -400f); // Slide back down
            float elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = endPos;
        }

        private void OnPlayPauseClicked()
        {
            if (tapePlayer == null) return;

            if (tapePlayer.IsPlaying)
            {
                tapePlayer.Pause();
            }
            else
            {
                if (tapePlayer.CurrentTape != null)
                {
                    tapePlayer.Resume();
                }
                else
                {
                    // Play first unlocked tape
                    for (int i = 0; i < tapePlayer.AllTapes.Count; i++)
                    {
                        if (tapePlayer.AllTapes[i].isUnlocked)
                        {
                            tapePlayer.PlayTape(i);
                            break;
                        }
                    }
                }
            }

            SoundManager.PlayButtonClick();
        }

        private void OnStopClicked()
        {
            if (tapePlayer != null)
            {
                tapePlayer.Stop();
                SoundManager.PlayButtonClick();
            }
        }

        private void OnNextClicked()
        {
            if (tapePlayer != null)
            {
                tapePlayer.NextTape();
                SoundManager.PlayButtonClick();
            }
        }

        private void OnPrevClicked()
        {
            if (tapePlayer != null)
            {
                tapePlayer.PreviousTape();
                SoundManager.PlayButtonClick();
            }
        }
    }
}