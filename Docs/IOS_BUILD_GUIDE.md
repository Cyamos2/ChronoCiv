# ChronoCiv iOS Build Guide

This guide explains how to build ChronoCiv for iOS devices using Unity.

## Prerequisites

### Required Software
- **Unity 2021.3 LTS or later** (with iOS Build Support module)
- **Xcode 14.0 or later**
- **macOS** (required for iOS builds)
- **Apple Developer Account** (for testing on devices)

### Unity Modules Required
1. Open Unity Hub → Installs → Your Unity Version → Add Modules
2. Ensure these are installed:
   - iOS Build Support
   - Mac Build Support

## Setup Steps

### Step 1: Open the Project
1. Open Unity Hub
2. Click "Add" and select the ChronoCiv project folder
3. Open the project with your Unity version

### Step 2: Switch to iOS Platform
1. In Unity, go to **File → Build Settings**
2. Select **iOS** from the platform list
3. Click **Switch Platform**

### Step 3: Configure Player Settings

Go to **File → Build Settings → Player Settings**:

#### General Settings
```
Player → Resolution and Presentation:
- Default Canvas Width: 640
- Default Canvas Height: 360
- Run In Background: Enabled
- Supported Orientations: Landscape Left, Landscape Right
```

#### iOS Settings
```
Player → Other Settings:
- Color Space: Gamma (for retro pixel look)
- Auto Graphics API: Enabled
- Target Device: iPhone + iPad
- Target OS Version: iOS 14.0 or later
- Scripting Backend: IL2CPP
- Architecture: ARM64
- C++ Compiler Configuration: Release
- Enable Exceptions: None (for better performance)
- Strip Engine Code: Enabled
- Strip Level: High

Player → Publishing Settings:
- App Store URL Schemes: Not required
- Status Bar Style: Default
- Hide home button: Yes (requires iOS 7+)
- Status bar hidden: No
- Exit on suspend: No
```

#### Orientation
```
Player → Resolution and Presentation:
- Default Orientation: Landscape Left
- Supported: Landscape Left, Landscape Right
- Auto rotate: Yes
```

### Step 4: Configure iOS Config Asset

1. Create iOSConfig asset:
   - Right-click in Project window → Create → ChronoCiv → iOS Configuration
   - Configure the following settings:

#### Touch Input Settings
- Enable Multi-Touch: ✅ Enabled
- Touch Sensitivity: 1.0
- Long Press Duration: 0.5 seconds
- Tap Threshold Distance: 20 pixels

#### Virtual Cursor Settings
- Enable Virtual Cursor: ✅ Enabled
- Cursor Speed: 500
- Cursor Trail Duration: 0.3 seconds
- Cursor Follows Finger: ✅ Enabled
- Cursor Offset Y: 50 pixels

#### Camera Controls
- Enable Swipe Pan: ✅ Enabled
- Enable Pinch Zoom: ✅ Enabled
- Gesture Smoothing: 0.15
- Min Zoom: 3, Max Zoom: 10

#### Performance Settings
- Limit Frame Rate: ✅ Enabled
- Target Frame Rate: 60 (or 120 for ProMotion displays)
- Enable Low Power Mode: ⬜ Disabled (auto-detect)
- Reduce Effects in Low Power: ✅ Enabled
- Max Particles: 50

#### Haptic Feedback (Core Haptics API)
- Enable Haptics: ✅ Enabled
- Use Core Haptics API: ✅ Enabled (iOS 13+)
- Haptic Intensity: 1.0
- Haptic on Interact: ✅ Enabled
- Haptic on Era Transition: ✅ Enabled
- Haptic on Notification: ✅ Enabled

#### Safe Areas & Display
- Respect Safe Area: ✅ Enabled
- Support Notched Devices: ✅ Enabled (iPhone X and later)
- Support Dynamic Island: ✅ Enabled (iPhone 14 Pro and later)
- Support ProMotion Display: ✅ Enabled (iPhone 13 Pro and later)

#### Dark Mode Support
- Support Dark Mode: ✅ Enabled
- Follow System Theme: ✅ Enabled
- Custom Dark Mode Background: (0.1, 0.1, 0.1)
- Custom Dark Mode Text: (0.9, 0.9, 0.9)

### Step 5: Add Required Scenes

