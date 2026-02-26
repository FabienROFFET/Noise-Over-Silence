// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// VERSION : 6.4
// CHANGES : v6.4 - 2026-02-22 - Chapter intro screen
//           v6.3 - 2026-02-22 - Icon support with text fallback
//           v6.2 - 2026-02-22 - Save/Load/Exit system
//           v6.1 - 2026-02-22 - Two-panel layout: text bottom, choices right
//           v6.0 - 2026-02-22 - Tape system disabled, cleanup for narrative focus
//           v5.9 - 2026-02-22 - Added landing page with PLAY button
//           v5.8 - 2026-02-22 - PanelGlow removed, menu matches tape colors
//           v4.7 - 2026-02-21 - Added SoundManager creation
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
            Debug.Log("[UIBuilder v6.4] Building scene...");

            GameObject canvas = BuildCanvas();
            BuildEventSystem();
            
            // Build landing page FIRST (shown on startup)
            BuildLandingPage(canvas);
            
            BuildCamera();
            BuildSoundManager(); // Sound system
            // BuildTapePlayer(); // DISABLED - Tape system removed
            // BuildTapeDeckUI(canvas); // DISABLED - Tape UI removed

            Image        bgImage    = BuildBackgroundImage(canvas);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            BuildVignette(canvas);
            SlidingPanel panel      = BuildSlidingPanel(canvas);
            ChapterIntroScreen chapterIntro = BuildChapterIntro(canvas); // Chapter intro screen
            BuildGameMenu(canvas); // Save/Load/Exit buttons

            WireGameManager(imgDisplay, panel, chapterIntro);

            Debug.Log("[UIBuilder v6.4] Done! (Tape system disabled)");
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
            Debug.Log("[UIBuilder v6.4] Canvas created.");
            return go;
        }

        // ── EventSystem ─────────────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[UIBuilder v6.4] EventSystem created.");
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
            
            Debug.Log("[UIBuilder v6.4] SoundManager created.");
        }

        // ── Tape Player ──────────────────────────────────────────────────────
        private void BuildTapePlayer()
        {
            GameObject go = new GameObject("TapePlayer");
            go.AddComponent<TapePlayer>();
            Debug.Log("[UIBuilder v6.4] TapePlayer created.");
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

            Debug.Log("[UIBuilder v6.4] BackgroundImage created.");
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

            Debug.Log("[UIBuilder v6.4] Vignette overlay created.");
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

            // ═══════════════════════════════════════════════════════════════
            // TEXT PANEL (BOTTOM) - Just narrative text
            // ═══════════════════════════════════════════════════════════════
            
            GameObject textPanelGO = new GameObject("TextPanel");
            textPanelGO.transform.SetParent(panelCanvas.transform, false);

            RectTransform textPanelRect = textPanelGO.AddComponent<RectTransform>();
            textPanelRect.anchorMin        = new Vector2(0f, 0f);      // Bottom-left
            textPanelRect.anchorMax        = new Vector2(1f, 0f);      // Bottom-right
            textPanelRect.pivot            = new Vector2(0.5f, 0f);    // Pivot bottom-center
            textPanelRect.sizeDelta        = new Vector2(0f, 180f);    // Full width, 180px tall
            textPanelRect.anchoredPosition = new Vector2(0f, 0f);      // At bottom edge

            Image textPanelImg = textPanelGO.AddComponent<Image>();
            textPanelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.4f); // Very light (40% opacity)

            // Narrative text (centered in text panel)
            TextMeshProUGUI tmp = CreateTMPText(textPanelGO, "NarrativeText",
                new Vector2(100f, 20f), new Vector2(-100f, -20f), 28);
            tmp.alignment = TextAlignmentOptions.Center; // Centered

            // ═══════════════════════════════════════════════════════════════
            // CHOICE PANEL (RIGHT-TOP) - Just choice buttons
            // ═══════════════════════════════════════════════════════════════
            
            GameObject choicePanelGO = new GameObject("ChoicePanel");
            choicePanelGO.transform.SetParent(panelCanvas.transform, false);

            RectTransform choicePanelRect = choicePanelGO.AddComponent<RectTransform>();
            choicePanelRect.anchorMin        = new Vector2(1f, 1f);     // Right-TOP
            choicePanelRect.anchorMax        = new Vector2(1f, 1f);     // Right-TOP
            choicePanelRect.pivot            = new Vector2(1f, 1f);     // Pivot right-top
            choicePanelRect.sizeDelta        = new Vector2(400f, 600f); // 400px wide, 600px tall
            choicePanelRect.anchoredPosition = new Vector2(450f, 0f);   // Start off-screen (right)

            Image choicePanelImg = choicePanelGO.AddComponent<Image>();
            choicePanelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            // ── Choice container ──────────────────────────────────────────────
            // Inside choice panel - positioned at TOP
            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(choicePanelGO.transform, false);

            RectTransform cr    = containerGO.AddComponent<RectTransform>();
            cr.anchorMin        = new Vector2(0f, 1f);     // Top-left
            cr.anchorMax        = new Vector2(1f, 1f);     // Top-right
            cr.pivot            = new Vector2(0.5f, 1f);   // Pivot at top
            cr.sizeDelta        = new Vector2(-40f, 500f); // Full width minus padding, 500px tall
            cr.anchoredPosition = new Vector2(0f, -20f);   // 20px from top edge

            VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing               = 20f;
            vlg.padding               = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment        = TextAnchor.UpperCenter; // Align to top
            vlg.childControlWidth     = true;
            vlg.childControlHeight    = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // ── Choice button prefab ──────────────────────────────────────────
            GameObject prefab = BuildChoiceButtonPrefab();
            prefab.transform.SetParent(this.transform, false);
            prefab.SetActive(false);

            // ── Wire SlidingPanel script ──────────────────────────────────────
            // Attach to choice panel (left side - the one that slides)
            SlidingPanel sp = choicePanelGO.AddComponent<SlidingPanel>();
            SetField(sp, "panelRect",          choicePanelRect);
            SetField(sp, "panelBackground",    choicePanelImg);
            SetField(sp, "narrativeText",      tmp);              // Text is in bottom panel
            SetField(sp, "choiceContainer",    containerGO.transform);
            SetField(sp, "choiceButtonPrefab", prefab);
            SetField(sp, "slideSpeed",         0.4f);
            
            // Add TypewriterEffect AFTER narrativeText is assigned
            TypewriterEffect typewriter = tmp.gameObject.AddComponent<TypewriterEffect>();
            SetField(sp, "typewriter", typewriter);
            SetField(sp, "useTypewriter", true);
            Debug.Log("[UIBuilder v6.4] TypewriterEffect added to narrativeText!");

            Debug.Log("[UIBuilder v6.4] SlidingPanel created.");
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

            Debug.Log($"[UIBuilder v6.4] TMP '{name}' created. font={tmp.font?.name} mat={tmp.fontSharedMaterial?.name}");
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
                Debug.Log("[UIBuilder v6.4] Cassette player sprite loaded!");
            }
            else
            {
                // Fallback color if sprite not found
                panelImg.color = new Color(0.8f, 0.78f, 0.7f, 1f); // Beige
                Debug.LogWarning("[UIBuilder v6.4] Cassette player sprite not found at Resources/Images/UI/cassette_player");
            }

            // Pull tab on TOP edge - at LEFT side
            GameObject tabGO = new GameObject("PullTab");
            tabGO.transform.SetParent(panelGO.transform, false);

            RectTransform tabRect = tabGO.AddComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(0f, 1f); // Top-left
            tabRect.anchorMax = new Vector2(0f, 1f); // Top-left
            tabRect.pivot = new Vector2(0f, 0f);     // Pivot at bottom-left
            tabRect.sizeDelta = new Vector2(100f, 40f); // Same size as menu
            tabRect.anchoredPosition = new Vector2(0f, 0f); // At left edge, touching border

            Image tabImg = tabGO.AddComponent<Image>();
            tabImg.color = new Color(0.55f, 0.6f, 0.5f, 1f); // Grey-green like cassette tape

            TextMeshProUGUI tabText = CreateTMPText(tabGO, "TabText", 
                new Vector2(5f, 5f), new Vector2(-5f, -5f), 20);
            tabText.text = "TAPE"; // Matches cassette theme
            tabText.alignment = TextAlignmentOptions.Center;
            tabText.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark grey text

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
                    Debug.Log("[UIBuilder v6.4] Pull tab clicked!");
                    deckUI.Toggle();
                });
                Debug.Log("[UIBuilder v6.4] Pull tab manually wired in UIBuilder.");
            }

            Debug.Log("[UIBuilder v6.4] TapeDeckUI created with pixel art cassette player.");
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

        // ── Chapter Intro Screen ─────────────────────────────────────────
        private ChapterIntroScreen BuildChapterIntro(GameObject canvas)
        {
            // Full-screen intro panel
            GameObject introPanel = new GameObject("ChapterIntroPanel");
            introPanel.transform.SetParent(canvas.transform, false);

            RectTransform introRect = introPanel.AddComponent<RectTransform>();
            introRect.anchorMin = Vector2.zero;
            introRect.anchorMax = Vector2.one;
            introRect.offsetMin = Vector2.zero;
            introRect.offsetMax = Vector2.zero;

            // High sorting order (above everything except landing page)
            Canvas introCanvas = introPanel.AddComponent<Canvas>();
            introCanvas.overrideSorting = true;
            introCanvas.sortingOrder = 50; // Above game (10), below landing (100)

            GraphicRaycaster raycaster = introPanel.AddComponent<GraphicRaycaster>();

            // Background image
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(introPanel.transform, false);

            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.black; // Default black

            // Chapter number text (top-center)
            GameObject chapterNumGO = new GameObject("ChapterNumber");
            chapterNumGO.transform.SetParent(introPanel.transform, false);

            RectTransform chapterNumRect = chapterNumGO.AddComponent<RectTransform>();
            chapterNumRect.anchorMin = new Vector2(0.5f, 0.6f);
            chapterNumRect.anchorMax = new Vector2(0.5f, 0.6f);
            chapterNumRect.pivot = new Vector2(0.5f, 0.5f);
            chapterNumRect.sizeDelta = new Vector2(800f, 100f);

            TextMeshProUGUI chapterNumText = CreateTMPText(chapterNumGO, "Text",
                Vector2.zero, Vector2.zero, 48);
            chapterNumText.text = "CHAPTER 1";
            chapterNumText.alignment = TextAlignmentOptions.Center;
            chapterNumText.color = new Color(0.9f, 0.85f, 0.7f, 1f); // Beige
            chapterNumText.fontStyle = FontStyles.Bold;

            // Chapter title text (below number)
            GameObject chapterTitleGO = new GameObject("ChapterTitle");
            chapterTitleGO.transform.SetParent(introPanel.transform, false);

            RectTransform chapterTitleRect = chapterTitleGO.AddComponent<RectTransform>();
            chapterTitleRect.anchorMin = new Vector2(0.5f, 0.4f);
            chapterTitleRect.anchorMax = new Vector2(0.5f, 0.4f);
            chapterTitleRect.pivot = new Vector2(0.5f, 0.5f);
            chapterTitleRect.sizeDelta = new Vector2(1000f, 100f);

            TextMeshProUGUI chapterTitleText = CreateTMPText(chapterTitleGO, "Text",
                Vector2.zero, Vector2.zero, 36);
            chapterTitleText.text = "THE DAY STILL STARTS";
            chapterTitleText.alignment = TextAlignmentOptions.Center;
            chapterTitleText.color = new Color(0.7f, 0.65f, 0.5f, 1f); // Darker beige
            chapterTitleText.fontStyle = FontStyles.Normal;

            // Add ChapterIntroScreen component
            ChapterIntroScreen chapterIntro = introPanel.AddComponent<ChapterIntroScreen>();
            SetField(chapterIntro, "introPanel", introPanel);
            SetField(chapterIntro, "backgroundImage", bgImage);
            SetField(chapterIntro, "chapterNumberText", chapterNumText);
            SetField(chapterIntro, "chapterTitleText", chapterTitleText);

            introPanel.SetActive(false); // Hidden by default

            Debug.Log("[UIBuilder v6.4] Chapter intro screen created.");
            return chapterIntro;
        }

        // ── Game Menu (Save/Load/Exit) ──────────────────────────────────
        private void BuildGameMenu(GameObject canvas)
        {
            // Container for all three buttons - top-left corner
            GameObject menuContainer = new GameObject("GameMenu");
            menuContainer.transform.SetParent(canvas.transform, false);

            RectTransform containerRect = menuContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 1f);
            containerRect.anchorMax = new Vector2(0f, 1f);
            containerRect.pivot = new Vector2(0f, 1f);
            containerRect.sizeDelta = new Vector2(200f, 40f); // Narrower for icon buttons
            containerRect.anchoredPosition = new Vector2(10f, -10f); // 10px padding from corner

            // Horizontal layout for buttons
            HorizontalLayoutGroup layout = menuContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // SAVE button
            CreateGameMenuButton(menuContainer, "SAVE", () => {
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.SaveGame();
                    Debug.Log("[UIBuilder v6.4] Game saved!");
                }
            });

            // LOAD button
            CreateGameMenuButton(menuContainer, "LOAD", () => {
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.LoadSavedGame();
                    Debug.Log("[UIBuilder v6.4] Game loaded!");
                }
            });

            // EXIT button
            CreateGameMenuButton(menuContainer, "EXIT", () => {
                Debug.Log("[UIBuilder v6.4] Exiting game...");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            });

            Debug.Log("[UIBuilder v6.4] Game menu created (Save/Load/Exit).");
        }

        private void CreateGameMenuButton(GameObject parent, string label, System.Action onClick)
        {
            GameObject btnGO = new GameObject(label + "Button");
            btnGO.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(60f, 40f); // Square buttons for icons

            Image btnImg = btnGO.AddComponent<Image>();
            
            // Try to load icon image
            string iconPath = "";
            switch (label)
            {
                case "SAVE":
                    iconPath = "Images/UI/Icons/icon_save";
                    break;
                case "LOAD":
                    iconPath = "Images/UI/Icons/icon_load";
                    break;
                case "EXIT":
                    iconPath = "Images/UI/Icons/icon_exit";
                    break;
            }

            Sprite iconSprite = Resources.Load<Sprite>(iconPath);
            if (iconSprite != null)
            {
                // Use icon image
                btnImg.sprite = iconSprite;
                btnImg.color = Color.white; // Show icon as-is
                btnImg.type = Image.Type.Simple;
                btnImg.preserveAspect = true;
                Debug.Log($"[UIBuilder v6.4] Loaded icon: {iconPath}");
            }
            else
            {
                // Fallback to colored button with letter
                btnImg.color = new Color(0.55f, 0.6f, 0.5f, 1f); // Grey-green
                
                // Add text fallback
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(btnGO.transform, false);

                RectTransform textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                TextMeshProUGUI btnText = CreateTMPText(textGO, "ButtonText", Vector2.zero, Vector2.zero, 24);
                btnText.text = label[0].ToString(); // First letter (S, L, X)
                btnText.alignment = TextAlignmentOptions.Center;
                btnText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                
                Debug.LogWarning($"[UIBuilder v6.4] Icon not found: {iconPath} - using text fallback");
            }

            Button button = btnGO.AddComponent<Button>();
            button.onClick.AddListener(() => onClick());

            // Hover effect
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;
        }

        // ── Wire GameManager ──────────────────────────────────────────────
        private void WireGameManager(ImageDisplay imgDisplay, SlidingPanel panel, ChapterIntroScreen chapterIntro)
        {
            JsonLoader  jl = GetComponent<JsonLoader>()  ?? gameObject.AddComponent<JsonLoader>();
            GameManager gm = GetComponent<GameManager>() ?? gameObject.AddComponent<GameManager>();

            SetField(gm, "jsonLoader",   jl);
            SetField(gm, "imageDisplay", imgDisplay);
            SetField(gm, "slidingPanel", panel);
            SetField(gm, "chapterIntroScreen", chapterIntro);
            SetField(gm, "startEpisode", 1);

            // CRITICAL: Set GameManager reference in SlidingPanel
            panel.SetGameManager(gm);

            Debug.Log("[UIBuilder v6.4] GameManager wired.");
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
                Debug.LogWarning($"[UIBuilder v6.4] Field '{fieldName}' not found on {target.GetType().Name}");
        }

        // ══════════════════════════════════════════════════════════════════
        // LANDING PAGE
        // ══════════════════════════════════════════════════════════════════

        private void BuildLandingPage(GameObject canvas)
        {
            // Landing page container - HIGH PRIORITY (renders on top)
            GameObject landingPage = new GameObject("LandingPage");
            landingPage.transform.SetParent(canvas.transform, false);
            
            RectTransform landingRect = landingPage.AddComponent<RectTransform>();
            landingRect.anchorMin = Vector2.zero;
            landingRect.anchorMax = Vector2.one;
            landingRect.offsetMin = Vector2.zero;
            landingRect.offsetMax = Vector2.zero;
            
            // Add Canvas to landing page for layering control
            Canvas landingCanvas = landingPage.AddComponent<Canvas>();
            landingCanvas.overrideSorting = true;
            landingCanvas.sortingOrder = 100; // On top of everything
            
            GraphicRaycaster raycaster = landingPage.AddComponent<GraphicRaycaster>();
            
            // Background image
            GameObject bgGO = new GameObject("LandingBackground");
            bgGO.transform.SetParent(landingPage.transform, false);
            
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImage = bgGO.AddComponent<Image>();
            
            // Load landing background
            Sprite landingSprite = Resources.Load<Sprite>("Images/UI/landing_background");
            if (landingSprite != null)
            {
                bgImage.sprite = landingSprite;
                bgImage.color = Color.white;
                Debug.Log("[UIBuilder v6.4] Landing background loaded!");
            }
            else
            {
                bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark fallback
                Debug.LogWarning("[UIBuilder v6.4] Landing background not found at Resources/Images/UI/landing_background");
            }
            
            // Button container - vertically stacked
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(landingPage.transform, false);
            
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.35f);
            containerRect.anchorMax = new Vector2(0.5f, 0.35f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(250f, 220f); //Tall enough for 3 buttons
            containerRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = buttonContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            // NEW GAME button
            CreateLandingButton(buttonContainer, "NEW GAME", () => {
                Debug.Log("[UIBuilder v6.4] NEW GAME clicked");
                SoundManager.PlayGameMusic();
                landingPage.SetActive(false);
                
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.LoadEpisode(1, 1); // Start from beginning
                }
            });

            // LOAD button
            CreateLandingButton(buttonContainer, "LOAD", () => {
                Debug.Log("[UIBuilder v6.4] LOAD clicked");
                SoundManager.PlayGameMusic();
                landingPage.SetActive(false);
                
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.LoadSavedGame(); // Load from save
                }
            });

            // EXIT button
            CreateLandingButton(buttonContainer, "EXIT", () => {
                Debug.Log("[UIBuilder v6.4] EXIT clicked");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            });
            
            Debug.Log("[UIBuilder v6.4] Landing page created with NEW GAME/LOAD/EXIT");
        }

        private void CreateLandingButton(GameObject parent, string label, System.Action onClick)
        {
            GameObject btnGO = new GameObject(label + "Button");
            btnGO.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0f, 60f); // Standard button height

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0f, 0f, 0f, 0.7f);

            Button button = btnGO.AddComponent<Button>();
            button.onClick.AddListener(() => onClick());

            // Button text only - no icons
            TextMeshProUGUI btnText = CreateTMPText(btnGO, "Text",
                new Vector2(10f, 10f), new Vector2(-10f, -10f), 28);
            btnText.text = label;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = new Color(0.9f, 0.85f, 0.7f, 1f); // Beige/tan
            btnText.fontStyle = FontStyles.Bold;

            // Hover effect
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0f, 0f, 0f, 0.7f);
            colors.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            button.colors = colors;
        }
    }
}