# Testing Implementation Plan - Hybrid Approach

## Phase 1: Fix Immediate Issue (Remove Problematic Python Config)
### Objective: Resolve Unity/VS Code freeze by removing inappropriate Python test configuration

**Step 1.1: Clean up .vscode/settings.json**
- [ ] Remove Python unittest configuration pointing to `./Data` directory
- [ ] Add appropriate Unity/C# VS Code settings
- [ ] Keep Unity-specific IntelliSense and formatting settings

**Step 1.2: Verify Configuration**
- [ ] Test VS Code loads without freezing
- [ ] Confirm Unity integration works properly
- [ ] Check that no test runners are attempting to run inappropriate tests

## Phase 2: Set Up Unity Test Framework
### Objective: Create proper Unity testing infrastructure

**Step 2.1: Create Test Assembly Structure**
- [ ] Create `Source/Tests/` directory
- [ ] Create `Tests.asmdef` assembly definition file for Unity tests
- [ ] Configure assembly to reference main game assemblies

**Step 2.2: Create Unity Test Runner Setup**
- [ ] Add `UnityEngine.TestTools` namespace imports in test files
- [ ] Create test assembly definition with proper references:
  - UnityEngine.TestTools
  - UnityEditor.TestTools
  - ChronoCiv main assembly

**Step 2.3: Create Sample Unit Tests**
- [ ] Create basic test for ResourceType enum validation
- [ ] Create test for Era transitions
- [ ] Create test for EventTrigger system
- [ ] Create test for NPC spawning logic

## Phase 3: Configure VS Code for Unity Testing
### Objective: Properly configure VS Code to work with Unity Test Framework

**Step 3.1: Update .vscode/settings.json**
- [ ] Add C# IDE settings for Unity development
- [ ] Configure OmniSharp for Unity
- [ ] Add test explorer settings (for when Unity tests are created)
- [ ] Set up proper code formatting rules

**Step 3.2: Create Editor Test Runner Script**
- [ ] Create `Editor/ChronoCivTestRunner.cs` 
- [ ] Add menu item to run tests from Unity Editor
- [ ] Configure test filtering by category

**Step 3.3: Documentation**
- [ ] Update README.md with testing instructions
- [ ] Document how to run tests in Unity
- [ ] Document how to create new tests

## Phase 4: Validation & Best Practices
### Objective: Ensure testing setup follows Unity best practices

**Step 4.1: Performance Validation**
- [ ] Verify no VS Code freezes on project load
- [ ] Confirm test execution doesn't impact game performance
- [ ] Check memory usage during test runs

**Step 4.2: Best Practices Implementation**
- [ ] Use [SetUp] and [TearDown] attributes appropriately
- [ ] Follow AAA pattern (Arrange, Act, Assert)
- [ ] Use Unity's `Assert` methods for Unity-specific assertions
- [ ] Keep tests isolated and independent

## Expected Outcomes
1. ✅ VS Code/Unity no longer freezes on project load
2. ✅ Proper Unity Test Framework infrastructure in place
3. ✅ Sample tests demonstrating testing patterns
4. ✅ Documentation for running and creating tests
5. ✅ Clean VS Code configuration optimized for Unity development

## Files to be Created/Modified

### New Files:
- `Source/Tests/ResourceTypeTests.cs` - Test resource type validation
- `Source/Tests/EraTests.cs` - Test era transitions  
- `Source/Tests/EventTriggerTests.cs` - Test event triggering
- `Source/Tests/NPCSpawnTests.cs` - Test NPC spawning
- `Source/Tests/Tests.asmdef` - Assembly definition for tests
- `Editor/ChronoCivTestRunner.cs` - Editor script for running tests

### Modified Files:
- `.vscode/settings.json` - Remove Python config, add Unity C# config
- `README.md` - Add testing documentation

### Files to Remove:
- Any incorrect test configurations pointing to wrong directories

