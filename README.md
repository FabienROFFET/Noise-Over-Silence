# 🎮 Noise Over Silence - Complete Project Structure

## 📂 Full Directory Tree

```
Assets/
│
├── Resources/
│   ├── Audio/
│   │   ├── Music/
│   │   │   ├── background_ambient.mp3
│   │   │   └── landing_music.mp3          ← NEW: Landing page music
│   │   │
│   │   ├── SFX/
│   │   │   ├── button_click.wav
│   │   │   ├── button_hover.wav
│   │   │   ├── panel_slide.wav
│   │   │   └── typing.wav
│   │   │
│   │   ├── Tapes/
│   │   │   ├── tape_01.mp3
│   │   │   ├── tape_02.mp3
│   │   │   └── tape_03.mp3
│   │   │
│   │   ├── Soundscapes/
│   │   │   ├── ambient_city.mp3
│   │   │   ├── ambient_interior.mp3
│   │   │   └── ambient_wind.mp3
│   │   │
│   │   └── VoiceOvers/                    ← NEW: Voice over audio
│   │       └── vo/
│   │           ├── ep1_event1_en.mp3
│   │           ├── ep1_event1_cs.mp3
│   │           ├── ep1_event1_pl.mp3
│   │           ├── ep1_event1_fr.mp3
│   │           ├── ep1_event2_en.mp3
│   │           └── ... (84 files total: 21 events × 4 languages)
│   │
│   └── Images/
│       ├── Events/
│       │   ├── ep1_event1.jpg
│       │   ├── ep1_event2.png
│       │   ├── ep1_event3.png
│       │   └── ... (21 event images)
│       │
│       └── UI/
│           ├── cassette_player.png         ← Pixel art cassette player
│           └── landing_background.png      ← Landing page background
│
├── StreamingAssets/
│   └── Episodes/
│       ├── episode01_en.json              ← English (COMPLETE)
│       ├── episode01_cs.json              ← Czech (COMPLETE)
│       ├── episode01_pl.json              ← Polish (placeholder)
│       └── episode01_fr.json              ← French (placeholder)
│
└── Scripts/
    │
    ├── Core/
    │   └── JsonLoader.cs                  v1.1
    │
    ├── Data/
    │   └── GameData.cs                    v1.2 (NEW: voice_over field)
    │
    ├── Managers/
    │   ├── GameManager.cs                 v1.3 (NEW: voice over playback)
    │   ├── SoundManager.cs                v1.1 (NEW: VO + landing music)
    │   └── TapePlayer.cs                  v1.0
    │
    ├── Setup/
    │   └── UIBuilder.cs                   v5.8 (PanelGlow removed, menu matches tape)
    │
    └── UI/
        ├── ChoiceButton.cs                v3.2
        ├── ImageDisplay.cs                v1.1
        ├── LandingPage.cs                 v1.0 (NEW)
        ├── PanelGlow.cs                   v1.3 (NOT USED - removed from UIBuilder)
        ├── SlidingPanel.cs                v3.6
        ├── TapeDeckUI.cs                  v1.5
        ├── TypewriterEffect.cs            v1.4 (30 chars/sec)
        └── VignetteEffect.cs              v1.2
```

---

## 📝 Script Versions Summary

| Script | Version | Location | Status |
|--------|---------|----------|--------|
| **ChoiceButton.cs** | v3.2 | UI/ | ✅ Active |
| **GameData.cs** | v1.2 | Data/ | ✅ Active (voice_over added) |
| **GameManager.cs** | v1.3 | Managers/ | ✅ Active (VO playback) |
| **ImageDisplay.cs** | v1.1 | UI/ | ✅ Active |
| **JsonLoader.cs** | v1.1 | Core/ | ✅ Active |
| **LandingPage.cs** | v1.0 | UI/ | ✅ Active (NEW) |
| **PanelGlow.cs** | v1.3 | UI/ | ❌ Not used (removed) |
| **SlidingPanel.cs** | v3.6 | UI/ | ✅ Active (typewriter) |
| **SoundManager.cs** | v1.1 | Managers/ | ✅ Active (VO + music) |
| **TapeDeckUI.cs** | v1.5 | UI/ | ✅ Active |
| **TapePlayer.cs** | v1.0 | Managers/ | ✅ Active |
| **TypewriterEffect.cs** | v1.4 | UI/ | ✅ Active (30 chars/sec) |
| **UIBuilder.cs** | v5.8 | Setup/ | ✅ Active (menu/tape match) |
| **VignetteEffect.cs** | v1.2 | UI/ | ✅ Active |

