// ============================================================
// PROJECT : Noise Over Silence
// FILE    : SlidingPanel.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// VERSION : 1.4
// CHANGES : v1.4 - 2026-02-16 - 2-frame delay coroutine, no MoveOffScreen, force color/alpha
//           v1.3 - 2026-02-16 - Forced panel background color, ForceMeshUpdate
//           v1.2 - 2026-02-16 - Fixed Start() vs ShowEvent() race condition
//           v1.1 - 2026-02-15 - Added ShowEvent(), Hide(callback), ApplyLayout()
//           v1.0 - 2026-02-14 - Initial version
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

        private GameManager      gameManager;
        private List<GameObject> activeButtons = new List<GameObject>();

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        public void ShowEvent(GameEvent gameEvent)
        {
            StopAllCoroutines();
            StartCoroutine(Show(gameEvent));
        }

        private IEnumerator Show(GameEvent gameEvent)
        {
            // Wait 2 frames for everything to settle
            yield return null;
            yield return null;

            // Panel background
            if (panelBackground != null)
                panelBackground.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            // Text
            if (narrativeText != null)
            {
                narrativeText.text  = gameEvent.text;
                narrativeText.color = Color.white;
                narrativeText.alpha = 1f;
                narrativeText.ForceMeshUpdate();
                Debug.Log($"[SlidingPanel v1.4] Text='{narrativeText.text}' Color={narrativeText.color}");
            }
            else
            {
                Debug.LogError("[SlidingPanel v1.4] narrativeText IS NULL");
            }

            // Choices
            BuildChoices(gameEvent.choices);

            // Place panel on screen
            if (panelRect != null)
            {
                panelRect.anchorMin        = new Vector2(1f, 0f);
                panelRect.anchorMax        = new Vector2(1f, 1f);
                panelRect.pivot            = new Vector2(1f, 0.5f);
                panelRect.sizeDelta        = new Vector2(640f, 0f);
                panelRect.anchoredPosition = Vector2.zero;
                Debug.Log($"[SlidingPanel v1.4] Panel pos={panelRect.anchoredPosition} size={panelRect.rect.size}");
            }
        }

        private void BuildChoices(List<Choice> choices)
        {
            foreach (GameObject btn in activeButtons)
                if (btn != null) Destroy(btn);
            activeButtons.Clear();

            if (choices == null || choices.Count == 0 || choiceButtonPrefab == null) return;

            foreach (Choice choice in choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                btnObj.SetActive(true);
                ChoiceButton cb = btnObj.GetComponent<ChoiceButton>();
                if (cb != null)
                {
                    int next = choice.next_event;
                    cb.Setup(choice.text, () => gameManager.MakeChoice(next));
                }
                activeButtons.Add(btnObj);
            }
        }

        public void Hide(Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(SlideOut(onComplete));
        }

        private IEnumerator SlideOut(Action onComplete)
        {
            Vector2 start   = panelRect.anchoredPosition;
            Vector2 end     = new Vector2(740f, 0f);
            float   elapsed = 0f;

            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                panelRect.anchoredPosition = Vector2.Lerp(start, end,
                    Mathf.SmoothStep(0f, 1f, elapsed / slideSpeed));
                yield return null;
            }

            panelRect.anchoredPosition = end;
            onComplete?.Invoke();
        }
    }
}
