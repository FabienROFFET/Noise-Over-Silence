// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// VERSION : 1.8
// CHANGES : v1.8 - 2026-02-16 - Use TMP prefab from Resources to fix HDR material issue
//           v1.7 - 2026-02-16 - Fixed HDR intensity=0 on TMP material
//           v1.6 - 2026-02-16 - SlidingPanel on separate Canvas sortingOrder=10
//           v1.5 - 2026-02-16 - Added versioning, fixed Image alpha
//           v1.4 - 2026-02-16 - Fixed RectTransform AddComponent vs GetComponent
//           v1.3 - 2026-02-16 - InputSystemUIInputModule for Unity 6
//           v1.2 - 2026-02-16 - Canvas built first before UI children
//           v1.1 - 2026-02-15 - Fixed field names via reflection
//           v1.0 - 2026-02-14 - Initial version
// ============================================================

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using NoiseOverSilent.Core;
using NoiseOverSilent.Managers;
using NoiseOverSilent.UI;

namespace NoiseOverSilent.Setup
{
    public class UIBuilder : MonoBehaviour
    {
        // Optional: assign a TMP prefab in Inspector to bypass runtime TMP material bugs
        [SerializeField] private GameObject tmpTextPrefab;
        [SerializeField] private GameObject tmpButtonPrefab;

        private void Awake()
        {
            Debug.Log("[UIBuilder v1.8] Building scene...");

            GameObject canvas = BuildCanvas();
            BuildEventSystem();
            BuildCamera();

            Image        bgImage    = BuildBackgroundImage(canvas);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            SlidingPanel panel      = BuildSlidingPanel(canvas);

            WireGameManager(imgDisplay, panel);

            Debug.Log("[UIBuilder v1.8] Done!");
        }

        // ── Canvas ──────────────────────────────────────────────────────────
        private GameObject BuildCanvas()
        {
            GameObject go     = new GameObject("Canvas");
            Canvas     canvas = go.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler       = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            go.AddComponent<GraphicRaycaster>();
            Debug.Log("[UIBuilder v1.8] Canvas created.");
            return go;
        }

        // ── EventSystem ─────────────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[UIBuilder v1.8] EventSystem created.");
        }

        // ── Camera ──────────────────────────────────────────────────────────
        private void BuildCamera()
        {
            if (FindFirstObjectByType<Camera>() != null) return;
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera c      = cam.AddComponent<Camera>();
            c.clearFlags      = CameraClearFlags.SolidColor;
            c.backgroundColor = Color.black;
        }

