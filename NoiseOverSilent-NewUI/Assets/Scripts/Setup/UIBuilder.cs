// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// VERSION : 5.3
// CHANGES : v4.7 - 2026-02-21 - Added SoundManager creation
//           v4.6 - 2026-02-16 - Button Image alpha=0.01 (ensure raycasting works)
//           v4.5 - 2026-02-16 - TMP raycastTarget=false (fix button click blocking)
//           v4.4 - 2026-02-16 - Text TopLeft alignment with 5px padding
//           v4.3 - 2026-02-16 - Button height 80px, spacing 25px, text centered with 15px padding
//           v4.2 - 2026-02-16 - Choices at y=50, text bottom=50
//           v4.1 - 2026-02-16 - Choice button text padding 10px all sides
//           v4.0 - 2026-02-16 - Text area: bottom=100, top=-100 (more space for choices)
//           v3.9 - 2026-02-16 - Choice container y=100 (more space from bottom)
//           v3.8 - 2026-02-16 - Button spacing 20px (fix overlap)
//           v3.7 - 2026-02-16 - Button height 70px, removed PanelGlow (grey panel)
//           v3.6 - 2026-02-16 - Choice button text padding 10px (fix overlap)
//           v3.5 - 2026-02-16 - Premium polish: vignette, panel glow, menu animations
//           v3.4 - 2026-02-16 - Call panel.SetGameManager() to fix choice clicks
//           v3.3 - 2026-02-16 - MENU at (0,0) absolute corner
//           v3.2 - 2026-02-16 - MENU at (5,-5), text top=-50, raycastTarget=true on buttons
//           v3.1 - 2026-02-16 - MENU at (10,-10), text top=-100, choices at y=40
//           v3.0 - 2026-02-16 - Canvas match=0 (width), preserveAspect=false to fill screen
//           v2.9 - 2026-02-16 - Menu dropdown with Exit and Settings at y=-60
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
            Debug.Log("[UIBuilder v5.3] Building scene...");

            GameObject canvas = BuildCanvas();
            BuildEventSystem();
            BuildCamera();
            BuildSoundManager(); // Sound system
            BuildTapePlayer(); // Tape player system

            Image        bgImage    = BuildBackgroundImage(canvas);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            BuildVignette(canvas);
            SlidingPanel panel      = BuildSlidingPanel(canvas);
            BuildTapeDeckUI(canvas); // Tape deck panel
            BuildExitButton(canvas);

            WireGameManager(imgDisplay, panel);

            Debug.Log("[UIBuilder v5.3] Done!");
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
            scaler.matchWidthOrHeight = 0f; // match width to fill screen

            go.AddComponent<GraphicRaycaster>();
            Debug.Log("[UIBuilder v5.3] Canvas created.");
            return go;
        }

        // ── EventSystem ─────────────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[UIBuilder v5.3] EventSystem created.");
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

        // ── Sound Manager ────────────────────────────────────────────────────
        private void BuildSoundManager()
        {
            GameObject go = new GameObject("SoundManager");
            SoundManager sm = go.AddComponent<SoundManager>();
            
            // Load audio from Resources (optional - can also assign in Inspector)
            AudioClip bgMusic = Resources.Load<AudioClip>("Audio/Music/background_ambient");
            AudioClip btnHover = Resources.Load<AudioClip>("Audio/SFX/button_hover");
            AudioClip btnClick = Resources.Load<AudioClip>("Audio/SFX/button_click");
            AudioClip pSlide = Resources.Load<AudioClip>("Audio/SFX/panel_slide");
            AudioClip typing = Resources.Load<AudioClip>("Audio/SFX/typing");
            
            if (bgMusic != null) SetField(sm, "backgroundMusic", bgMusic);
            if (btnHover != null) SetField(sm, "buttonHover", btnHover);
            if (btnClick != null) SetField(sm, "buttonClick", btnClick);
            if (pSlide != null) SetField(sm, "panelSlide", pSlide);
            if (typing != null) SetField(sm, "typing", typing);
            
            Debug.Log("[UIBuilder v5.3] SoundManager created.");
        }

        // ── Tape Player ──────────────────────────────────────────────────────
        private void BuildTapePlayer()
        {
            GameObject go = new GameObject("TapePlayer");
            go.AddComponent<TapePlayer>();
            Debug.Log("[UIBuilder v5.3] TapePlayer created.");
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
            img.preserveAspect = false; // stretch to fill screen

            Debug.Log("[UIBuilder v5.3] BackgroundImage created.");
            return img;
        }

        private ImageDisplay BuildImageDisplay(Image bgImage)
        {
            ImageDisplay id = bgImage.gameObject.AddComponent<ImageDisplay>();
            SetField(id, "eventImage", bgImage);
            return id;
        }

        // ── Vignette Effect ──────────────────────────────────────────────────
        private void BuildVignette(GameObject canvas)
        {
            GameObject vignetteGO = new GameObject("VignetteOverlay");
            vignetteGO.transform.SetParent(canvas.transform, false);

            RectTransform rect = vignetteGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = vignetteGO.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.4f);
            img.raycastTarget = false;
            
            // Radial gradient vignette
            img.sprite = CreateVignetteSprite();
            img.type = Image.Type.Simple;

            vignetteGO.AddComponent<VignetteEffect>();

            Debug.Log("[UIBuilder v5.3] Vignette overlay created.");
        }

        private Sprite CreateVignetteSprite()
        {
            // Create a simple radial gradient texture
            int size = 512;
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size * 0.7f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    float alpha = Mathf.Clamp01((dist / maxDist) - 0.3f);
                    alpha = Mathf.Pow(alpha, 1.5f); // Soften edges
                    pixels[y * size + x] = new Color(0f, 0f, 0f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
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
            pcs.matchWidthOrHeight  = 0f;
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

            // Glow removed - clean dark grey panel

            // ── Narrative text ────────────────────────────────────────────────
            TextMeshProUGUI tmp = CreateTMPText(panelGO, "NarrativeText",
                new Vector2(30f, 50f), new Vector2(-10f, -100f), 30);

            // ── Choice container ──────────────────────────────────────────────
            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(panelGO.transform, false);

            RectTransform cr    = containerGO.AddComponent<RectTransform>();
            cr.anchorMin        = new Vector2(0f,   0f);
            cr.anchorMax        = new Vector2(1f,   0f);
            cr.pivot            = new Vector2(0.5f, 0f);
            cr.sizeDelta        = new Vector2(-60f, 180f);
            cr.anchoredPosition = new Vector2(0f,   50f); // 50px from bottom

            VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing               = 25f; // More space between buttons
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

            Debug.Log("[UIBuilder v5.3] SlidingPanel created.");
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
            tmp.raycastTarget    = false; // Don't block button clicks!

            Debug.Log($"[UIBuilder v5.3] TMP '{name}' created. font={tmp.font?.name} mat={tmp.fontSharedMaterial?.name}");
            return tmp;
        }

        // ── Choice Button Prefab ─────────────────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 80f); // Taller buttons

            Image img  = go.AddComponent<Image>();
            img.color  = new Color(0f, 0f, 0f, 0.01f); // Nearly invisible but raycasts work
            img.raycastTarget = true; // MUST be true for clicks to register

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Text child - top-left aligned
            TextMeshProUGUI tmp = CreateTMPText(go, "Text",
                new Vector2(10f, 5f), new Vector2(-10f, -5f), 19);
            tmp.alignment        = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            go.AddComponent<ChoiceButton>();
            return go;
        }

        // ── Tape Deck UI ─────────────────────────────────────────────────────
        private void BuildTapeDeckUI(GameObject canvas)
        {
            // Tape deck panel on MAIN canvas (not separate)
            GameObject panelGO = new GameObject("TapeDeckPanel");
            panelGO.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f); // Bottom-left corner
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.sizeDelta = new Vector2(600f, 400f); // Size for cassette image
            panelRect.anchoredPosition = new Vector2(0f, -400f); // Start hidden below screen

            Image panelImg = panelGO.AddComponent<Image>();
            
            // Load pixel art cassette player sprite
            Sprite cassetteSprite = Resources.Load<Sprite>("Images/UI/cassette_player");
            if (cassetteSprite != null)
            {
                panelImg.sprite = cassetteSprite;
                panelImg.color = Color.white; // Full color
                Debug.Log("[UIBuilder v5.3] Cassette player sprite loaded!");
            }
            else
            {
                // Fallback color if sprite not found
                panelImg.color = new Color(0.8f, 0.78f, 0.7f, 1f); // Beige
                Debug.LogWarning("[UIBuilder v5.3] Cassette player sprite not found at Resources/Images/UI/cassette_player");
            }

            // Pull tab on TOP edge
            GameObject tabGO = new GameObject("PullTab");
            tabGO.transform.SetParent(panelGO.transform, false);

            RectTransform tabRect = tabGO.AddComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(0.5f, 1f); // Top center
            tabRect.anchorMax = new Vector2(0.5f, 1f);
            tabRect.pivot = new Vector2(0.5f, 0f);
            tabRect.sizeDelta = new Vector2(100f, 40f); // Horizontal tab
            tabRect.anchoredPosition = new Vector2(0f, 0f);

            Image tabImg = tabGO.AddComponent<Image>();
            tabImg.color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red tab

            TextMeshProUGUI tabText = CreateTMPText(tabGO, "TabText", 
                new Vector2(5f, 5f), new Vector2(-5f, -5f), 24);
            tabText.text = "PULL"; // Simple text instead of Unicode
            tabText.alignment = TextAlignmentOptions.Center;
            tabText.color = Color.white;

            Button tabBtn = tabGO.AddComponent<Button>();
            tabBtn.targetGraphic = tabImg;

            // Tape info - positioned over display area of pixel art
            TextMeshProUGUI tapeTitle = CreateTMPText(panelGO, "TapeTitle", 
                new Vector2(150f, 280f), new Vector2(-150f, 230f), 16);
            tapeTitle.alignment = TextAlignmentOptions.Center;
            tapeTitle.color = Color.black; // Black text on cassette display

            TextMeshProUGUI tapeArtist = CreateTMPText(panelGO, "TapeArtist", 
                new Vector2(150f, 230f), new Vector2(-150f, 200f), 13);
            tapeArtist.alignment = TextAlignmentOptions.Center;
            tapeArtist.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // Invisible control buttons over pixel art hotspots
            // Left knob = PREV
            GameObject prevBtn = CreateInvisibleButton(panelGO, "PrevButton", 
                new Vector2(100f, 300f), new Vector2(60f, 60f)); // Left knob position
            
            // RIGHT ARROW on cassette = NEXT (main control)
            GameObject nextBtn = CreateInvisibleButton(panelGO, "NextButton",
                new Vector2(520f, 200f), new Vector2(60f, 60f)); // Arrow on right

            // Center display area = PLAY/PAUSE
            GameObject playBtn = CreateInvisibleButton(panelGO, "PlayButton",
                new Vector2(250f, 250f), new Vector2(120f, 80f)); // Center display
            
            // Bottom buttons area = STOP
            GameObject stopBtn = CreateInvisibleButton(panelGO, "StopButton",
                new Vector2(300f, 100f), new Vector2(80f, 60f)); // Bottom buttons

            // Add TapeDeckUI component
            TapeDeckUI deckUI = panelGO.AddComponent<TapeDeckUI>();
            SetField(deckUI, "panelRect", panelRect);
            SetField(deckUI, "pullTab", tabBtn);
            SetField(deckUI, "tapeTitle", tapeTitle);
            SetField(deckUI, "tapeArtist", tapeArtist);
            SetField(deckUI, "tapeDescription", tapeArtist); // Reuse
            SetField(deckUI, "playButton", playBtn.GetComponent<Button>());
            SetField(deckUI, "stopButton", stopBtn.GetComponent<Button>());
            SetField(deckUI, "nextButton", nextBtn.GetComponent<Button>());
            SetField(deckUI, "prevButton", prevBtn.GetComponent<Button>());
            SetField(deckUI, "closeButton", tabBtn); // Tab toggles
            SetField(deckUI, "playButtonText", tapeTitle); // Will show play state
            
            // Wire up the pull tab AFTER fields are set
            if (tabBtn != null)
            {
                tabBtn.onClick.AddListener(() => {
                    Debug.Log("[UIBuilder v5.3] Pull tab clicked!");
                    deckUI.Toggle();
                });
                Debug.Log("[UIBuilder v5.3] Pull tab manually wired in UIBuilder.");
            }

            Debug.Log("[UIBuilder v5.3] TapeDeckUI created with pixel art cassette player.");
        }

        private GameObject CreateInvisibleButton(GameObject parent, string name, Vector2 position, Vector2 size)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent.transform, false);

            RectTransform rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f); // Invisible
            img.raycastTarget = true;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            
            // Visual feedback on hover
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 0f, 0.2f); // Slight yellow highlight
            colors.pressedColor = new Color(1f, 1f, 0f, 0.4f);
            btn.colors = colors;

            return btnGO;
        }

        private GameObject CreateTapeDeckButton(GameObject parent, string name, Vector2 position, string label)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent.transform, false);

            RectTransform rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(50f, 50f);
            rect.anchoredPosition = position;

            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            btn.colors = colors;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = CreateTMPText(textGO, "ButtonText", Vector2.zero, Vector2.zero, 20);
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;

            return btnGO;
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
            menuRect.anchoredPosition = new Vector2(0f, 0f);

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
            dropRect.sizeDelta        = new Vector2(100f, 80f); // Back to 2 options
            dropRect.anchoredPosition = new Vector2(0f, -40f);

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

            // Toggle dropdown with smooth animation
            menuButton.onClick.AddListener(() => {
                if (dropdown.activeSelf)
                {
                    dropdown.SetActive(false);
                }
                else
                {
                    dropdown.SetActive(true);
                    // Add slight scale animation
                    dropdown.transform.localScale = new Vector3(1f, 0f, 1f);
                    StartCoroutine(AnimateDropdown(dropdown.transform));
                }
            });

            Debug.Log("[UIBuilder v5.3] Menu dropdown created.");
        }

        private System.Collections.IEnumerator AnimateDropdown(Transform dropdown)
        {
            float elapsed = 0f;
            float duration = 0.15f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                dropdown.localScale = new Vector3(1f, t, 1f);
                yield return null;
            }

            dropdown.localScale = Vector3.one;
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

            // CRITICAL: Set GameManager reference in SlidingPanel
            panel.SetGameManager(gm);

            Debug.Log("[UIBuilder v5.3] GameManager wired.");
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
                Debug.LogWarning($"[UIBuilder v5.3] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}