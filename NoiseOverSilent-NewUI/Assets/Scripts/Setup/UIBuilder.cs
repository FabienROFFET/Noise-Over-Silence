// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// VERSION : 2.9
// CHANGES : v2.9 - 2026-02-16 - Menu dropdown with Exit and Settings at y=-60
//           v2.8 - 2026-02-16 - EXIT button position adjusted (y=-10 instead of -20)
//           v2.7 - 2026-02-16 - Panel 400px, text right=10, EXIT button upper left
//           v2.6 - 2026-02-16 - Panel width 600px (25% smaller), narrative text size 30
//           v2.5 - 2026-02-16 - Choice button text padding reduced to 1px
//           v2.4 - 2026-02-16 - Canvas Scaler match=0.5 for balanced width/height scaling
//           v2.3 - 2026-02-16 - Background image preserveAspect=true
//           v2.2 - 2026-02-16 - Choice button: height=55, text padding=15, wrapping enabled
//           v2.1 - 2026-02-16 - Text right=-400, bottom=220, choices at y=220
//           v2.0 - 2026-02-16 - Panel 800px wide, text area top=200px bottom=40px
//           v1.9 - 2026-02-16 - Fixed text area bounds (40px bottom instead of 220px)
//           v1.8 - 2026-02-16 - Use TMP prefab from Resources to fix HDR material issue
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
            Debug.Log("[UIBuilder v2.9] Building scene...");

            GameObject canvas = BuildCanvas();
            BuildEventSystem();
            BuildCamera();

            Image        bgImage    = BuildBackgroundImage(canvas);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            SlidingPanel panel      = BuildSlidingPanel(canvas);
            BuildExitButton(canvas);

            WireGameManager(imgDisplay, panel);

            Debug.Log("[UIBuilder v2.9] Done!");
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
            scaler.matchWidthOrHeight = 0.5f; // balanced between width and height

            go.AddComponent<GraphicRaycaster>();
            Debug.Log("[UIBuilder v2.9] Canvas created.");
            return go;
        }

        // ── EventSystem ─────────────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[UIBuilder v2.9] EventSystem created.");
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

            Image img         = go.AddComponent<Image>();
            img.color         = new Color(0.08f, 0.08f, 0.08f, 1f);
            img.raycastTarget = false;
            img.preserveAspect = true;

            Debug.Log("[UIBuilder v2.9] BackgroundImage created.");
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
            pcs.matchWidthOrHeight  = 0.5f;
            panelCanvas.AddComponent<GraphicRaycaster>();

            // Panel root
            GameObject    panelGO  = new GameObject("SlidingPanel");
            panelGO.transform.SetParent(panelCanvas.transform, false);

            RectTransform panelRect    = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin        = new Vector2(1f, 0f);
            panelRect.anchorMax        = new Vector2(1f, 1f);
            panelRect.pivot            = new Vector2(1f, 0.5f);
            panelRect.sizeDelta        = new Vector2(400f, 0f);
            panelRect.anchoredPosition = new Vector2(500f, 0f);

            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            // ── Narrative text ────────────────────────────────────────────────
            TextMeshProUGUI tmp = CreateTMPText(panelGO, "NarrativeText",
                new Vector2(30f, 220f), new Vector2(-10f, -200f), 30);

            // ── Choice container ──────────────────────────────────────────────
            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(panelGO.transform, false);

            RectTransform cr    = containerGO.AddComponent<RectTransform>();
            cr.anchorMin        = new Vector2(0f,   0f);
            cr.anchorMax        = new Vector2(1f,   0f);
            cr.pivot            = new Vector2(0.5f, 0f);
            cr.sizeDelta        = new Vector2(-60f, 180f);
            cr.anchoredPosition = new Vector2(0f,   220f);

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

            Debug.Log("[UIBuilder v2.9] SlidingPanel created.");
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

            Debug.Log($"[UIBuilder v2.9] TMP '{name}' created. font={tmp.font?.name} mat={tmp.fontSharedMaterial?.name}");
            return tmp;
        }

        // ── Choice Button Prefab ─────────────────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 55f);

            Image img  = go.AddComponent<Image>();
            img.color  = new Color(0f, 0f, 0f, 0f);

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Text child
            TextMeshProUGUI tmp = CreateTMPText(go, "Text",
                new Vector2(1f, 5f), new Vector2(-1f, -5f), 19);
            tmp.alignment        = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            go.AddComponent<ChoiceButton>();
            return go;
        }

        // ── Menu Dropdown ────────────────────────────────────────────────
        private void BuildExitButton(GameObject canvas)
        {
            // Menu button
            GameObject menuBtn = new GameObject("MenuButton");
            menuBtn.transform.SetParent(canvas.transform, false);

            RectTransform menuRect = menuBtn.AddComponent<RectTransform>();
            menuRect.anchorMin        = new Vector2(0f, 1f);
            menuRect.anchorMax        = new Vector2(0f, 1f);
            menuRect.pivot            = new Vector2(0f, 1f);
            menuRect.sizeDelta        = new Vector2(100f, 40f);
            menuRect.anchoredPosition = new Vector2(20f, -60f);

            Image menuImg = menuBtn.AddComponent<Image>();
            menuImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            Button menuButton = menuBtn.AddComponent<Button>();

            // Menu button text
            GameObject menuTextGO = new GameObject("Text");
            menuTextGO.transform.SetParent(menuBtn.transform, false);
            
            RectTransform menuTextRect = menuTextGO.AddComponent<RectTransform>();
            menuTextRect.anchorMin = Vector2.zero;
            menuTextRect.anchorMax = Vector2.one;
            menuTextRect.offsetMin = Vector2.zero;
            menuTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI menuText = CreateMenuText(menuTextGO, "MENU");

            // Dropdown panel
            GameObject dropdown = new GameObject("MenuDropdown");
            dropdown.transform.SetParent(canvas.transform, false);
            dropdown.SetActive(false);

            RectTransform dropRect = dropdown.AddComponent<RectTransform>();
            dropRect.anchorMin        = new Vector2(0f, 1f);
            dropRect.anchorMax        = new Vector2(0f, 1f);
            dropRect.pivot            = new Vector2(0f, 1f);
            dropRect.sizeDelta        = new Vector2(100f, 80f);
            dropRect.anchoredPosition = new Vector2(20f, -100f);

            Image dropImg = dropdown.AddComponent<Image>();
            dropImg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

            // Exit option
            CreateMenuOption(dropdown, "Exit", 0, () => {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            });

            // Settings option  
            CreateMenuOption(dropdown, "Settings", 1, () => {
                Debug.Log("[Menu] Settings (not implemented yet)");
            });

            // Toggle dropdown
            menuButton.onClick.AddListener(() => dropdown.SetActive(!dropdown.activeSelf));

            Debug.Log("[UIBuilder v2.9] Menu dropdown created.");
        }

        private TextMeshProUGUI CreateMenuText(GameObject parent, string text)
        {
            TextMeshProUGUI tmp = parent.AddComponent<TextMeshProUGUI>();
            
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                tmp.font = font;
                Material mat = new Material(font.material);
                mat.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
                tmp.fontSharedMaterial = mat;
            }

            tmp.text      = text;
            tmp.fontSize  = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return tmp;
        }

        private void CreateMenuOption(GameObject parent, string label, int index, UnityEngine.Events.UnityAction action)
        {
            GameObject option = new GameObject(label);
            option.transform.SetParent(parent.transform, false);

            RectTransform rect = option.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0f, 1f);
            rect.anchorMax        = new Vector2(1f, 1f);
            rect.pivot            = new Vector2(0.5f, 1f);
            rect.sizeDelta        = new Vector2(0f, 40f);
            rect.anchoredPosition = new Vector2(0f, -index * 40f);

            Image img = option.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0f);

            Button btn = option.AddComponent<Button>();
            btn.onClick.AddListener(action);

            ColorBlock colors = btn.colors;
            colors.normalColor      = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.pressedColor     = new Color(1f, 1f, 1f, 0.3f);
            btn.colors = colors;

            // Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(option.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5f, 0f);
            textRect.offsetMax = new Vector2(-5f, 0f);

            CreateMenuText(textGO, label);
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

            Debug.Log("[UIBuilder v2.9] GameManager wired.");
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
                Debug.LogWarning($"[UIBuilder v2.9] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}