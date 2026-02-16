// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// AUTHOR  : Noise Over Silence Dev Team
// DESC    : Builds the ENTIRE UI from scratch at runtime.
//           Attach ONLY to the GameManager GameObject.
//           No Canvas, no manual Inspector wiring needed.
//           Fixes:
//             - RectTransform: UI objects must be parented to
//               a Canvas BEFORE setting RectTransform values.
//             - Input System: uses InputSystemUIInputModule
//               for Unity 6 compatibility.
// ============================================================

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;   // requires Input System package
using TMPro;
using NoiseOverSilent.Core;
using NoiseOverSilent.Managers;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Setup
{
    public class UIBuilder : MonoBehaviour
    {
        private void Awake()
        {
            // Canvas MUST exist before any UI RectTransform is touched
            GameObject canvas = BuildCanvas();

            BuildEventSystem();
            BuildCamera();

            Image        bgImage    = BuildBackgroundImage(canvas);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            SlidingPanel panel      = BuildSlidingPanel(canvas);

            WireGameManager(imgDisplay, panel);

            Debug.Log("[UIBuilder] Scene built successfully!");
        }

        // ── 1. CANVAS (first — UI RectTransforms need a canvas parent) ──
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
            Debug.Log("[UIBuilder] Canvas created.");
            return go;
        }

        // ── 2. EVENT SYSTEM ───────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            // Unity 6 + New Input System requires InputSystemUIInputModule
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[UIBuilder] EventSystem created.");
        }

        // ── 3. CAMERA ─────────────────────────────────────────────────
        private void BuildCamera()
        {
            if (FindFirstObjectByType<Camera>() != null) return;

            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera c = cam.AddComponent<Camera>();
            c.clearFlags      = CameraClearFlags.SolidColor;
            c.backgroundColor = Color.black;
            Debug.Log("[UIBuilder] Camera created.");
        }

        // ── 4. BACKGROUND IMAGE ───────────────────────────────────────
        private Image BuildBackgroundImage(GameObject canvas)
        {
            GameObject go = new GameObject("BackgroundImage");
            go.transform.SetParent(canvas.transform, false);

            // Explicitly add RectTransform BEFORE accessing it
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color         = new Color(0.08f, 0.08f, 0.08f, 1f);
            img.raycastTarget = false;
            // Force color again — Image alpha resets to 0 on AddComponent
            img.color = new Color(0.08f, 0.08f, 0.08f, 1f);

            Debug.Log("[UIBuilder] BackgroundImage created.");
            return img;
        }

        // ── 4b. IMAGE DISPLAY SCRIPT ──────────────────────────────────
        private ImageDisplay BuildImageDisplay(Image bgImage)
        {
            ImageDisplay id = bgImage.gameObject.AddComponent<ImageDisplay>();
            SetField(id, "eventImage", bgImage);
            return id;
        }

        // ── 5. SLIDING PANEL ──────────────────────────────────────────
        private SlidingPanel BuildSlidingPanel(GameObject canvas)
        {
            // -- Panel root (parent to canvas first!) --
            GameObject panelGO = new GameObject("SlidingPanel");
            panelGO.transform.SetParent(canvas.transform, false);

            RectTransform panelRect    = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin        = new Vector2(1f, 0f);
            panelRect.anchorMax        = new Vector2(1f, 1f);
            panelRect.pivot            = new Vector2(1f, 0.5f);
            panelRect.sizeDelta        = new Vector2(640f, 0f);
            panelRect.anchoredPosition = new Vector2(740f, 0f);

            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

            // -- Narrative text --
            GameObject textGO = new GameObject("NarrativeText");
            textGO.transform.SetParent(panelGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30f,  220f);
            textRect.offsetMax = new Vector2(-30f, -40f);

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize          = 22;
            tmp.alignment         = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode  = TextWrappingModes.Normal;
            tmp.text              = "";
            tmp.color             = new Color(0.88f, 0.88f, 0.88f, 1f);

            // CanvasGroup overrides TMP material alpha issues on runtime-created objects
            CanvasGroup cg = textGO.AddComponent<CanvasGroup>();
            cg.alpha          = 1f;
            cg.interactable   = true;
            cg.blocksRaycasts = false;

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

            // -- Choice button prefab (hidden template on GameManager) --
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

            Debug.Log("[UIBuilder] SlidingPanel created.");
            return sp;
        }

        // ── 5b. CHOICE BUTTON PREFAB ──────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");

            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 45f);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            GameObject textChild = new GameObject("Text");
            textChild.transform.SetParent(go.transform, false);

            RectTransform tRect = textChild.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(5f,  0f);
            tRect.offsetMax = new Vector2(-5f, 0f);

            TextMeshProUGUI tmp = textChild.AddComponent<TextMeshProUGUI>();
            tmp.fontSize         = 19;
            tmp.alignment        = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.text             = "";
            tmp.color            = new Color(0.88f, 0.88f, 0.88f, 1f); // set LAST

            go.AddComponent<ChoiceButton>();
            return go;
        }

        // ── 6. WIRE GAME MANAGER ──────────────────────────────────────
        private void WireGameManager(ImageDisplay imgDisplay, SlidingPanel panel)
        {
            JsonLoader jl = GetComponent<JsonLoader>();
            if (jl == null)
                jl = gameObject.AddComponent<JsonLoader>();

            GameManager gm = GetComponent<GameManager>();
            if (gm == null)
                gm = gameObject.AddComponent<GameManager>();

            SetField(gm, "jsonLoader",   jl);
            SetField(gm, "imageDisplay", imgDisplay);
            SetField(gm, "slidingPanel", panel);
            SetField(gm, "startEpisode", 1);
            SetField(gm, "startEventId", 1);

            Debug.Log("[UIBuilder] GameManager wired.");
        }

        // ── REFLECTION HELPER ─────────────────────────────────────────
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
                Debug.LogWarning($"[UIBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}