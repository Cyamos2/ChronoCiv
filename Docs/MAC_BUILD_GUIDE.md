# ChronoCiv Mac Build Guide

This guide explains how to build ChronoCiv for macOS devices using Unity.

## Prerequisites

### Required Software
- **Unity 2021.3 LTS or later** (with macOS Build Support module)
- **Xcode 14.0 or later** (for code signing and notarization)
- **macOS** (required for macOS builds)
- **Apple Developer Account** (for distribution)

### Unity Modules Required
1. Open Unity Hub → Installs → Your Unity Version → Add Modules
2. Ensure these are installed:
   - macOS Build Support
   - iOS Build Support (optional, for iOS builds)

## Setup Steps

### Step 1: Open the Project
1. Open Unity Hub
2. Click "Add" and select the ChronoCiv project folder
3. Open the project with your Unity version

### Step 2: Switch to macOS Platform
1. In Unity, go to **File → Build Settings**
2. Select **macOS** from the platform list
3. Click **Switch Platform**

### Step 3: Configure Player Settings

Go to **File → Build Settings → Player Settings**:

#### General Settings
```
Player → Resolution and Presentation:
- Default Canvas Width: 1280
- Default Canvas Height: 720
- Run In Background: Enabled
- Capture Single Screen: Disabled
```

#### macOS Settings
```
Player → Other Settings:
- Color Space: Gamma (for retro pixel look)
- Auto Graphics API: Disabled
- Graphics APIs: Metal (remove OpenGLCore if present)
- Target OS Version: macOS 10.15 (Catalina) or later
- Scripting Backend: IL2CPP
- Architecture: x86_64 (Intel) + ARM64 (Apple Silicon)

Player → Publishing Settings:
- Compression Format: Gzip
- Enable Exceptions: None (for better performance)
- Strip Engine Code: Enabled
- Strip Level: High
```

#### Resolution and Display
```
Player → Resolution and Presentation:
- Mac Retina Support: Enabled
- Run in Background: Enabled
- Allow Fullscreen Switch: Enabled
- Fullscreen Mode: Fullscreen Window
```

### Step 4: Configure macOS-Specific Settings

1. Go to **File → Build Settings → Player Settings → Mac Config**
2. Create a MacConfig asset:
   - Right-click in Project window → Create → ChronoCiv → Mac Configuration
   - Set the following values:
     - Use Command Key: ✅ Enabled
     - Enable Trackpad Scroll: ✅ Enabled
     - Enable Trackpad Pinch: ✅ Enabled
     - Support Fullscreen: ✅ Enabled
     - Support Retina Display: ✅ Enabled
     - Enable Metal Rendering: ✅ Enabled

### Step 5: Configure Quality Settings

1. Go to **Edit → Project Settings → Quality**
2. Set the following for macOS:
   - VSync Count: 1 (for smooth rendering)
   - Anti Aliasing: 2x (for crisp edges)
   - Anisotropic Filtering: Per Texture
   - Soft Particles: Enabled

### Step 6: Add Required Scenes

1. Go to **File → Build Settings**
2. Click **Add Open Scenes**
3. Add these scenes:
   - `Scenes/MainMenu.scene`
   - `Scenes/Game.scene`

### Step 7: Configure Input Manager

1. Create empty GameObject named "InputManager"
2. Add `InputManager` component from `Source/Core/Platform/`
3. Set `Current Platform` to `Auto`
4. The platform will auto-detect macOS

### Step 8: Configure Camera

1. Find Main Camera in scene
2. Add `CameraController` component from `Source/Core/Platform/`
3. Configure for macOS:
   - Projection: Orthographic
   - Size: 5
   - Edge Pan: Enabled
   - Edge Threshold: 20

## Keyboard Controls

### Movement
| Key | Action |
|-----|--------|
| Arrow Keys | Move player |
| Space | Interact/Select |
| Escape | Open Menu/Cancel |
| Tab | Cycle through NPCs |

