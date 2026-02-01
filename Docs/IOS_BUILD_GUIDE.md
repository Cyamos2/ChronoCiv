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
```

#### iOS Settings
```
Player → Other Settings:
- Color Space: Gamma (for retro pixel look)
- Auto Graphics API: Enabled
- Target Device: iPhone + iPad

Player → Publishing Settings:
- Scripting Backend: IL2CPP
- Architecture: ARM64
- C++ Compiler Configuration: Release
```

#### Orientation
```
Player → Resolution and Presentation:
- Default Orientation: Landscape Left
- Supported: Landscape Left, Landscape Right
```

### Step 4: Add Required Scenes

1. Go to **File → Build Settings**
2. Click **Add Open Scenes**
3. Add these scenes:
   - `Scenes/MainMenu.scene`
   - `Scenes/Game.scene`

### Step 5: Add Input Manager

1. Create empty GameObject named "InputManager"
2. Add `InputManager` component from `Source/Core/Platform/`
3. Set `Current Platform` to `Auto`

### Step 6: Configure Camera

1. Find Main Camera in scene
2. Add `CameraController` component from `Source/Core/Platform/`
3. Or configure manually:
   - Projection: Orthographic
   - Size: 5

### Step 7: Build and Run

1. Connect iOS device or use simulator
2. In **Build Settings**, click **Build And Run**
3. Choose folder for Xcode project
4. Unity will build and launch Xcode
5. In Xcode, select device and click Run

## iOS Controls

- **Tap**: Select/interact with NPCs and buildings
- **Tap & Hold**: Move character to position
- **Swipe**: Pan camera
- **Pinch**: Zoom in/out
- **Double Tap**: Toggle virtual cursor visibility

## Troubleshooting

| Issue | Solution |
|-------|----------|
| iOS Build Support missing | Install via Unity Hub → Add Modules |
| Metal not supported | Remove Metal from Graphics APIs |
| arm64 error | Set Architecture to ARM64 |
| Touch not working | Ensure InputManager in scene |

## Files Created

iOS support files:
- `Source/Core/Platform/InputManager.cs`
- `Source/Core/Platform/iOSConfig.cs`
- `Source/Core/Platform/CameraController.cs`
- `Source/Core/Platform/VirtualJoystick.cs`
- `Source/GamePlay/Player/PlayerController.cs`
- `Source/GamePlay/Player/PlayerCursor.cs`
- `Source/GamePlay/Player/PlayerProfile.cs`

## Additional Resources

- [Unity iOS Documentation](https://docs.unity3d.com/Manual/ios.html)
- [Apple Developer Program](https://developer.apple.com/programs/)
