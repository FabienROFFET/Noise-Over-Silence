using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using NoiseOverSilent.Core;
using NoiseOverSilent.Data;
using NoiseOverSilent.Managers;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Setup
{
    /// <summary>
    /// Add ONLY to the GameManager object.
    /// Builds the entire UI from scratch at runtime.
    /// No Canvas, no manual wiring needed.
    /// </summary>
    public class UIBuilder : MonoBehaviour
    {
        private void Awake()
        {
            BuildEventSystem();
            BuildCamera();

            GameObject canvasGO     = BuildCanvas();
            Image      bgImage      = BuildBackgroundImage(canvasGO);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            SlidingPanel panel      = BuildSlidingPanel(canvasGO);

            WireGameManager(imgDisplay, panel);

            Debug.Log("UIBuilder: Done! Scene fully built.");
        }

        // ── EVENT SYSTEM ──────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ── CAMERA ────────────────────────────────────────────────────
        private void BuildCamera()
        {
            if (FindFirstObjectByType<Camera>() != null) return;

            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera c = cam.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = Color.black;
        }

        // ── CANVAS ────────────────────────────────────────────────────
        private GameObject BuildCanvas()
        {
            GameObject go = new GameObject("Canvas");

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution  = new Vector2(1920, 1080);
            scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight   = 1f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        // ── BACKGROUND IMAGE ──────────────────────────────────────────
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
            img.color = new Color(0.08f, 0.08f, 0.08f, 1f);
            img.raycastTarget = false;

            return img;
        }

        // ── IMAGE DISPLAY ─────────────────────────────────────────────
        private ImageDisplay BuildImageDisplay(Image bgImage)
        {
            ImageDisplay id = bgImage.gameObject.AddComponent<ImageDisplay>();
            SetField(id, "eventImage", bgImage);
            return id;
        }

        // ── SLIDING PANEL ─────────────────────────────────────────────
        private SlidingPanel BuildSlidingPanel(GameObject canvas)
        {
            // Panel root
            GameObject panelGO = new GameObject("SlidingPanel");
            panelGO.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot     = new Vector2(1f, 0.5f);
            panelRect.sizeDelta = new Vector2(640f, 0f);
            panelRect.anchoredPosition = new Vector2(740f, 0f); // start off-screen

            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

            // Narrative text
            GameObject textGO = new GameObject("NarrativeText");
            textGO.transform.SetParent(panelGO.transform, false);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30f,  220f);
            textRect.offsetMax = new Vector2(-30f, -40f);

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize         = 22;
            tmp.color            = new Color(0.88f, 0.88f, 0.88f, 1f);
            tmp.alignment        = TextAlignmentOptions.TopLeft;
            tmp.enableWordWrapping = true;
            tmp.text             = "";

            // Choice container
            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(panelGO.transform, false);

            RectTransform containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin        = new Vector2(0f, 0f);
            containerRect.anchorMax        = new Vector2(1f, 0f);
            containerRect.pivot            = new Vector2(0.5f, 0f);
            containerRect.sizeDelta        = new Vector2(-60f, 210f);
            containerRect.anchoredPosition = new Vector2(0f, 20f);

            VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing              = 12f;
            vlg.padding              = new RectOffset(10, 10, 5, 5);
            vlg.childAlignment       = TextAnchor.LowerLeft;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Choice button prefab (hidden template stored on UIBuilder)
            GameObject prefab = BuildChoiceButtonPrefab();
            prefab.transform.SetParent(this.transform, false);
            prefab.SetActive(false);

            // Wire SlidingPanel script
            SlidingPanel sp = panelGO.AddComponent<SlidingPanel>();
            SetField(sp, "panelRect",          panelRect);
            SetField(sp, "panelBackground",    panelImg);
            SetField(sp, "narrativeText",      tmp);
            SetField(sp, "choiceContainer",    containerGO.transform);
            SetField(sp, "choiceButtonPrefab", prefab);
            SetField(sp, "slideSpeed",         0.4f);
            SetField(sp, "panelOpacity",       0.88f);

            return sp;
        }

        // ── CHOICE BUTTON PREFAB ──────────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 45f);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // fully transparent

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Text child
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);

            RectTransform tRect = textGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(5f, 0f);
            tRect.offsetMax = new Vector2(-5f, 0f);

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize           = 19;
            tmp.color              = new Color(0.88f, 0.88f, 0.88f, 1f);
            tmp.alignment          = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = false;
            tmp.text               = "";

            go.AddComponent<ChoiceButton>();

            return go;
        }

        // ── WIRE GAME MANAGER ─────────────────────────────────────────
        private void WireGameManager(ImageDisplay imgDisplay, SlidingPanel panel)
        {
            // Add JsonLoader to this object
            JsonLoader jl = GetComponent<JsonLoader>();
            if (jl == null)
                jl = gameObject.AddComponent<JsonLoader>();

            // Get or add GameManager
            GameManager gm = GetComponent<GameManager>();
            if (gm == null)
                gm = gameObject.AddComponent<GameManager>();

            SetField(gm, "jsonLoader",    jl);
            SetField(gm, "imageDisplay",  imgDisplay);
            SetField(gm, "slidingPanel",  panel);
            SetField(gm, "startEpisode",  1);
            SetField(gm, "startEventId",  1);
        }

        // ── REFLECTION HELPER ─────────────────────────────────────────
        private void SetField(object target, string fieldName, object value)
        {
            Type type = target.GetType();
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
                Debug.LogWarning($"UIBuilder: '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
