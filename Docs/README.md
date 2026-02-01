# ChronoCiv - Retro Civilization Game

## Project Overview
A retro-style civilization game that spans from Stone Age to Future, with time progression, free-roaming NPCs, and multiple biomes.

### Technical Specifications
- **Resolution**: 640x360 (16:9)
- **Pixel Scale**: 2x or 3x for crisp retro look
- **Starting Era**: Stone Age (-5000 BCE)
- **Time Progression**: 100 years per day (even while idle)
- **World Biomes**: 10 unique biomes
- **NPC Behavior**: Free-roaming with AI movement

---

## Game Features

### 1. Time & Era System
- 7 distinct eras spanning human history
- 100 years pass each in-game day
- Visual transitions between eras
- Buildings and clothing evolve with eras

### 2. World Generation
- 10 biomes: Desert, Tundra, Tropical Rainforest, Temperate Forest, Taiga, 
  Savanna, Mountain, Ocean, River, Swamp
- Procedural terrain generation
- Resource placement based on biome
- Day/night cycle with weather

### 3. NPC System
- Free-roaming characters
- Era-appropriate appearances
- Dialogue system
- Task assignments
- Population growth

### 4. Resource Management
- Food, Wood, Stone, Iron, Gold
- Population capacity
- Tech tree progression
- Building construction

---

## Era Timeline
1. **Stone Age** (-5000 BCE) - Hunter-gatherer society
2. **Ancient Era** (-3000 BCE) - First civilizations
3. **Classical Era** (1 BCE/CE) - Great empires
4. **Medieval Era** (500 CE) - Knights and castles
5. **Renaissance Era** (1400 CE) - Arts and science
6. **Industrial Era** (1800 CE) - Steam power
7. **Modern Era** (1950 CE) - Technology
8. **Future Era** (2200 CE) - Space age

---

## Controls

### Desktop Controls (Windows/Linux)
- **Left Click**: Select/interact with NPCs, buildings
- **Right Click**: Move character to position
- **Middle Click / Alt+Left Click**: Pan camera
- **Mouse Scroll**: Zoom in/out
- **Space**: Pause/Resume time
- **Escape**: Open menu
- **Ctrl+S**: Quick Save
- **Ctrl+L**: Quick Load
- **Ctrl+P**: Pause/Resume
- **Ctrl+R**: Restart

### macOS Controls
- **Left Click**: Select/interact with NPCs, buildings
- **Right Click**: Move character to position
- **Middle Click / Option+Left Click**: Pan camera
- **Mouse Scroll / Trackpad Scroll**: Zoom in/out
- **Space**: Pause/Resume time
- **Escape**: Open menu

#### Keyboard Shortcuts (Mac)
| Shortcut | Action |
|----------|--------|
| ⌘ + S | Quick Save |
| ⌘ + L | Quick Load |
| ⌘ + P | Pause/Resume |
| ⌘ + R | Restart Game |
| ⌘ + , | Settings |
| ⌘ + Q | Quit Game |
| ⌘ + F | Toggle Fullscreen |

#### Trackpad Gestures (Mac)
| Gesture | Action |
|---------|--------|
| Two-finger Scroll | Pan camera |
| Two-finger Pinch | Zoom in/out |
| Three-finger Swipe Left | Previous era |
| Three-finger Swipe Right | Next era |
| Three-finger Tap | Interact |

#### Arrow Keys
| Key | Action |
|-----|--------|
| ↑/↓ | Move North/South |
| ←/→ | Move West/East |
| Tab | Cycle through NPCs/buildings |
| Enter | Select/Confirm |
| Delete | Cancel/Delete |

### iOS Controls
- **Tap**: Select/interact with NPCs, buildings
- **Tap & Hold (short)**: Show virtual cursor
- **Tap & Hold (long)**: Move character to position
- **Swipe (1 finger)**: Pan camera
- **Swipe (2 fingers)**: Pan camera (larger area)
- **Pinch (2 fingers)**: Zoom in/out
- **Double Tap**: Toggle virtual cursor visibility

### Virtual Joystick (Optional)
When enabled, the virtual joystick appears on the left side of the screen:
- **Drag joystick**: Move character
- **Joystick handles**: Visual feedback for direction and intensity

### Virtual Cursor
On iOS, a virtual cursor follows your touch input:
- **Visual indicator**: Shows current touch position
- **Trail effect**: Shows movement history
- **Click feedback**: Visual and haptic feedback on tap
- **Movement marker**: Shows target destination when moving

---

## iOS-Specific Features

### Touch Gestures
- Single-finger tap for interaction
- Single-finger drag for camera panning
- Two-finger pinch for zoom
- Two-finger pan for larger movements

### Haptic Feedback
- Light impact on UI interactions
- Success feedback on task completion
- Warning feedback on important events
- Can be disabled in settings

### Performance Optimization
- Automatic frame rate limiting (60 FPS)
- Particle effect reduction on low-end devices
- Reduced visual effects in low power mode
- Smart batching of draw calls

### Safe Area Support
- Respects iOS safe areas on notched devices
- Landscape-only orientation for optimal gameplay
- Proper padding around device edges

---

## Development Notes
- Pixel art sprites at 16x16 or 32x32 base resolution
- C# scripts for Unity engine
- JSON data files for easy modding
- Save/Load system for progress

---

## Testing Guide

### Unity Test Framework Setup
This project uses Unity Test Framework (UTF) for unit and integration testing.

### Test Location
All tests are located in: `Source/Tests/`

### Running Tests
1. **Open Unity Test Runner**: Window > General > Test Runner
2. **Choose Test Mode**:
   - Edit Mode: For unit tests (most tests)
   - Play Mode: For integration tests
3. **Execute Tests**:
   - Click 'Run All' for all tests
   - Select specific tests and click 'Run Selected'
4. **View Results**:
   - Green checkmark = Pass
   - Red X = Fail
   - Click test for details

### Test Categories
- **Resource System**: Tests for resource types and management
- **Era System**: Tests for era progression and transitions
- **NPC System**: Tests for NPC data structures and behavior
- **Event System**: Tests for event triggering (coming soon)
- **Animation System**: Tests for sprite animation (coming soon)

### Available Tests
1. **ResourceTypeTests.cs**: Validates resource type definitions and conversions
2. **EraTests.cs**: Tests era data structures and year calculations
3. **NPCTests.cs**: Tests NPC, NPCProfile, Task data structures

### Editor Test Menu
Access test functions via Unity menu: **ChronoCiv > Tests**
- Run All Tests
- Run Resource/Era/NPC Tests individually
- Test Documentation
- Validate Setup

### Creating New Tests
1. Create test file in `Source/Tests/`
2. Use NUnit attributes: `[Test]`, `[SetUp]`, `[TearDown]`
3. Add `[Category("")]` for organization
4. Add `[Description("")]` for documentation
5. Follow AAA pattern: Arrange, Act, Assert

### Best Practices
- Keep tests isolated and independent
- Use descriptive test names
- Test one thing per test
- Group related tests in same class
- Use Unity's `Assert` methods for Unity-specific assertions

