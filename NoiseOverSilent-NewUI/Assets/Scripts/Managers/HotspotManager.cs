// ============================================================
// PROJECT : Noise Over Silence
// FILE    : HotspotManager.cs
// PATH    : Assets/Scripts/Managers/
// CREATED : 2026-03-17
// VERSION : 2.0
// CHANGES : v2.0 - 2026-03-17
//           - Two hotspot types: "text" (popup) and "choice" (navigate)
//           - Visual distinction: blue circle = text, amber circle = choice
//           - GameManager reference added — choice hotspots call ShowEvent()
//           - HidePopup() / ClearHotspots() unchanged for compatibility
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NoiseOverSilent.Managers;

namespace NoiseOverSilent.UI
{
    public class HotspotManager : MonoBehaviour
    {
        // Set by UIBuilder via SetGameManager()
        private GameManager gameManager;

        private RectTransform imageContainer;
        private GameObject textHotspotPrefab;
        private GameObject choiceHotspotPrefab;
        private GameObject textPopup;
        private TextMeshProUGUI popupText;
        private List<GameObject> activeHotspots = new List<GameObject>();

        // Colors
        private static readonly Color TextColor   = new Color(0.22f, 0.65f, 1f,  0.65f); // blue
        private static readonly Color ChoiceColor = new Color(0.93f, 0.62f, 0.12f, 0.8f); // amber

        public void SetGameManager(GameManager gm)
        {
            gameManager = gm;
            Debug.Log("[HotspotManager v2.0] GameManager set.");
        }

        public void Initialize(RectTransform imageContainerRect, Transform canvasTransform)
        {
            imageContainer = imageContainerRect;
            textHotspotPrefab   = CreateHotspotPrefab("TextHotspotPrefab",   canvasTransform, TextColor);
            choiceHotspotPrefab = CreateHotspotPrefab("ChoiceHotspotPrefab", canvasTransform, ChoiceColor);
            CreateTextPopup(canvasTransform);
            Debug.Log("[HotspotManager v2.0] Initialized.");
        }

        // ── PREFABS ────────────────────────────────────────────────────────
        private GameObject CreateHotspotPrefab(string prefabName, Transform parent, Color color)
        {
            GameObject go = new GameObject(prefabName);
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40, 40);

            Image circle = go.AddComponent<Image>();
            circle.sprite       = CreateCircleSprite();
            circle.color        = color;
            circle.raycastTarget = true;

            go.AddComponent<Button>();
            go.AddComponent<HotspotPulse>();
            return go;
        }

        private void CreateTextPopup(Transform parent)
        {
            textPopup = new GameObject("HotspotTextPopup");
            textPopup.transform.SetParent(parent, false);

            RectTransform rect = textPopup.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420, 160);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            Image bg = textPopup.AddComponent<Image>();
            bg.color         = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            bg.raycastTarget = false; // popup is display-only, no click needed

            // Main text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textPopup.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15f, 28f);
            textRect.offsetMax = new Vector2(-15f, -12f);
            popupText = textObj.AddComponent<TextMeshProUGUI>();
            popupText.fontSize         = 14;
            popupText.color            = new Color(0.78f, 0.82f, 0.85f, 1f);
            popupText.alignment        = TextAlignmentOptions.TopLeft;
            popupText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            popupText.raycastTarget    = false;