### Camera Controls
| Key/Gesture | Action |
|-------------|--------|
| Mouse Scroll | Zoom in/out |
| Click + Drag | Pan camera |
| Trackpad Scroll | Pan camera |
| Trackpad Pinch | Zoom in/out |

### System
| Shortcut | Action |
|----------|--------|
| ⌘ + S | Quick Save |
| ⌘ + L | Quick Load |
| ⌘ + P | Pause/Resume |
| ⌘ + R | Restart Game |
| ⌘ + , | Settings |
| ⌘ + Q | Quit Game |

### Era Selection
| Key | Action |
|-----|--------|
| 1 | Stone Age |
| 2 | Ancient Era |
| 3 | Classical Era |
| 4 | Medieval Era |
| 5 | Renaissance Era |
| 6 | Industrial Era |
| 7 | Modern Era |
| 8 | Future Era |

## Trackpad Gestures

### Supported Gestures
- **Two-finger Scroll**: Pan camera
- **Two-finger Pinch**: Zoom in/out
- **Three-finger Swipe**: Quick save/load
- **Three-finger Tap**: Interact

### Configuration
You can adjust gesture sensitivity in the MacConfig asset:
- Trackpad Scroll Speed: 1.0 (default)
- Trackpad Pinch Speed: 0.05 (default)
- Trackpad Swipe Threshold: 0.5 (default)

## Magic Mouse Support

### Supported Gestures
- **Scroll**: Zoom in/out
- **Swipe Left/Right**: Previous/Next era
- **Swipe Up/Down**: Quick save/load
- **Force Click**: Interact

### Configuration
Adjust in MacConfig asset:
- Magic Mouse Scroll Speed: 1.0 (default)
- Magic Mouse Swipe Threshold: 0.3 (default)

## Building and Running

### Development Build
1. Go to **File → Build Settings**
2. Enable **Development Build**
3. Click **Build**
4. Choose output folder
5. Unity will build the .app bundle

### Production Build
1. Disable **Development Build**
2. Click **Build**
3. Choose output folder
4. Unity will create a release .app bundle

### Running the Build
```bash
# Open the built app
open /path/to/ChronoCiv.app

# Run from terminal
/Applications/ChronoCiv.app/Contents/MacOS/ChronoCiv
```

## Code Signing and Distribution

### For App Store Distribution

1. **Create Apple Developer Account**
   - Sign up at https://developer.apple.com/
   - Enroll in Apple Developer Program ($99/year)

2. **Configure Code Signing**
   - Go to **File → Build Settings → Player Settings → Publishing Settings**
   - Set Code Signing Identity: Your Developer ID certificate
   - Automatically Sign: Enabled

3. **Build for App Store**
   ```bash
   # Build from Unity
   unity -buildTarget macOS -buildPath ./Build
   ```

4. **Notarize the App**
   ```bash
   # Upload to Apple for notarization
   xcrun altool --notarize-app \
     --primary-bundle-id com.chrono.civ \
     --username "your-apple-id@email.com" \
     --password "app-specific-password" \
     --build-path ./Build/ChronoCiv.app
   
   # Check notarization status
   xcrun altool --notarization-info <request-id> \
     --username "your-apple-id@email.com" \
     --password "app-specific-password"
   ```

5. **Staple Notarization**
   ```bash
   xcrun stapler staple ./Build/ChronoCiv.app
   ```

### For Direct Distribution

1. **Developer ID Signing**
   ```bash
   # Sign the app
   codesign --sign "Developer ID Application: Your Name" \
     --entitlements ./ChronoCiv.entitlements \
     --deep ./Build/ChronoCiv.app
   
   # Verify signature
   codesign --verify --verbose=4 ./Build/ChronoCiv.app
   ```

2. **Create DMG Installer**
   ```bash
   # Create DMG
   create-dmg \
     --volname "ChronoCiv" \
     --volicon "./Data/Art/AppIcon.icns" \
     --background "./Data/Art/dmg_background.png" \
     --window-size 600 400 \
     --app-drop-link 450 200 \
     ./ChronoCiv.dmg \
     ./Build/ChronoCiv.app
   ```

