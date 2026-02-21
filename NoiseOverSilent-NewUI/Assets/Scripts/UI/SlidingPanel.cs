// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 2.9
// CHANGES : v2.9 - 2026-02-16 - Button_1: full height (5 to -75) | Button_2: top area (60 to -10)
//           v2.9 - 2026-02-16 - Button_1 text limited to top 40px (prevent overlap)
//           v2.8 - 2026-02-16 - Text padding adjusted AFTER Setup (fix click bug)
//           v2.7 - 2026-02-16 - Button_1: left=0, top=5 | Button_2+: left=0, top=70
//           v2.6 - 2026-02-16 - Unique button names (ChoiceButton_1, ChoiceButton_2, etc.)
//           v2.5 - 2026-02-16 - Staggered choice animations
//           v2.4 - 2026-02-16 - Added typewriter effect support
//           v2.3 - 2026-02-16 - SetGameManager() instead of FindFirstObjectByType
//           v2.2 - 2026-02-16 - Added debug logs
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoiseOverSilent.Data;
using NoiseOverSilent.Managers;

namespace NoiseOverSilent.UI
{
    public class SlidingPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform   panelRect;
        [SerializeField] private Image           panelBackground;
        [SerializeField] private TextMeshProUGUI narrativeText;
        [SerializeField] private Transform       choiceContainer;
        [SerializeField] private GameObject      choiceButtonPrefab;
        [SerializeField] private float           slideSpeed = 0.4f;
        [SerializeField] private bool            useTypewriter = true;

        private GameManager      gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();
        private TypewriterEffect typewriter;

        public void SetGameManager(GameManager gm)
        {
            gameManager = gm;
            Debug.Log($"[SlidingPanel v2.4] GameManager set: {(gameManager != null ? gameManager.name : "NULL")}");
        }

        private void Awake()
        {
            if (narrativeText != null && useTypewriter)
            {
                typewriter = narrativeText.gameObject.GetComponent<TypewriterEffect>();
                if (typewriter == null)
                    typewriter = narrativeText.gameObject.AddComponent<TypewriterEffect>();
            }
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            StopAllCoroutines();
            StartCoroutine(Show(gameEvent));
        }

        private IEnumerator Show(GameEvent gameEvent)
        {
            yield return null;
            yield return null;

            if (panelBackground != null)
                panelBackground.color = new Color(0.05f, 0.05f, 0.05f, 0.92f); // Dark grey

            if (narrativeText != null)
            {
                narrativeText.text  = gameEvent.text;
                narrativeText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                narrativeText.alpha = 1f;
                narrativeText.enabled = false;
                narrativeText.enabled = true;
                narrativeText.ForceMeshUpdate();
                
                // Apply typewriter effect
                if (useTypewriter && typewriter != null)
                {
                    typewriter.TypeText(gameEvent.text);
                }
                
                Debug.Log($"[SlidingPanel v2.4] Text shown (typewriter={(useTypewriter ? "ON" : "OFF")})");
            }

            BuildChoices(gameEvent.choices);

            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(1f, 0f);
                panelRect.anchorMax = new Vector2(1f, 1f);
                panelRect.pivot     = new Vector2(1f, 0.5f);
                panelRect.sizeDelta = new Vector2(400f, 0f);

                Vector2 startPos = new Vector2(500f, 0f);
                Vector2 endPos   = Vector2.zero;
                float   elapsed  = 0f;

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
        }

        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0 || choiceButtonPrefab == null)
            {
                return;
            }

            int index = 0;
            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                btnObj.name = $"ChoiceButton_{index + 1}";
                btnObj.SetActive(true);
                
                ChoiceButton cb = btnObj.GetComponent<ChoiceButton>();
                if (cb != null)
                {
                    int next = choice.next_event;
                    cb.Setup(choice.text, () => {
                        if (gameManager != null)
                            gameManager.MakeChoice(next);
                    });
                    
                    // Staggered slide-in animation
                    cb.AnimateEntry(index * 0.1f);
                }
                
                // Adjust text padding AFTER Setup
                TextMeshProUGUI textComponent = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    RectTransform textRect = textComponent.GetComponent<RectTransform>();
                    if (index == 0)
                    {
                        // First button: keep text in top area only
                        textRect.offsetMin = new Vector2(0f, 40f);  // Start 40px from bottom (top half)
                        textRect.offsetMax = new Vector2(-10f, -5f); // End 5px from top
                    }
                    else
                    {
                        // Second+ buttons: text at top
                        textRect.offsetMin = new Vector2(0f, 60f); // Start 60px from bottom
                        textRect.offsetMax = new Vector2(-10f, -10f); // End 10px from top
                    }
                }
                
                activeButtons.Add(btnObj);
                index++;
            }
        }

        public void Hide(Action onComplete)
        {
            StopAllCoroutines();
            
            // Skip typewriter to end before hiding
            if (typewriter != null)
                typewriter.SkipToEnd();
            
            StartCoroutine(SlideOut(onComplete));
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            Vector2 start   = panelRect.anchoredPosition;
            Vector2 end     = new Vector2(500f, 0f);
            float   elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed);
                panelRect.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            panelRect.anchoredPosition = end;
            onComplete?.Invoke();
        }
    }
}