            textPopup.SetActive(false);
        }

        // ── PUBLIC API ─────────────────────────────────────────────────────
        public void ShowHotspots(List<Data.Hotspot> hotspots)
        {
            ClearHotspots();

            if (hotspots == null || hotspots.Count == 0)
            {
                Debug.Log("[HotspotManager v2.0] No hotspots for this event.");
                return;
            }

            foreach (var hotspot in hotspots)
                CreateHotspot(hotspot);

            int textCount   = hotspots.FindAll(h => h.type != "choice").Count;
            int choiceCount = hotspots.FindAll(h => h.type == "choice").Count;
            Debug.Log($"[HotspotManager v2.0] Showing {hotspots.Count} hotspot(s) — {textCount} text, {choiceCount} choice.");
        }

        public void HidePopup()
        {
            StopAllCoroutines();
            textPopup.SetActive(false);
        }

        public void ClearHotspots()
        {
            foreach (var h in activeHotspots)
                if (h != null) Destroy(h);
            activeHotspots.Clear();
            HidePopup();
        }

        // ── INTERNALS ──────────────────────────────────────────────────────
        private void CreateHotspot(Data.Hotspot data)
        {
            bool isChoice = (data.type == "choice");
            GameObject prefab = isChoice ? choiceHotspotPrefab : textHotspotPrefab;

            GameObject hotspot = Instantiate(prefab, imageContainer);
            hotspot.name       = $"Hotspot_{data.type}_{data.x}_{data.y}";
            hotspot.SetActive(true);

            RectTransform rect = hotspot.GetComponent<RectTransform>();
            rect.anchorMin        = new Vector2(data.x, data.y);
            rect.anchorMax        = new Vector2(data.x, data.y);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta        = new Vector2(data.radius * 2, data.radius * 2);

            // Choice hotspots: label floats ABOVE the circle
            if (isChoice)
            {
                GameObject labelGO = new GameObject("Label");
                labelGO.transform.SetParent(hotspot.transform, false);
                RectTransform lr = labelGO.AddComponent<RectTransform>();
                // Position label above the circle: centred horizontally, sits above top edge
                float circleSize = data.radius * 2;
                lr.anchorMin        = new Vector2(0.5f, 1f);
                lr.anchorMax        = new Vector2(0.5f, 1f);
                lr.pivot            = new Vector2(0.5f, 0f);
                lr.sizeDelta        = new Vector2(120f, 26f);
                lr.anchoredPosition = new Vector2(0f, 6f); // 6px gap above circle top
                TextMeshProUGUI lbl = labelGO.AddComponent<TextMeshProUGUI>();
                lbl.text             = data.text;
                lbl.fontSize         = 12;
                lbl.color            = new Color(1f, 0.95f, 0.85f, 1f);
                lbl.alignment        = TextAlignmentOptions.Center;
                lbl.textWrappingMode = TMPro.TextWrappingModes.Normal;
                lbl.fontStyle        = FontStyles.Bold;
                lbl.raycastTarget    = false;
            }

            Button btn = hotspot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (isChoice)
            {
                int targetEvent  = data.next_event;
                string label     = data.text;
                GameObject self  = hotspot;
                btn.onClick.AddListener(() =>
                {
                    StartCoroutine(PunchAndNavigate(self.transform, targetEvent, label));
                });
            }
            else
            {
                // Text hotspot: show popup on hover, hide on exit
                string capturedText = data.text;
                RectTransform capturedRect = rect;
                HotspotHover hover = hotspot.AddComponent<HotspotHover>();
                hover.Init(
                    onEnter: () => OnTextHotspotEnter(capturedText, capturedRect),
                    onExit:  HidePopup
                );
            }

            activeHotspots.Add(hotspot);
        }

        private void OnTextHotspotEnter(string text, RectTransform hotspotRect)
        {
            Debug.Log($"[HotspotManager v2.1] Text hover: {text.Substring(0, Mathf.Min(50, text.Length))}...");

            popupText.text = text;

            RectTransform popupRect = textPopup.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            hotspotRect.GetWorldCorners(corners);
            Vector2 hotspotWorldPos = (corners[0] + corners[2]) / 2;
            popupRect.position = (Vector3)hotspotWorldPos + new Vector3(0, 130, 0);

            // Clamp to screen
            Canvas.ForceUpdateCanvases();
            Vector3[] popupCorners = new Vector3[4];
            popupRect.GetWorldCorners(popupCorners);
            float sw = Screen.width, sh = Screen.height;
            if (popupCorners[2].x > sw)  popupRect.position += new Vector3(sw - popupCorners[2].x - 10, 0, 0);
            if (popupCorners[0].x < 0)   popupRect.position += new Vector3(-popupCorners[0].x + 10, 0, 0);
            if (popupCorners[2].y > sh)  popupRect.position += new Vector3(0, sh - popupCorners[2].y - 10, 0);
            if (popupCorners[0].y < 0)   popupRect.position += new Vector3(0, -popupCorners[0].y + 10, 0);

            textPopup.transform.SetAsLastSibling();
            textPopup.SetActive(true);
        }

        // Scale-punch on click: grows to 1.4x then shrinks to 0, then navigates
        private IEnumerator PunchAndNavigate(Transform target, int targetEvent, string label)
        {
            float elapsed  = 0f;
            float growTime = 0.08f;
            float shrinkTime = 0.12f;

            // Grow
            while (elapsed < growTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / growTime;
                target.localScale = Vector3.one * Mathf.Lerp(1f, 1.45f, t);
                yield return null;
            }

            // Shrink to zero
            elapsed = 0f;
            while (elapsed < shrinkTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / shrinkTime;
                target.localScale = Vector3.one * Mathf.Lerp(1.45f, 0f, t);
                yield return null;
            }

            target.localScale = Vector3.zero;
            OnChoiceHotspotClicked(targetEvent, label);
        }

        private void OnChoiceHotspotClicked(int targetEvent, string label)
        {
            Debug.Log($"[HotspotManager v2.0] Choice hotspot clicked: '{label}' → Event {targetEvent}");

            if (gameManager == null)
            {
                Debug.LogError("[HotspotManager v2.0] GameManager is null — cannot navigate!");
                return;
            }

            HidePopup();
            gameManager.ShowEvent(targetEvent);
        }

        // ── CIRCLE SPRITE ──────────────────────────────────────────────────
        private Sprite CreateCircleSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels    = new Color[size * size];
            Vector2 center    = new Vector2(size / 2f, size / 2f);
            float   radius    = size / 2f - 2f;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = Vector2.Distance(new Vector2(x, y), center) < radius
                        ? Color.white : Color.clear;

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }

    // ── HOTSPOT PULSE ──────────────────────────────────────────────────────
    public class HotspotPulse : MonoBehaviour
    {
        private Image image;
        private float timer = 0f;
        private float speed = 2f;

        void Start()
        {
            image = GetComponent<Image>();
            if (image == null) { enabled = false; return; }
        }

        void Update()
        {
            if (image == null) return;
            timer += Time.deltaTime * speed;
            float alpha = Mathf.Lerp(0.3f, 0.85f, (Mathf.Sin(timer) + 1f) / 2f);
            Color color = image.color;
            color.a     = alpha;
            image.color = color;
        }
    }

    // ── HOTSPOT HOVER ──────────────────────────────────────────────────────
    // Fires onEnter when the pointer enters the circle, onExit when it leaves.
    // Attached only to text hotspots — choice hotspots use Button.onClick instead.
    public class HotspotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private System.Action onEnter;
        private System.Action onExit;

        public void Init(System.Action onEnter, System.Action onExit)
        {
            this.onEnter = onEnter;
            this.onExit  = onExit;
        }

        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData)  => onExit?.Invoke();
    }
}