### For Steam Distribution

1. **Steamworks Integration**
   - Download Steamworks SDK from https://partner.steamgames.com/
   - Integrate Steam API into the project
   - Configure Steam achievement callbacks

2. **Build for Steam**
   - Build macOS .app bundle
   - Upload via Steam Partner Dashboard

## Apple Silicon (M1/M2/M3) Support

### Universal Build Configuration
```
Player → Other Settings:
- Architecture: ARM64 (for Apple Silicon)
- Scripting Backend: IL2CPP
- C++ Compiler Configuration: Release
```

### Performance Optimization for Apple Silicon
1. Enable Metal Rendering
2. Set Target Frame Rate to 144 (ProMotion displays)
3. Enable VSync for smooth rendering
4. Configure quality settings for high performance

### Recommended Quality Settings
| Setting | Apple Silicon | Intel Mac |
|---------|--------------|-----------|
| Anti Aliasing | 4x | 2x |
| Shadow Quality | High | Medium |
| Texture Quality | Full | Half |
| Particles | High | Medium |

## Troubleshooting

### Issue: Black Screen on Launch
**Solution:**
- Ensure Metal is selected as Graphics API
- Check display color space is Gamma (not Linear)
- Verify all required assets are included in build

### Issue: Poor Performance on Intel Mac
**Solution:**
- Reduce anti-aliasing to 2x or 0x
- Lower shadow quality settings
- Reduce texture quality
- Disable unnecessary effects

### Issue: Keyboard Shortcuts Not Working
**Solution:**
- Ensure Command key is enabled in MacConfig
- Check no other applications are capturing input
- Verify keyboard layout in macOS System Preferences

### Issue: Trackpad Gestures Not Responsive
**Solution:**
- Enable Trackpad input in MacConfig
- Adjust sensitivity settings
- Ensure no conflicting macOS gestures

### Issue: App Crashes on Quit
**Solution:**
- Implement proper cleanup in OnApplicationQuit
- Save game state before quitting
- Dispose of all resources properly

### Issue: Notarization Fails
**Solution:**
- Check code signing certificate is valid
- Verify entitlements file is correct
- Ensure all frameworks are signed
- Check for missing hardened runtime entitlements

## Files Created

macOS support files:
- `Source/Core/Platform/MacConfig.cs` - macOS configuration
- `Source/Core/Platform/InputManager.cs` - Input handling (updated for Mac)
- `Source/Core/Platform/CameraController.cs` - Camera controls (updated for Mac)

## Additional Resources

- [Unity macOS Documentation](https://docs.unity3d.com/Manual/BuildingForMacOSX.html)
- [Apple Developer Program](https://developer.apple.com/programs/)
- [Apple Silicon Developer Guide](https://developer.apple.com/documentation/apple_silicon)
- [Code Signing Requirements](https://developer.apple.com/library/archive/documentation/Security/Conceptual/CodeSigningGuide/Introduction/Introduction.html)
- [Notarization Guide](https://developer.apple.com/documentation/xcode/notarizing_macos_software_before_distribution)

## Performance Benchmarks

### Expected Performance on Apple Silicon
| Device | Resolution | FPS | Quality |
|--------|-----------|-----|---------|
| M1 MacBook Air | 1280x720 | 60 | High |
| M1 MacBook Pro | 1920x1080 | 60 | High |
| M2 MacBook Air | 2560x1440 | 60 | High |
| M2 Mac Studio | 3840x2160 | 60 | Ultra |

### Expected Performance on Intel Mac
| Device | Resolution | FPS | Quality |
|--------|-----------|-----|---------|
| Intel MacBook Pro (2019) | 1280x720 | 60 | Medium |
| Intel iMac (2020) | 1920x1080 | 60 | Medium-High |
| Intel Mac Pro (2019) | 2560x1440 | 60 | High |

