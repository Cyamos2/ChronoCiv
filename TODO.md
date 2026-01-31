# TODO - iOS Support and Player Character

## Task Overview
Add iOS platform support and implement a cursor-controllable player character.

---

## Phase 1: iOS Platform Support
- [x] Create `Source/Core/Platform/InputManager.cs`
  - Unified input system supporting mouse, touch, and cursor
  - Platform detection (iOS vs desktop)
  - Touch gesture handling (tap, drag, pinch)
- [x] Create `Source/Core/Platform/iOSConfig.cs`
  - iOS-specific configuration
  - Touch input settings
  - Virtual cursor settings
- [ ] Update `GAME_SUMMARY.txt` with iOS features
- [ ] Update `Docs/README.md` with iOS controls

## Phase 2: Player Character
- [ ] Create `Source/GamePlay/Player/PlayerController.cs`
  - Main player character controller
  - Cursor-following movement
  - Integration with NavMesh for pathfinding
  - Animation state management
- [ ] Create `Source/GamePlay/Player/PlayerCursor.cs`
  - Visual cursor indicator for touch/mobile
  - Cursor trail effects
  - Touch position feedback
- [ ] Create `Source/GamePlay/Player/PlayerProfile.cs`
  - Player data structure
  - Save/load support
- [ ] Update `Data/npc_profiles.json` - Add player profile entry

## Phase 3: Input System
- [ ] Implement unified input handling
  - Desktop: Mouse cursor + click to move
  - iOS: Touch + virtual cursor
  - Cross-platform: Both input methods work
- [ ] Add cursor trail/visual feedback
- [ ] Pathfinding integration with existing NavMesh system
- [ ] Camera control integration (pan, zoom)

## Phase 4: UI Updates for iOS
- [ ] Add mobile-friendly action buttons
- [ ] Touch gesture support for camera (pan, zoom)
- [ ] Virtual joystick option for movement
- [ ] iOS-optimized HUD layout

---

## Progress Log
- [ ] Phase 1 Complete
- [ ] Phase 2 Complete
- [ ] Phase 3 Complete
- [ ] Phase 4 Complete
- [ ] All Tests Pass

