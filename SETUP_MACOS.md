# ChronoCiv macOS Unity Setup Guide

This guide provides step-by-step instructions to set up ChronoCiv in Unity for macOS development.

## Prerequisites Checklist

### 1. Install Unity with macOS Build Support
- [ ] Open Unity Hub (already running)
- [ ] Click "Installs" → "Install Editor"
- [ ] Select **Unity 2021.3 LTS** or later (2022.3 LTS recommended)
- [ ] **IMPORTANT**: Add the **macOS Build Support** module
- [ ] Wait for installation to complete

### 2. Verify Unity Installation
After installation:
```bash
# Check Unity version
ls /Applications/Unity/Hub/Editor/

# Should show installed versions like:
# 2021.3.40f1/
# 2022.3.40f1/
```

### 3. Clone/Open the Project
- [ ] Open Unity Hub
- [ ] Click "Projects" → "Add" → Select `/Users/christopheramos/Projects/ChronoCiv`
- [ ] Click the project to open it

---

## Unity Project Setup Steps

### Step 1: Switch to macOS Platform
1. In Unity, go to **File → Build Settings**
2. Select **macOS** from the platform list
3. Click **Switch Platform** (this may take a few minutes)

### Step 2: Configure Player Settings

Go to **File → Build Settings → Player Settings**:

#### General Settings
- **Resolution and Presentation**:
  - Default Canvas Width: 1280
  - Default Canvas Height: 720
  - Run In Background: ✅ Enabled
  - Capture Single Screen: ❌ Disabled

#### macOS-Specific Settings
- **Other Settings**:
  - Color Space: **Gamma** (for retro pixel look)
  - Auto Graphics API: ❌ Disabled
  - Graphics APIs: **Metal** (remove OpenGLCore)
  - Target OS Version: macOS 10.15 (Catalina) or later
  - Scripting Backend: **IL2CPP**
  - Architecture: **ARM64** (for Apple Silicon) + x86_64 (for Intel)
  
- **Publishing Settings**:
  - Compression Format: Gzip
  - Enable Exceptions: None
  - Strip Engine Code: ✅ Enabled
  - Strip Level: High

#### Resolution Settings
- **Resolution and Presentation**:
  - Mac Retina Support: ✅ Enabled
  - Allow Fullscreen Switch: ✅ Enabled
  - Fullscreen Mode: Fullscreen Window

### Step 3: Configure Quality Settings
1. Go to **Edit → Project Settings → Quality**
2. Set for macOS:
   - VSync Count: 1
   - Anti Aliasing: 2x
   - Anisotropic Filtering: Per Texture
   - Soft Particles: ✅ Enabled

### Step 4: Add Required Scenes
1. Go to **File → Build Settings**
2. Click **Add Open Scenes**
3. Add:
   - `Scenes/MainMenu.scene`
   - `Scenes/Game.scene`

### Step 5: Configure Input Manager
1. Create empty GameObject named "InputManager" in the scene
2. Add `InputManager` component from `Source/Core/Platform/InputManager.cs`
3. Set `Current Platform` to **Auto**

### Step 6: Configure Camera
1. Find Main Camera in the scene
2. Add `CameraController` component from `Source/Core/Platform/CameraController.cs`
3. Configure:
   - Projection: Orthographic
   - Size: 5
   - Edge Pan: ✅ Enabled
   - Edge Threshold: 20

### Step 7: Import Sprites
1. Select all PNG files in `Data/Art/Sprites/`
2. Set Texture Type to **Sprite (2D and UI)**
3. Pixels Per Unit: 16 for NPCs, 32 for buildings
4. Click **Apply**

---

## Project Structure Overview

