// ============================================================
// PROJECT : Noise Over Silence
// FILE    : UIBuilder.cs
// PATH    : Assets/Scripts/Setup/
// CREATED : 2026-02-14
// VERSION : 6.4
// CHANGES : v6.4 - 2026-03-17 - Replaced dropdown menu with 3 always-visible
//                                icon buttons in top-right corner (Save/Load/Exit)
//           v6.2 - 2026-03-17 - Added ChapterIntroScreen
//           v6.1 - 2026-02-22 - Two-panel layout
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
        [SerializeField] private GameObject tmpTextPrefab;
        [SerializeField] private GameObject tmpButtonPrefab;

        private void Awake()
        {
            Debug.Log("[UIBuilder v6.3] Building scene...");

            GameObject canvas = BuildCanvas();
            BuildEventSystem();

            BuildCamera();
            BuildSoundManager();

            Image        bgImage    = BuildBackgroundImage(canvas);
            ImageDisplay imgDisplay = BuildImageDisplay(bgImage);
            BuildVignette(canvas);

            SlidingPanel       panel        = BuildSlidingPanel(canvas);
            ChapterIntroScreen chapterIntro = BuildChapterIntroScreen(canvas);
            BuildTopRightIcons(canvas);

            // Landing page last — highest sort order, shown on startup
            BuildLandingPage(canvas);

            WireGameManager(imgDisplay, panel, chapterIntro);

            Debug.Log("[UIBuilder v6.3] Done!");
        }

        // ── Canvas ──────────────────────────────────────────────────────────
        private GameObject BuildCanvas()
        {
            GameObject go     = new GameObject("Canvas");
            Canvas     canvas = go.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler        = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0f;

            go.AddComponent<GraphicRaycaster>();
            Debug.Log("[UIBuilder v6.3] Canvas created.");
            return go;
        }

        // ── EventSystem ─────────────────────────────────────────────────────
        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[UIBuilder v6.3] EventSystem created.");
        }

        // ── Camera ──────────────────────────────────────────────────────────
        private void BuildCamera()
        {
            if (FindFirstObjectByType<Camera>() != null) return;
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera c = cam.AddComponent<Camera>();
            c.clearFlags      = CameraClearFlags.SolidColor;
            c.backgroundColor = Color.black;
        }

        // ── Sound Manager ────────────────────────────────────────────────────
        private void BuildSoundManager()
        {
            GameObject go = new GameObject("SoundManager");
            SoundManager sm = go.AddComponent<SoundManager>();

            AudioClip bgMusic  = Resources.Load<AudioClip>("Audio/Music/background_ambient");
            AudioClip btnHover = Resources.Load<AudioClip>("Audio/SFX/button_hover");
            AudioClip btnClick = Resources.Load<AudioClip>("Audio/SFX/button_click");
            AudioClip pSlide   = Resources.Load<AudioClip>("Audio/SFX/panel_slide");
            AudioClip typing   = Resources.Load<AudioClip>("Audio/SFX/typing");

            if (bgMusic  != null) SetField(sm, "backgroundMusic", bgMusic);
            if (btnHover != null) SetField(sm, "buttonHover",     btnHover);
            if (btnClick != null) SetField(sm, "buttonClick",     btnClick);
            if (pSlide   != null) SetField(sm, "panelSlide",      pSlide);
            if (typing   != null) SetField(sm, "typing",          typing);

            Debug.Log("[UIBuilder v6.3] SoundManager created.");
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

            Image img          = go.AddComponent<Image>();
            img.color          = new Color(0.08f, 0.08f, 0.08f, 1f);
            img.raycastTarget  = false;
            img.preserveAspect = false;

            Debug.Log("[UIBuilder v6.3] BackgroundImage created.");
            return img;
        }

        private ImageDisplay BuildImageDisplay(Image bgImage)
        {
            ImageDisplay id = bgImage.gameObject.AddComponent<ImageDisplay>();
            SetField(id, "eventImage", bgImage);
            return id;
        }

        // ── Vignette ─────────────────────────────────────────────────────────
        private void BuildVignette(GameObject canvas)
        {
            GameObject go = new GameObject("VignetteOverlay");
            go.transform.SetParent(canvas.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color         = new Color(0f, 0f, 0f, 0.4f);
            img.raycastTarget = false;
            img.sprite        = CreateVignetteSprite();
            img.type          = Image.Type.Simple;

            go.AddComponent<VignetteEffect>();
        }

        private Sprite CreateVignetteSprite()
        {
            int       size   = 512;
            Texture2D tex    = new Texture2D(size, size);
            Color[]   pixels = new Color[size * size];
            Vector2   center = new Vector2(size / 2f, size / 2f);
            float     maxD   = size * 0.7f;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = Mathf.Pow(Mathf.Clamp01(
                        Vector2.Distance(new Vector2(x, y), center) / maxD - 0.3f), 1.5f);
                    pixels[y * size + x] = new Color(0f, 0f, 0f, a);
                }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        // ── Sliding Panel ────────────────────────────────────────────────────
        private SlidingPanel BuildSlidingPanel(GameObject canvas)
        {
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

            // Text panel (bottom strip)
            GameObject textPanelGO = new GameObject("TextPanel");
            textPanelGO.transform.SetParent(panelCanvas.transform, false);

            RectTransform textPanelRect = textPanelGO.AddComponent<RectTransform>();
            textPanelRect.anchorMin        = new Vector2(0f, 0f);
            textPanelRect.anchorMax        = new Vector2(1f, 0f);
            textPanelRect.pivot            = new Vector2(0.5f, 0f);
            textPanelRect.sizeDelta        = new Vector2(0f, 180f);
            textPanelRect.anchoredPosition = new Vector2(0f, 0f);

            Image textPanelImg = textPanelGO.AddComponent<Image>();
            textPanelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.4f);

            TextMeshProUGUI tmp = CreateTMPText(textPanelGO, "NarrativeText",
                new Vector2(100f, 20f), new Vector2(-100f, -20f), 28);
            tmp.alignment = TextAlignmentOptions.Center;

            // Choice panel (right side)
            GameObject choicePanelGO = new GameObject("ChoicePanel");
            choicePanelGO.transform.SetParent(panelCanvas.transform, false);

            RectTransform choicePanelRect = choicePanelGO.AddComponent<RectTransform>();
            choicePanelRect.anchorMin        = new Vector2(1f, 1f);
            choicePanelRect.anchorMax        = new Vector2(1f, 1f);
            choicePanelRect.pivot            = new Vector2(1f, 1f);
            choicePanelRect.sizeDelta        = new Vector2(400f, 600f);
            choicePanelRect.anchoredPosition = new Vector2(450f, 0f);

            Image choicePanelImg = choicePanelGO.AddComponent<Image>();
            choicePanelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

            GameObject containerGO = new GameObject("ChoiceContainer");
            containerGO.transform.SetParent(choicePanelGO.transform, false);

            RectTransform cr    = containerGO.AddComponent<RectTransform>();
            cr.anchorMin        = new Vector2(0f, 1f);
            cr.anchorMax        = new Vector2(1f, 1f);
            cr.pivot            = new Vector2(0.5f, 1f);
            cr.sizeDelta        = new Vector2(-40f, 500f);
            cr.anchoredPosition = new Vector2(0f, -20f);

            VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing               = 20f;
            vlg.padding               = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment        = TextAnchor.UpperCenter;
            vlg.childControlWidth     = true;
            vlg.childControlHeight    = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            GameObject prefab = BuildChoiceButtonPrefab();
            prefab.transform.SetParent(this.transform, false);
            prefab.SetActive(false);

            SlidingPanel sp = choicePanelGO.AddComponent<SlidingPanel>();
            SetField(sp, "panelRect",          choicePanelRect);
            SetField(sp, "panelBackground",    choicePanelImg);
            SetField(sp, "narrativeText",      tmp);
            SetField(sp, "choiceContainer",    containerGO.transform);
            SetField(sp, "choiceButtonPrefab", prefab);
            SetField(sp, "slideSpeed",         0.4f);

            TypewriterEffect typewriter = tmp.gameObject.AddComponent<TypewriterEffect>();
            SetField(sp, "typewriter",    typewriter);
            SetField(sp, "useTypewriter", true);

            Debug.Log("[UIBuilder v6.3] SlidingPanel created.");
            return sp;
        }

        // ── Chapter Intro Screen ─────────────────────────────────────────────
        private ChapterIntroScreen BuildChapterIntroScreen(GameObject canvas)
        {
            GameObject introGO = new GameObject("ChapterIntroScreen");
            introGO.transform.SetParent(canvas.transform, false);

            // sortingOrder 200 — renders above landing page (100) so it is
            // visible after the landing page hides itself before calling LoadEpisode
            Canvas introCanvas = introGO.AddComponent<Canvas>();
            introCanvas.overrideSorting = true;
            introCanvas.sortingOrder    = 200;
            introGO.AddComponent<GraphicRaycaster>();

            RectTransform introRect = introGO.GetComponent<RectTransform>();
            introRect.anchorMin = Vector2.zero;
            introRect.anchorMax = Vector2.one;
            introRect.offsetMin = Vector2.zero;
            introRect.offsetMax = Vector2.zero;

            // Solid black base — also the CanvasGroup target for fading
            Image baseBg = introGO.AddComponent<Image>();
            baseBg.color         = Color.black;
            baseBg.raycastTarget = true;

            // Optional chapter artwork
            GameObject bgGO = new GameObject("ChapterBg");
            bgGO.transform.SetParent(introGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color          = Color.black;
            bgImg.preserveAspect = false;

            // "CHAPTER X"
            GameObject numGO = new GameObject("ChapterNumber");
            numGO.transform.SetParent(introGO.transform, false);
            RectTransform numRect = numGO.AddComponent<RectTransform>();
            numRect.anchorMin        = new Vector2(0.5f, 0.5f);
            numRect.anchorMax        = new Vector2(0.5f, 0.5f);
            numRect.sizeDelta        = new Vector2(800f, 60f);
            numRect.anchoredPosition = new Vector2(0f, 60f);
            TextMeshProUGUI numTmp = CreateTMPText(numGO, "NumberText",
                Vector2.zero, Vector2.zero, 18);
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color     = new Color(0.55f, 0.55f, 0.55f, 1f);
            numTmp.fontStyle = FontStyles.UpperCase;

            // Chapter title
            GameObject titleGO = new GameObject("ChapterTitle");
            titleGO.transform.SetParent(introGO.transform, false);
            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin        = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax        = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta        = new Vector2(1000f, 80f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);
            TextMeshProUGUI titleTmp = CreateTMPText(titleGO, "TitleText",
                Vector2.zero, Vector2.zero, 38);
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color     = new Color(0.9f, 0.9f, 0.9f, 1f);
            titleTmp.fontStyle = FontStyles.UpperCase;

            ChapterIntroScreen cis = introGO.AddComponent<ChapterIntroScreen>();
            SetField(cis, "introPanel",        introGO);
            SetField(cis, "backgroundImage",   bgImg);
            SetField(cis, "chapterNumberText", numTmp);
            SetField(cis, "chapterTitleText",  titleTmp);
            SetField(cis, "fadeDuration",      0.6f);

            introGO.SetActive(false);

            Debug.Log("[UIBuilder v6.3] ChapterIntroScreen created (sortingOrder 200).");
            return cis;
        }

        // ── Menu (top-left) — Save / Load / Exit with icons ──────────────────
        // Three icon buttons always visible in the top-right corner.
        // Order right-to-left: Exit | Load | Save
        // Each button is 44×44px with 8px gap between them.
        private void BuildTopRightIcons(GameObject canvas)
        {
            Sprite iconSave = Resources.Load<Sprite>("Images/UI/Icons/icon_save");
            Sprite iconLoad = Resources.Load<Sprite>("Images/UI/Icons/icon_load");
            Sprite iconExit = Resources.Load<Sprite>("Images/UI/Icons/icon_exit");

            if (iconSave == null) Debug.LogWarning("[UIBuilder v6.4] icon_save not found at Resources/Images/UI/Icons/icon_save");
            if (iconLoad == null) Debug.LogWarning("[UIBuilder v6.4] icon_load not found at Resources/Images/UI/Icons/icon_load");
            if (iconExit == null) Debug.LogWarning("[UIBuilder v6.4] icon_exit not found at Resources/Images/UI/Icons/icon_exit");

            float btnSize = 44f;
            float gap     = 8f;
            float margin  = 12f;

            // Save — leftmost (slot 0)
            MakeTopRightIconButton(canvas, "SaveButton", iconSave,
                anchorOffsetFromRight: margin,
                () =>
                {
                    SoundManager.PlayButtonClick();
                    if (SaveLoadSystem.SaveExists())
                        Debug.Log("[Menu] Save confirmed — file already up to date.");
                    else
                        Debug.LogWarning("[Menu] No save exists yet — play an event first.");
                });

            // Load — middle (slot 1)
            MakeTopRightIconButton(canvas, "LoadButton", iconLoad,
                anchorOffsetFromRight: margin + (btnSize + gap) * 1,
                () =>
                {
                    SoundManager.PlayButtonClick();
                    GameManager gm = FindFirstObjectByType<GameManager>();
                    if (gm != null)
                    {
                        if (!gm.LoadFromSave())
                            Debug.LogWarning("[Menu] No save file found.");
                    }
                });

            // Exit — slot 2
            MakeTopRightIconButton(canvas, "ExitButton", iconExit,
                anchorOffsetFromRight: margin + (btnSize + gap) * 2,
                () =>
                {
                    SoundManager.PlayButtonClick();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });

            Debug.Log("[UIBuilder v6.4] Top-right icon buttons created (Save / Load / Exit).");
        }

        private void MakeTopRightIconButton(GameObject canvas, string name, Sprite icon,
            float anchorOffsetFromRight, UnityEngine.Events.UnityAction action)
        {
            float btnSize = 44f;
            float margin  = 12f;

            GameObject go = new GameObject(name);
            go.transform.SetParent(canvas.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0f, 1f);
            rect.anchorMax        = new Vector2(0f, 1f);
            rect.pivot            = new Vector2(0f, 1f);
            rect.sizeDelta        = new Vector2(btnSize, btnSize);
            // anchoredPosition: positive-x from left edge, negative-y from top
            rect.anchoredPosition = new Vector2(anchorOffsetFromRight, -margin);

            // Semi-transparent dark background
            Image bg = go.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.7f);

            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(action);

            ColorBlock colors = btn.colors;
            colors.normalColor      = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.7f);
            colors.pressedColor     = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = colors;

            // Icon image — fills the button with small padding
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(8f,  8f);
            iconRect.offsetMax = new Vector2(-8f, -8f);
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite        = icon;
            iconImg.color         = new Color(0.9f, 0.9f, 0.9f, 1f);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
        }

        // ── Landing Page ─────────────────────────────────────────────────────
        private void BuildLandingPage(GameObject canvas)
        {
            GameObject landingPage = new GameObject("LandingPage");
            landingPage.transform.SetParent(canvas.transform, false);

            RectTransform landingRect = landingPage.AddComponent<RectTransform>();
            landingRect.anchorMin = Vector2.zero;
            landingRect.anchorMax = Vector2.one;
            landingRect.offsetMin = Vector2.zero;
            landingRect.offsetMax = Vector2.zero;

            Canvas landingCanvas = landingPage.AddComponent<Canvas>();
            landingCanvas.overrideSorting = true;
            landingCanvas.sortingOrder    = 100;
            landingPage.AddComponent<GraphicRaycaster>();

            GameObject bgGO = new GameObject("LandingBackground");
            bgGO.transform.SetParent(landingPage.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bgGO.AddComponent<Image>();
            Sprite landingSprite = Resources.Load<Sprite>("Images/UI/landing_background");
            if (landingSprite != null) { bgImage.sprite = landingSprite; bgImage.color = Color.white; }
            else bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            GameObject playBtn = new GameObject("PlayButton");
            playBtn.transform.SetParent(landingPage.transform, false);
            RectTransform playRect = playBtn.AddComponent<RectTransform>();
            playRect.anchorMin        = new Vector2(0.5f, 0.35f);
            playRect.anchorMax        = new Vector2(0.5f, 0.35f);
            playRect.pivot            = new Vector2(0.5f, 0.5f);
            playRect.sizeDelta        = new Vector2(200f, 60f);
            playRect.anchoredPosition = Vector2.zero;

            Image playImg = playBtn.AddComponent<Image>();
            playImg.color = new Color(0f, 0f, 0f, 0.7f);

            Button playButton = playBtn.AddComponent<Button>();

            TextMeshProUGUI playText = CreateTMPText(playBtn, "PlayText",
                new Vector2(10f, 10f), new Vector2(-10f, -10f), 32);
            playText.text      = "PLAY";
            playText.alignment = TextAlignmentOptions.Center;
            playText.color     = new Color(0.9f, 0.85f, 0.7f, 1f);
            playText.fontStyle = FontStyles.Bold;

            playButton.onClick.AddListener(() =>
            {
                Debug.Log("[UIBuilder v6.3] PLAY clicked.");
                SoundManager.PlayGameMusic();
                // Hide landing page FIRST, then LoadEpisode so the
                // ChapterIntroScreen (sortingOrder 200) is fully visible
                landingPage.SetActive(false);
                GameManager gm = FindFirstObjectByType<GameManager>();
                gm?.LoadEpisode(1, 1);
            });

            ColorBlock colors = playButton.colors;
            colors.normalColor      = new Color(0f,   0f,   0f,   0.7f);
            colors.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            colors.pressedColor     = new Color(0.1f, 0.1f, 0.1f, 1f);
            colors.selectedColor    = new Color(0f,   0f,   0f,   0.7f);
            playButton.colors       = colors;

            Debug.Log("[UIBuilder v6.3] Landing page created.");
        }

        // ── TMP helper ───────────────────────────────────────────────────────
        private TextMeshProUGUI CreateTMPText(GameObject parent, string name,
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

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
            {
                tmp.font = font;
                Material freshMat = new Material(tmp.font.material);
                freshMat.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
                tmp.fontSharedMaterial = freshMat;
            }

            tmp.fontSize         = fontSize;
            tmp.alignment        = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode     = TextOverflowModes.Overflow;
            tmp.color            = Color.white;
            tmp.text             = "";
            tmp.raycastTarget    = false;
            return tmp;
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
            tmp.text          = text;
            tmp.fontSize      = 16;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.color         = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        // ── Choice Button Prefab ─────────────────────────────────────────────
        private GameObject BuildChoiceButtonPrefab()
        {
            GameObject go = new GameObject("ChoiceButton");
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 80f);

            Image img = go.AddComponent<Image>();
            img.color         = new Color(0f, 0f, 0f, 0.01f);
            img.raycastTarget = true;

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            TextMeshProUGUI tmp = CreateTMPText(go, "Text",
                new Vector2(10f, 5f), new Vector2(-10f, -5f), 19);
            tmp.alignment        = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            go.AddComponent<ChoiceButton>();
            return go;
        }

        // ── Wire GameManager ─────────────────────────────────────────────────
        private void WireGameManager(ImageDisplay imgDisplay, SlidingPanel panel,
                                     ChapterIntroScreen chapterIntro)
        {
            JsonLoader  jl = GetComponent<JsonLoader>()  ?? gameObject.AddComponent<JsonLoader>();
            GameManager gm = GetComponent<GameManager>() ?? gameObject.AddComponent<GameManager>();

            SetField(gm, "jsonLoader",         jl);
            SetField(gm, "imageDisplay",       imgDisplay);
            SetField(gm, "slidingPanel",       panel);
            SetField(gm, "chapterIntroScreen", chapterIntro);
            SetField(gm, "startEpisode",       1);
            SetField(gm, "startEventId",       1);

            panel.SetGameManager(gm);

            Debug.Log("[UIBuilder v6.3] GameManager wired.");
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
                Debug.LogWarning($"[UIBuilder v6.3] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}