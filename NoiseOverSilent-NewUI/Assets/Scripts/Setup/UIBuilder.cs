// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// UPDATED : 2026-03-17 (Added HotspotManager)
// DESC    : Builds the ENTIRE UI from scratch at runtime.
//           Attach ONLY to the GameManager GameObject.
//           No Canvas, no manual Inspector wiring needed.
// ============================================================

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using NoiseOverSilent.Core;
using NoiseOverSilent.Managers;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Setup
{
    /// <summary>
    /// UIBuilder v7.0 - 2026-03-17
    /// Now includes HotspotManager for clickable areas on images
    /// </summary>
    public class UIBuilder : MonoBehaviour
    {
        private HotspotManager hotspotManager;

        private void Awake()
        {
            Debug.Log("[UIBuilder v7.0] Building scene...");

            BuildEventSystem();
            BuildCamera();

            GameObject    canvas      = BuildCanvas();
            Image         bgImage     = BuildBackgroundImage(canvas);
            ImageDisplay  imgDisplay  = BuildImageDisplay(bgImage);
            SlidingPanel  panel       = BuildSlidingPanel(canvas);
            
            // NEW: Build HotspotManager
            BuildHotspotManager(bgImage.GetComponent<RectTransform>(), canvas.transform);

            WireGameManager(imgDisplay, panel);

            Debug.Log("[UIBuilder v7.0] Scene built successfully! (Hotspot system enabled)");
        }

        // ── 1. EVENT SYSTEM ────────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[UIBuilder v7.0] EventSystem created.");
        }

        // ── 2. CAMERA ──────────────────────────────────────────────────────
        private void BuildCamera()
        {
            if (FindFirstObjectByType<Camera>() != null) return;

            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera c = cam.AddComponent<Camera>();
            c.clearFlags      = CameraClearFlags.SolidColor;
            c.backgroundColor = Color.black;
            Debug.Log("[UIBuilder v7.0] Camera created.");
        }

        // ── 3. CANVAS ──────────────────────────────────────────────────────
        private GameObject BuildCanvas()
        {
            GameObject go = new GameObject("Canvas");

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 1f;

            go.AddComponent<GraphicRaycaster>();
            Debug.Log("[UIBuilder v7.0] Canvas created.");
            return go;
        }

        // ── 4. BACKGROUND IMAGE ────────────────────────────────────────────
        private Image BuildBackgroundImage(GameObject canvas)
        {
            GameObject go = new GameObject("BackgroundImage");
            go.transform.SetParent(canvas.transform, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color         = new Color(0.08f, 0.08f, 0.08f, 1f);
            img.raycastTarget = false;

            Debug.Log("[UIBuilder v7.0] BackgroundImage created.");
            return img;
        }

        // ── 4b. IMAGE DISPLAY SCRIPT ───────────────────────────────────────
        private ImageDisplay BuildImageDisplay(Image bgImage)
        {
            ImageDisplay id = bgImage.gameObject.AddComponent<ImageDisplay>();
            SetField(id, "eventImage", bgImage);
            Debug.Log("[UIBuilder v7.0] ImageDisplay created.");
            return id;
        }

        // ── 4c. HOTSPOT MANAGER (NEW!) ─────────────────────────────────────
        private void BuildHotspotManager(RectTransform imageContainer, Transform canvasTransform)
        {
            hotspotManager = gameObject.AddComponent<HotspotManager>();
            hotspotManager.Initialize(imageContainer, canvasTransform);
            Debug.Log("[UIBuilder v7.0] HotspotManager created.");
        }

        // ── 5. SLIDING PANEL ───────────────────────────────────────────────
        private SlidingPanel BuildSlidingPanel(GameObject canvas)
        {
            // -- Panel root --
            GameObject panelGO = new GameObject("SlidingPanel");
            panelGO.transform.SetParent(canvas.transform, false);

            RectTransform panelRect    = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin        = new Vector2(1f, 0f);
            panelRect.anchorMax        = new Vector2(1f, 1f);
            panelRect.pivot            = new Vector2(1f, 0.5f);
            panelRect.sizeDelta        = new Vector2(640f, 0f);
            panelRect.anchoredPosition = new Vector2(740f, 0f); // off-screen

            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

            // -- Narrative text --
            GameObject textGO = new GameObject("NarrativeText");
            textGO.transform.SetParent(panelGO.transform, false);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30f,  220f);
            textRect.offsetMax = new Vector2(-30f, -40f);

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize            = 22;
            tmp.color               = new Color(0.88f, 0.88f, 0.88f, 1f);
            tmp.alignment           = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode    = TMPro.TextWrappingModes.Normal;
            tmp.text                = "";

            // -- Choice container --
            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(panelGO.transform, false);

            RectTransform containerRect    = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin        = new Vector2(0f,   0f);
            containerRect.anchorMax        = new Vector2(1f,   0f);
            containerRect.pivot            = new Vector2(0.5f, 0f);
            containerRect.sizeDelta        = new Vector2(-60f, 210f);
            containerRect.anchoredPosition = new Vector2(0f,   20f);

            VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing               = 12f;
            vlg.padding               = new RectOffset(10, 10, 5, 5);
            vlg.childAlignment        = TextAnchor.LowerLeft;
            vlg.childControlWidth     = true;
            vlg.childControlHeight    = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // -- Choice button prefab (hidden template) --
            GameObject prefab = BuildChoiceButtonPrefab();
            prefab.transform.SetParent(this.transform, false);
            prefab.SetActive(false);

            // -- Wire SlidingPanel script --
            SlidingPanel sp = panelGO.AddComponent<SlidingPanel>();
            SetField(sp, "panelRect",          panelRect);
            SetField(sp, "panelBackground",    panelImg);
            SetField(sp, "narrativeText",      tmp);
            SetField(sp, "choiceContainer",    containerGO.transform);
            SetField(sp, "choiceButtonPrefab", prefab);
            SetField(sp, "slideSpeed",         0.4f);
            SetField(sp, "panelOpacity",       0.88f);

            Debug.Log("[UIBuilder v7.0] SlidingPanel created.");
            return sp;
        }

        // ── 5b. CHOICE BUTTON PREFAB ───────────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 45f);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // transparent background

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Text child
            GameObject textChild = new GameObject("Text");
            textChild.transform.SetParent(go.transform, false);

            RectTransform tRect = textChild.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(5f,  0f);
            tRect.offsetMax = new Vector2(-5f, 0f);

            TextMeshProUGUI tmp = textChild.AddComponent<TextMeshProUGUI>();
            tmp.fontSize            = 19;
            tmp.color               = new Color(0.88f, 0.88f, 0.88f, 1f);
            tmp.alignment           = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode    = TMPro.TextWrappingModes.NoWrap;
            tmp.text                = "";

            go.AddComponent<ChoiceButton>();
            return go;
        }

        // ── 6. WIRE GAME MANAGER ───────────────────────────────────────────
        private void WireGameManager(ImageDisplay imgDisplay, SlidingPanel panel)
        {
            // JsonLoader on same GameObject
            JsonLoader jl = GetComponent<JsonLoader>();
            if (jl == null)
                jl = gameObject.AddComponent<JsonLoader>();

            // GameManager on same GameObject
            GameManager gm = GetComponent<GameManager>();
            if (gm == null)
                gm = gameObject.AddComponent<GameManager>();

            SetField(gm, "jsonLoader",   jl);
            SetField(gm, "imageDisplay", imgDisplay);
            SetField(gm, "slidingPanel", panel);
            SetField(gm, "hotspotManager", hotspotManager); // NEW!
            SetField(gm, "startEpisode", 1);
            SetField(gm, "startEventId", 1);

            Debug.Log("[UIBuilder v7.0] GameManager wired (with HotspotManager).");
        }

        // ── REFLECTION HELPER ──────────────────────────────────────────────
        /// <summary>Sets a private or public SerializeField by name using reflection.</summary>
        private void SetField(object target, string fieldName, object value)
        {
            Type      type  = target.GetType();
            FieldInfo field = null;

            while (type != null && field == null)
            {
                field = type.GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                type = type.BaseType;
            }

            if (field != null)
                field.SetValue(target, value);
            else
                Debug.LogWarning($"[UIBuilder v7.0] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}