```
ChronoCiv/
├── Scenes/
│   ├── MainMenu.scene
│   └── Game.scene
├── Source/
│   ├── Core/
│   │   ├── TimeManager.cs
│   │   ├── EventBus.cs
│   │   ├── SaveSystem.cs
│   │   └── Platform/
│   │       ├── MacConfig.cs
│   │       ├── InputManager.cs
│   │       ├── CameraController.cs
│   │       └── iCloudSaveManager.cs
│   ├── GamePlay/
│   │   ├── Eras/
│   │   ├── NPCs/
│   │   ├── Resources/
│   │   ├── TechTree/
│   │   ├── Weather/
│   │   ├── Events/
│   │   ├── Animation/
│   │   ├── OfflineProgress/
│   │   └── Player/
│   └── UI/
├── Data/
│   ├── eras.json
│   ├── resources.json
│   ├── buildings.json
│   ├── npc_profiles.json
│   ├── dialogue.json
│   ├── tasks.json
│   ├── weather.json
│   ├── tech_tree.json
│   ├── events.json
│   └── Art/Sprites/
└── Docs/
    ├── README.md
    ├── MAC_BUILD_GUIDE.md
    └── IOS_BUILD_GUIDE.md
```

---

## Keyboard Controls (macOS)

| Key | Action |
|-----|--------|
| Arrow Keys | Move player |
| Space | Interact/Select |
| Escape | Open Menu/Cancel |
| Tab | Cycle through NPCs |

### System Shortcuts
| Shortcut | Action |
|----------|--------|
| ⌘ + S | Quick Save |
| ⌘ + L | Quick Load |
| ⌘ + P | Pause/Resume |
| ⌘ + R | Restart Game |
| ⌘ + , | Settings |
| ⌘ + Q | Quit Game |
| ⌘ + F | Toggle Fullscreen |

### Era Selection
| Key | Era |
|-----|-----|
| 1 | Stone Age |
| 2 | Ancient Era |
| 3 | Classical Era |
| 4 | Medieval Era |
| 5 | Renaissance Era |
| 6 | Industrial Era |
| 7 | Modern Era |
| 8 | Future Era |

---

## Trackpad Gestures

- **Two-finger Scroll**: Pan camera
- **Two-finger Pinch**: Zoom in/out
- **Three-finger Swipe**: Quick save/load
- **Three-finger Tap**: Interact
- **Option + Click + Drag**: Pan camera

---

## Building the Project

### Development Build
1. Go to **File → Build Settings**
2. Enable **Development Build**
3. Click **Build**
4. Choose output folder
5. Unity will create `.app` bundle

### Running the Build
```bash
# Open the built app
open /path/to/ChronoCiv.app

# Or run from terminal
/Applications/ChronoCiv.app/Contents/MacOS/ChronoCiv
```

---

## Testing in Unity

### Running the Game
1. Press **Play** button in Unity toolbar
2. Or press **⌘ + P**

### Running Tests
1. Go to **Window → General → Test Runner**
2. Select **Edit Mode** tab
3. Click **Run All** to run all unit tests

---

## Troubleshooting

### Issue: Unity not found after installation
**Solution**:
```bash
# Check installed versions
ls /Applications/Unity/Hub/Editor/

# Open Unity Hub and check Installs tab
```

### Issue: macOS Build Support not available
**Solution**:
1. Open Unity Hub
2. Go to **Installs** → Select your Unity version
3. Click **Add Modules**
4. Select **macOS Build Support**
5. Click **Install**

### Issue: Project won't open
**Solution**:
1. Check Unity version compatibility (2021.3+ required)
2. Delete `Library` folder (will regenerate)
3. Restart Unity Hub

### Issue: Sprites not displaying
**Solution**:
1. Select sprite files in Project window
2. Set Texture Type to "Sprite (2D and UI)"
3. Click Apply
4. Set Pixels Per Unit to 16 or 32

### Issue: Black screen on launch
**Solution**:
1. Ensure Metal is selected as Graphics API
2. Check Color Space is Gamma (not Linear)
3. Verify scenes are added to Build Settings

---

## Next Steps

After successful setup:
1. ✅ Test basic gameplay
2. ✅ Run unit tests
3. ✅ Build development version
4. ✅ Test on target hardware
5. Configure code signing for distribution

---

## Additional Resources

- [Unity macOS Documentation](https://docs.unity3d.com/Manual/BuildingForMacOSX.html)
- [Apple Developer Program](https://developer.apple.com/programs/)
- [ChronoCiv Build Guide](Docs/MAC_BUILD_GUIDE.md)