        // ── Background Image ─────────────────────────────────────────────────
        private Image BuildBackgroundImage(GameObject canvas)
        {
            GameObject go = new GameObject("BackgroundImage");
            go.transform.SetParent(canvas.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img     = go.AddComponent<Image>();
            img.color         = new Color(0.08f, 0.08f, 0.08f, 1f);
            img.raycastTarget = false;

            Debug.Log("[UIBuilder v1.8] BackgroundImage created.");
            return img;
        }

        private ImageDisplay BuildImageDisplay(Image bgImage)
        {
            ImageDisplay id = bgImage.gameObject.AddComponent<ImageDisplay>();
            SetField(id, "eventImage", bgImage);
            return id;
        }

        // ── Sliding Panel ────────────────────────────────────────────────────
        private SlidingPanel BuildSlidingPanel(GameObject canvas)
        {
            // Separate canvas — higher sort order renders on top of background
            GameObject panelCanvas = new GameObject("PanelCanvas");
            Canvas     pc          = panelCanvas.AddComponent<Canvas>();
            pc.renderMode   = RenderMode.ScreenSpaceOverlay;
            pc.sortingOrder = 10;

            CanvasScaler pcs        = panelCanvas.AddComponent<CanvasScaler>();
            pcs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            pcs.referenceResolution = new Vector2(1920, 1080);
            pcs.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            pcs.matchWidthOrHeight  = 1f;
            panelCanvas.AddComponent<GraphicRaycaster>();

            // Panel root
            GameObject    panelGO  = new GameObject("SlidingPanel");
            panelGO.transform.SetParent(panelCanvas.transform, false);

            RectTransform panelRect    = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin        = new Vector2(1f, 0f);
            panelRect.anchorMax        = new Vector2(1f, 1f);
            panelRect.pivot            = new Vector2(1f, 0.5f);
            panelRect.sizeDelta        = new Vector2(640f, 0f);
            panelRect.anchoredPosition = new Vector2(740f, 0f);

            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            // ── Narrative text ────────────────────────────────────────────────
            TextMeshProUGUI tmp = CreateTMPText(panelGO, "NarrativeText",
                new Vector2(30f, 220f), new Vector2(-30f, -40f), 22);

            // ── Choice container ──────────────────────────────────────────────
            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(panelGO.transform, false);

            RectTransform cr    = containerGO.AddComponent<RectTransform>();
            cr.anchorMin        = new Vector2(0f,   0f);
            cr.anchorMax        = new Vector2(1f,   0f);
            cr.pivot            = new Vector2(0.5f, 0f);
            cr.sizeDelta        = new Vector2(-60f, 210f);
            cr.anchoredPosition = new Vector2(0f,   20f);

            VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing               = 12f;
            vlg.padding               = new RectOffset(10, 10, 5, 5);
            vlg.childAlignment        = TextAnchor.LowerLeft;
            vlg.childControlWidth     = true;
            vlg.childControlHeight    = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // ── Choice button prefab ──────────────────────────────────────────
            GameObject prefab = BuildChoiceButtonPrefab();
            prefab.transform.SetParent(this.transform, false);
            prefab.SetActive(false);

            // ── Wire SlidingPanel script ──────────────────────────────────────
            SlidingPanel sp = panelGO.AddComponent<SlidingPanel>();
            SetField(sp, "panelRect",          panelRect);
            SetField(sp, "panelBackground",    panelImg);
            SetField(sp, "narrativeText",      tmp);
            SetField(sp, "choiceContainer",    containerGO.transform);
            SetField(sp, "choiceButtonPrefab", prefab);
            SetField(sp, "slideSpeed",         0.4f);

            Debug.Log("[UIBuilder v1.8] SlidingPanel created.");
            return sp;
        }

        // ── TMP helper — creates text with proper non-HDR material ───────────
        private TextMeshProUGUI CreateTMPText(
            GameObject parent, string name,
            Vector2 offsetMin, Vector2 offsetMax, float fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();

            // Step 1: assign font FIRST
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
                tmp.font = font;

            // Step 2: create a brand-new material instance — bypasses HDR intensity=0 bug
            if (tmp.font != null)
            {
                Material freshMat = new Material(tmp.font.material);
                freshMat.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
                tmp.fontSharedMaterial = freshMat;
            }

            // Step 3: set properties AFTER material
            tmp.fontSize         = fontSize;
            tmp.alignment        = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode     = TextOverflowModes.Overflow;
            tmp.color            = Color.white;
            tmp.text             = "";

            Debug.Log($"[UIBuilder v1.8] TMP '{name}' created. font={tmp.font?.name} mat={tmp.fontSharedMaterial?.name}");
            return tmp;
        }

        // ── Choice Button Prefab ─────────────────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 45f);

            Image img  = go.AddComponent<Image>();
            img.color  = new Color(0f, 0f, 0f, 0f);

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Text child
            TextMeshProUGUI tmp = CreateTMPText(go, "Text",
                new Vector2(5f, 0f), new Vector2(-5f, 0f), 19);
            tmp.alignment        = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            go.AddComponent<ChoiceButton>();
            return go;
        }

        // ── GameManager wiring ───────────────────────────────────────────────
        private void WireGameManager(ImageDisplay imgDisplay, SlidingPanel panel)
        {
            JsonLoader  jl = GetComponent<JsonLoader>()  ?? gameObject.AddComponent<JsonLoader>();
            GameManager gm = GetComponent<GameManager>() ?? gameObject.AddComponent<GameManager>();

            SetField(gm, "jsonLoader",   jl);
            SetField(gm, "imageDisplay", imgDisplay);
            SetField(gm, "slidingPanel", panel);
            SetField(gm, "startEpisode", 1);
            SetField(gm, "startEventId", 1);

            Debug.Log("[UIBuilder v1.8] GameManager wired.");
        }

        // ── Reflection helper ────────────────────────────────────────────────
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
                Debug.LogWarning($"[UIBuilder v1.8] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}