1. Go to **File → Build Settings**
2. Click **Add Open Scenes**
3. Add these scenes:
   - `Scenes/MainMenu.scene`
   - `Scenes/Game.scene`

### Step 6: Add Input Manager

1. Create empty GameObject named "InputManager"
2. Add `InputManager` component from `Source/Core/Platform/`
3. Set `Current Platform` to `Auto`

### Step 7: Configure Camera

1. Find Main Camera in scene
2. Add `CameraController` component from `Source/Core/Platform/`
3. Configure for iOS:
   - Projection: Orthographic
   - Size: 5
   - Enable Swipe Pan: ✅ Enabled
   - Enable Pinch Zoom: ✅ Enabled
   - Gesture Smoothing: 0.15

### Step 8: Configure iCloud Save Manager (Optional)

1. Create empty GameObject named "CloudSaveManager"
2. Add `iCloudSaveManager` component from `Source/Core/Platform/`
3. Configure:
   - Enable Cloud Sync: ✅ Enabled
   - iCloud Container ID: iCloud.com.chrono.civ
   - Conflict Resolution: Keep Newer
   - Auto Sync on Pause: ✅ Enabled

## iOS-Specific Features

### Core Haptics Integration

ChronoCiv uses Apple's Core Haptics framework (iOS 13+) for advanced haptic feedback:

#### Supported Haptic Types
- **Light Impact**: UI interactions, light selections
- **Medium Impact**: Standard interactions
- **Heavy Impact**: Important events, era transitions
- **Success**: Task completion, achievements
- **Warning**: Resource warnings, low health
- **Error**: Critical events, game over
- **Selection**: Menu item selection

#### Configuration
Adjust haptic intensity in iOSConfig:
- Intensity: 0.0 to 1.0 (default: 1.0)
- Can be disabled per-event-type for testing

### Dark Mode Support

ChronoCiv automatically adapts to iOS Dark Mode:

#### Supported Devices
- All devices running iOS 13 and later

#### Behavior
- Follows system-wide Dark Mode setting
- Custom color scheme for optimal retro look
- Automatic UI color adaptation

### Low Power Mode Detection

ChronoCiv detects and adapts to iOS Low Power Mode:

#### When Low Power Mode is Active:
- Frame rate reduced to 30 FPS
- Particle effects reduced or disabled
- Shadows disabled
- Anti-aliasing disabled
- Background refresh disabled

### Safe Area Support

ChronoCiv properly handles all iOS device safe areas:

#### Supported Devices
- iPhone X, XS, XR, 11, 12, 13, 14 (all Pro variants)
- iPhone 14 Pro with Dynamic Island
- All iPad models

#### Features
- Respects notch area
- Handles home indicator
- Adapts to different aspect ratios
- Dynamic Island aware (iPhone 14 Pro)

### ProMotion Display Support

ChronoCiv supports 120Hz ProMotion displays:

#### Supported Devices
- iPad Pro 12.9" (2nd gen and later)
- iPad Pro 11" (all)
- iPhone 13 Pro, Pro Max
- iPhone 14 Pro, Pro Max

#### Behavior
- Automatic refresh rate detection
- 120Hz rendering when available
- Smooth scrolling and animations

## iOS Controls

### Touch Gestures
| Gesture | Action |
|---------|--------|
| Tap | Select/interact with NPCs and buildings |
| Tap & Hold (short) | Show virtual cursor |
| Tap & Hold (long) | Move character to position |
| Swipe (1 finger) | Pan camera |
| Swipe (2 fingers) | Pan camera (larger area) |
| Pinch (2 fingers) | Zoom in/out |
| Double Tap | Toggle virtual cursor visibility |

### Virtual Cursor
When enabled, a virtual cursor follows your touch input:
- Visual indicator shows current touch position
- Trail effect shows movement history
- Click feedback with haptic response
- Movement marker shows target destination

### Virtual Joystick (Optional)
When enabled, the virtual joystick appears on the left side:
- Drag joystick to move character
- Joystick handles show direction and intensity
- Snap-to-finger positioning for easy use

## Game Controller Support

ChronoCiv supports MFi game controllers on iOS:

### Supported Controllers
- Sony DualShock 4
- Microsoft Xbox Wireless Controller
- Apple MFi Controller
- SteelSeries Nimbus