---

## 🎨 Visual Assets Required

### **Images:**
```
Resources/Images/
├── Events/
│   ├── ep1_event1.jpg
│   ├── ep1_event2.png
│   └── ... (21 total)
│
└── UI/
    ├── cassette_player.png       ← Pixel art cassette tape player
    └── landing_background.png    ← Landing page background
```

### **Audio:**
```
Resources/Audio/
├── Music/
│   ├── background_ambient.mp3    ← Game background music
│   └── landing_music.mp3         ← Landing page theme
│
├── SFX/
│   ├── button_click.wav
│   ├── button_hover.wav
│   ├── panel_slide.wav
│   └── typing.wav
│
├── Tapes/
│   ├── tape_01.mp3
│   ├── tape_02.mp3
│   └── tape_03.mp3
│
├── Soundscapes/
│   ├── ambient_city.mp3
│   ├── ambient_interior.mp3
│   └── ambient_wind.mp3
│
└── VoiceOvers/
    └── vo/
        ├── ep1_event1_en.mp3
        ├── ep1_event1_cs.mp3
        └── ... (84 files: 21 events × 4 languages)
```

---

## 🌍 JSON Episodes (Multi-Language)

```
StreamingAssets/Episodes/
├── episode01_en.json    ✅ Complete (21 events, all VO paths)
├── episode01_cs.json    ✅ Complete (Czech translation)
├── episode01_pl.json    ⏳ Structure ready (needs translation)
└── episode01_fr.json    ⏳ Structure ready (needs translation)
```

---

## 🎯 Scene Hierarchy (Runtime)

When game runs, UIBuilder creates:

```
GameScene
├── GameManager
├── SoundManager
├── TapePlayer
├── EventSystem
│
├── Canvas (Main)
│   ├── BackgroundImage
│   ├── VignetteOverlay
│   ├── MenuButton (grey-green, 100×40, top-left)
│   │   ├── Text: "MENU"
│   │   └── MenuDropdown
│   │       ├── Exit
│   │       └── Settings
│   │
│   └── TapeDeckPanel (600×400, bottom-left)
│       ├── PullTab (grey-green, 100×40, "TAPE")
│       ├── TapeTitle
│       ├── TapeArtist
│       ├── InvisibleButtons (PREV, NEXT, PLAY, STOP)
│       └── CassettePlayerSprite
│
└── PanelCanvas (sortingOrder=10)
    └── SlidingPanel (400px width, right side)
        ├── NarrativeText (with TypewriterEffect)
        └── ChoiceContainer
            └── ChoiceButtons (dynamic)
```

---

## 🎮 Key Features Status

| Feature | Status | Notes |
|---------|--------|-------|
| **JSON-driven narrative** | ✅ Working | 21 events, choices, branching |
| **Typewriter effect** | ✅ Working | 30 chars/sec |
| **Cassette tape player** | ✅ Working | Bottom-left, grey-green tab |
| **Menu system** | ✅ Working | Matches tape style |
| **Voice over** | ✅ Integrated | Just add audio files |
| **Multi-language** | ✅ Ready | EN complete, CS complete |
| **Landing page** | ✅ Script ready | Needs UI build + images |
| **Sound system** | ✅ Working | Music, SFX, VO support |
| **Vignette** | ✅ Working | Atmospheric edge darkening |
| **Panel glow** | ❌ Removed | Clean panel preferred |

