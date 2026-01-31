using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools;

/// <summary>
/// ChronoCiv Test Runner - Editor menu for running Unity tests
/// Provides convenient access to test execution from Unity Editor menu
/// </summary>
namespace ChronoCiv.Editor
{
    public class ChronoCivTestRunner
    {
        // Menu items for running tests
        [MenuItem("ChronoCiv/Tests/Run All Tests")]
        public static void RunAllTests()
        {
            Debug.Log("ChronoCiv: Starting all tests...");
            var runner = EditorApplication.isPlaying ?
                "PlayMode tests cannot run in play mode" :
                "Use Unity Test Runner (Window > General > Test Runner) to execute tests";

            Debug.Log($"ChronoCiv: {runner}");
            EditorUtility.DisplayDialog("Test Runner",
                "Please use Unity Test Runner to execute tests:\n\n" +
                "1. Open Test Runner: Window > General > Test Runner\n" +
                "2. Select 'Run All' or choose specific tests\n" +
                "Tests are located in: Source/Tests/",
                "OK");
        }

        [MenuItem("ChronoCiv/Tests/Run Resource Tests")]
        public static void RunResourceTests()
        {
            Debug.Log("ChronoCiv: Filtered test execution - Resource Tests");
            EditorUtility.DisplayDialog("Resource Tests",
                "To run Resource tests:\n\n" +
                "1. Open Test Runner: Window > General > Test Runner\n" +
                "2. Expand 'ChronoCiv.Core.Tests'\n" +
                "3. Select 'ResourceTypeTests'\n" +
                "4. Click 'Run Selected'",
                "OK");
        }

        [MenuItem("ChronoCiv/Tests/Run Era Tests")]
        public static void RunEraTests()
        {
            Debug.Log("ChronoCiv: Filtered test execution - Era Tests");
            EditorUtility.DisplayDialog("Era Tests",
                "To run Era tests:\n\n" +
                "1. Open Test Runner: Window > General > Test Runner\n" +
                "2. Expand 'ChronoCiv.Core.Tests'\n" +
                "3. Select 'EraTests'\n" +
                "4. Click 'Run Selected'",
                "OK");
        }

        [MenuItem("ChronoCiv/Tests/Run NPC Tests")]
        public static void RunNPCTests()
        {
            Debug.Log("ChronoCiv: Filtered test execution - NPC Tests");
            EditorUtility.DisplayDialog("NPC Tests",
                "To run NPC tests:\n\n" +
                "1. Open Test Runner: Window > General > Test Runner\n" +
                "2. Expand 'ChronoCiv.Core.Tests'\n" +
                "3. Select 'NPCTests'\n" +
                "4. Click 'Run Selected'",
                "OK");
        }

        [MenuItem("ChronoCiv/Tests/Test Documentation")]
        public static void ShowTestDocumentation()
        {
            string documentation = @"CHRONOCIV TESTING DOCUMENTATION
================================

LOCATION
--------
Tests are located in: Source/Tests/

TEST STRUCTURE
--------------
- ChronoCiv.Core.Tests.asmdef - Test assembly definition
- ResourceTypeTests.cs - Tests for resource system
- EraTests.cs - Tests for era progression
- NPCTests.cs - Tests for NPC system

RUNNING TESTS
-------------
1. Open Unity Test Runner:
   Window > General > Test Runner

2. Choose test mode:
   - Edit Mode: For unit tests (most tests)
   - Play Mode: For integration tests

3. Run tests:
   - Click 'Run All' for all tests
   - Select specific tests and click 'Run Selected'

4. View results:
   - Green checkmark = Pass
   - Red X = Fail
   - Click test for details

TEST CATEGORIES
---------------
Tests are tagged with categories:
- Resource System
- Era System  
- NPC System
- Event System (coming soon)
- Animation System (coming soon)

TROUBLESHOOTING
---------------
If tests don't appear:
1. Reimport the project: Assets > Reimport All
2. Check assembly definition is set up correctly
3. Ensure Unity Test Framework package is installed

ADDING NEW TESTS
----------------
1. Create new test file in Source/Tests/
2. Add [Test] attribute to test methods
3. Use [Category("")] for organization
4. Use [Description("")] for documentation
5. Follow AAA pattern: Arrange, Act, Assert

BEST PRACTICES
--------------
- Keep tests isolated and independent
- Use descriptive test names
- Test one thing per test
- Use assertions to verify expected outcomes
- Group related tests in same class";

            Debug.Log($"ChronoCiv Test Documentation:\n{documentation}");

            // Create a temporary asset to display documentation
            var tempAsset = new TextAsset(documentation);
            string path = "Assets/Temp_TestDocumentation.txt";
            AssetDatabase.CreateAsset(tempAsset, path);
            AssetDatabase.OpenAsset(tempAsset);
            AssetDatabase.DeleteAsset(path);
        }

        [MenuItem("ChronoCiv/Tests/Validate Setup")]
        public static void ValidateTestSetup()
        {
            bool isValid = true;
            var validationReport = new System.Text.StringBuilder();

            validationReport.AppendLine("CHRONOCIV TEST SETUP VALIDATION");
            validationReport.AppendLine("=================================\n");

            // Check test assembly definition
            string asmdefPath = "Assets/Source/Tests/ChronoCiv.Core.Tests.asmdef";
            if (System.IO.File.Exists(asmdefPath))
            {
                validationReport.AppendLine("✓ Test assembly definition found");
            }
            else
            {
                validationReport.AppendLine("✗ Test assembly definition missing");
                isValid = false;
            }

            // Check test files exist
            int testFileCount = 0;
            foreach (string file in System.IO.Directory.GetFiles("Assets/Source/Tests", "*.cs"))
            {
                if (file.Contains("Tests.cs"))
                {
                    testFileCount++;
                }
            }

            if (testFileCount >= 3)
            {
                validationReport.AppendLine($"✓ Test files found: {testFileCount}");
            }
            else
            {
                validationReport.AppendLine($"✗ Insufficient test files: {testFileCount}");
                isValid = false;
            }

            // Check Unity version compatibility
            validationReport.AppendLine($"\nUnity Version: {Application.unityVersion}");
            validationReport.AppendLine($"Test Framework: Unity Test Framework 1.1+");

            validationReport.AppendLine($"\nOVERALL STATUS: {(isValid ? "VALID" : "ISSUES FOUND")}");

            Debug.Log(validationReport.ToString());

            EditorUtility.DisplayDialog("Test Setup Validation",
                validationReport.ToString(),
                isValid ? "✓ Valid" : "✗ Issues Found");
        }
    }
}