### Controller Features
- Left stick: Movement
- Right stick: Camera pan
- D-pad: Quick era selection
- A/X button: Interact
- B/O button: Cancel/Back
- Start button: Pause
- Rumble feedback (haptic)

## Building and Running

### Development Build
1. Go to **File → Build Settings**
2. Enable **Development Build**
3. Click **Build**
4. Choose output folder
5. Unity will create Xcode project

### Production Build
1. Disable **Development Build**
2. Click **Build**
3. Choose output folder
4. Unity will create release-ready Xcode project

### Running on Device
1. Open the Xcode project
2. Select your development team
3. Set bundle identifier: com.chrono.civ
4. Connect iOS device
5. Click **Run** (⌘R)

## App Store Submission

### Required Configuration

1. **App Store Connect**
   - Create app record with bundle ID: com.chrono.civ
   - Set app name: ChronoCiv
   - Configure pricing and availability

2. **Build Settings**
   ```
   Player → Publishing Settings:
   - Scripting Backend: IL2CPP
   - Architecture: ARM64
   - Compression Format: Gzip
   ```

3. **Icons and Splash**
   - Add App Store icon (1024x1024)
   - Configure launch screen

4. **Submit for Review**
   - Complete app description
   - Add screenshots for all device sizes
   - Set age rating
   - Submit for review

### Required Capabilities
- Push Notifications (optional)
- In-App Purchase (optional)
- Game Center (for achievements)

## Troubleshooting

### Issue: Touch Not Working
**Solution:**
- Ensure InputManager is in scene with Current Platform = Auto
- Check touch input is enabled in iOSConfig
- Verify device supports multi-touch
- Test with single touch first

### Issue: Haptic Feedback Not Working
**Solution:**
- Ensure device supports Core Haptics (iPhone 8+)
- Check haptics are enabled in iOSConfig
- Verify haptic intensity > 0
- Test with UI interactions first
- Check device is not in Silent mode

### Issue: Poor Performance
**Solution:**
- Enable Low Power Mode in iOSConfig
- Reduce target frame rate
- Disable particle effects
- Reduce max NPCs
- Lower shadow quality
- Use lower anti-aliasing

### Issue: Safe Areas Not Respected
**Solution:**
- Ensure Respect Safe Area is enabled in iOSConfig
- Check all UI elements anchor to screen edges
- Use Safe Area layout guide in Unity UI
- Test on actual notched device

### Issue: Dark Mode Not Working
**Solution:**
- Ensure Support Dark Mode is enabled in iOSConfig
- Test on device with Dark Mode enabled in Settings
- Check custom colors are set for dark theme
- Verify UI elements use configurable colors

### Issue: Game Controller Not Detected
**Solution:**
- Ensure controller is MFi certified
- Pair controller via Bluetooth before launching app
- Check Enable Game Controller in iOSConfig
- Test with different controller brands

## Files Created

iOS support files:
- `Source/Core/Platform/InputManager.cs` - Input handling
- `Source/Core/Platform/iOSConfig.cs` - iOS configuration
- `Source/Core/Platform/CameraController.cs` - Camera controls
- `Source/Core/Platform/VirtualJoystick.cs` - Virtual joystick
- `Source/Core/Platform/iCloudSaveManager.cs` - Cloud save sync
- `Source/GamePlay/Player/PlayerController.cs` - Player controller
- `Source/GamePlay/Player/PlayerCursor.cs` - Virtual cursor

## Performance Recommendations

### Device-Specific Settings

#### iPhone 13/14 Pro (ProMotion)
- Target Frame Rate: 120
- Anti Aliasing: 4x
- Shadows: Medium
- Particles: High

#### iPhone 11/12/13/14 (Standard)
- Target Frame Rate: 60
- Anti Aliasing: 2x
- Shadows: Low
- Particles: Medium

#### Older Devices (iPhone 8-X)
- Target Frame Rate: 30
- Anti Aliasing: 0x
- Shadows: Disabled
- Particles: Low

### Memory Optimization
- Limit max particles: 50
- Use object pooling for NPCs
- Unload unused assets
- Compress textures

## Additional Resources

- [Unity iOS Documentation](https://docs.unity3d.com/Manual/ios.html)
- [Core Haptics Guide](https://developer.apple.com/documentation/corehaptics)
- [Apple Developer Program](https://developer.apple.com/programs/)
- [iOS Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/ios/overview/themes/)
- [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
