using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools;

namespace ChronoCiv.Editor
{
    public class ChronoCivTestRunner
    {
        [MenuItem("ChronoCiv/Tests/Run All Tests")]
        public static void RunAllTests()
        {
            Debug.Log("ChronoCiv: Starting all tests...");

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Debug.Log("ChronoCiv: Test assembly: " + assembly.FullName);

            int testClassCount = 0;
            int testMethodCount = 0;

            var testTypes = System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && t.GetMethod("Test") != null && t.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Length > 0);

            foreach (var type in testTypes)
            {
                testClassCount++;
                var methods = type.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Length > 0);
                testMethodCount += methods.Count();
            }

            Debug.Log("ChronoCiv: Found " + testClassCount + " test classes with " + testMethodCount + " test methods");

            EditorUtility.DisplayDialog("Test Runner",
                "Found " + testClassCount + " test classes with " + testMethodCount + " test methods.\n\n" +
                "To run tests in Unity Editor:\n" +
                "1. Open Test Runner: Window > General > Test Runner\n" +
                "2. Select Edit Mode tab\n" +
                "3. Click Run All or select specific tests\n\n" +
                "Tests are located in: Source/Tests/",
                "OK");
        }

        [MenuItem("ChronoCiv/Tests/Run Resource Tests")]
        public static void RunResourceTests()
        {
            Debug.Log("ChronoCiv: Running Resource System tests...");

            int passCount = 0;
            int failCount = 0;

            var resourceTests = new ChronoCiv.Tests.ResourceTypeTests();

            try
            {
                resourceTests.ResourceType_CanBeInstantiated();
                passCount++;
                Debug.Log("PASS: ResourceType_CanBeInstantiated");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: ResourceType_CanBeInstantiated - " + e.Message);
            }

            try
            {
                resourceTests.ResourceType_PropertyAccess();
                passCount++;
                Debug.Log("PASS: ResourceType_PropertyAccess");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: ResourceType_PropertyAccess - " + e.Message);
            }

            try
            {
                resourceTests.ResourceType_DifferentCategories();
                passCount++;
                Debug.Log("PASS: ResourceType_DifferentCategories");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: ResourceType_DifferentCategories - " + e.Message);
            }

            try
            {
                resourceTests.ResourceType_Renewability();
                passCount++;
                Debug.Log("PASS: ResourceType_Renewability");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: ResourceType_Renewability - " + e.Message);
            }

            try
            {
                resourceTests.ResourceType_ValueAndWeight();
                passCount++;
                Debug.Log("PASS: ResourceType_ValueAndWeight");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: ResourceType_ValueAndWeight - " + e.Message);
            }

            DisplayTestResults("Resource Tests", passCount, failCount);
        }

        [MenuItem("ChronoCiv/Tests/Run Era Tests")]
        public static void RunEraTests()
        {
            Debug.Log("ChronoCiv: Running Era System tests...");

            int passCount = 0;
            int failCount = 0;

            var eraTests = new ChronoCiv.Tests.EraTests();

            try
            {
                eraTests.Era_DataStructureInitialization();
                passCount++;
                Debug.Log("PASS: Era_DataStructureInitialization");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_DataStructureInitialization - " + e.Message);
            }

            try
            {
                eraTests.Era_PropertyAccess();
                passCount++;
                Debug.Log("PASS: Era_PropertyAccess");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_PropertyAccess - " + e.Message);
            }

            try
            {
                eraTests.Era_YearRangeValidation();
                passCount++;
                Debug.Log("PASS: Era_YearRangeValidation");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_YearRangeValidation - " + e.Message);
            }

            try
            {
                eraTests.Era_DurationCalculation();
                passCount++;
                Debug.Log("PASS: Era_DurationCalculation");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_DurationCalculation - " + e.Message);
            }

            try
            {
                eraTests.Era_DurationProperty();
                passCount++;
                Debug.Log("PASS: Era_DurationProperty");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_DurationProperty - " + e.Message);
            }

            try
            {
                eraTests.Era_ContainsYear();
                passCount++;
                Debug.Log("PASS: Era_ContainsYear");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_ContainsYear - " + e.Message);
            }

            try
            {
                eraTests.Era_GetProgress();
                passCount++;
                Debug.Log("PASS: Era_GetProgress");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Era_GetProgress - " + e.Message);
            }

            DisplayTestResults("Era Tests", passCount, failCount);
        }

        [MenuItem("ChronoCiv/Tests/Run NPC Tests")]
        public static void RunNPCTests()
        {
            Debug.Log("ChronoCiv: Running NPC System tests...");

            int passCount = 0;
            int failCount = 0;

            var npcTests = new ChronoCiv.Tests.NPCTests();

            try
            {
                npcTests.NPC_DataStructureInitialization();
                passCount++;
                Debug.Log("PASS: NPC_DataStructureInitialization");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: NPC_DataStructureInitialization - " + e.Message);
            }

            try
            {
                npcTests.NPCState_EnumValues();
                passCount++;
                Debug.Log("PASS: NPCState_EnumValues");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: NPCState_EnumValues - " + e.Message);
            }

            try
            {
                npcTests.Direction_EnumValues();
                passCount++;
                Debug.Log("PASS: Direction_EnumValues");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Direction_EnumValues - " + e.Message);
            }

            try
            {
                npcTests.Task_DataStructure();
                passCount++;
                Debug.Log("PASS: Task_DataStructure");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: Task_DataStructure - " + e.Message);
            }

            try
            {
                npcTests.NPCProfile_DataStructure();
                passCount++;
                Debug.Log("PASS: NPCProfile_DataStructure");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError("FAIL: NPCProfile_DataStructure - " + e.Message);
            }

            DisplayTestResults("NPC Tests", passCount, failCount);
        }

        [MenuItem("ChronoCiv/Tests/Run All Inline")]
        public static void RunAllTestsInline()
        {
            Debug.Log("ChronoCiv: Running all tests inline...");

            int totalPass = 0;
            int totalFail = 0;

            // Run Resource Tests
            int resourcePass = 0, resourceFail = 0;
            RunResourceTestsInline(out resourcePass, out resourceFail);
            totalPass += resourcePass;
            totalFail += resourceFail;

            // Run Era Tests  
            int eraPass = 0, eraFail = 0;
            RunEraTestsInline(out eraPass, out eraFail);
            totalPass += eraPass;
            totalFail += eraFail;

            // Run NPC Tests
            int npcPass = 0, npcFail = 0;
            RunNPCTestsInline(out npcPass, out npcFail);
            totalPass += npcPass;
            totalFail += npcFail;

            DisplayTestResults("ALL TESTS", totalPass, totalFail);
        }

        private static void RunResourceTestsInline(out int passCount, out int failCount)
        {
            passCount = 0;
            failCount = 0;

            var tests = new ChronoCiv.Tests.ResourceTypeTests();

            var testMethods = new System.Action[]
            {
                () => tests.ResourceType_CanBeInstantiated(),
                () => tests.ResourceType_PropertyAccess(),
                () => tests.ResourceType_DifferentCategories(),
                () => tests.ResourceType_Renewability(),
                () => tests.ResourceType_ValueAndWeight()
            };

            foreach (var test in testMethods)
            {
                try
                {
                    test();
                    passCount++;
                }
                catch (System.Exception e)
                {
                    failCount++;
                    Debug.LogError("Test failed: " + e.Message);
                }
            }
        }

        private static void RunEraTestsInline(out int passCount, out int failCount)
        {
            passCount = 0;
            failCount = 0;

            var tests = new ChronoCiv.Tests.EraTests();

            var testMethods = new System.Action[]
            {
                () => tests.Era_DataStructureInitialization(),
                () => tests.Era_PropertyAccess(),
                () => tests.Era_YearRangeValidation(),
                () => tests.Era_DurationCalculation(),
                () => tests.Era_DurationProperty(),
                () => tests.Era_ContainsYear(),
                () => tests.Era_GetProgress(),
                () => tests.Era_GetColor(),
                () => tests.EraTransition_DataStructure(),
                () => tests.Era_BuildingsList(),
                () => tests.Era_TechnologyLevel()
            };

            foreach (var test in testMethods)
            {
                try
                {
                    test();
                    passCount++;
                }
                catch (System.Exception e)
                {
                    failCount++;
                    Debug.LogError("Test failed: " + e.Message);
                }
            }
        }

        private static void RunNPCTestsInline(out int passCount, out int failCount)
        {
            passCount = 0;
            failCount = 0;

            var tests = new ChronoCiv.Tests.NPCTests();

            var testMethods = new System.Action[]
            {
                () => tests.NPC_DataStructureInitialization(),
                () => tests.NPC_PropertyAccess(),
                () => tests.NPCState_EnumValues(),
                () => tests.Direction_EnumValues(),
                () => tests.Task_DataStructure(),
                () => tests.Task_TargetPosition(),
                () => tests.Task_NoTargetPosition(),
                () => tests.NPCProfile_DataStructure(),
                () => tests.NPCProfile_Animations(),
                () => tests.NPCProfile_SpriteSettings(),
                () => tests.NPCProfile_Clothing(),
                () => tests.NPCProfile_Equipment(),
                () => tests.NPCProfile_Skills(),
                () => tests.NPCProfile_DialogueSet()
            };

            foreach (var test in testMethods)
            {
                try
                {
                    test();
                    passCount++;
                }
                catch (System.Exception e)
                {
                    failCount++;
                    Debug.LogError("Test failed: " + e.Message);
                }
            }
        }

        private static void DisplayTestResults(string testCategory, int passCount, int failCount)
        {
            int total = passCount + failCount;
            string result = failCount == 0 ? "ALL PASSED" : failCount + " FAILED";

            Debug.Log("ChronoCiv: " + testCategory + " - " + passCount + "/" + total + " passed " + result);

            EditorUtility.DisplayDialog(testCategory,
                "Results: " + passCount + "/" + total + " passed\n\n" +
                result + "\n\n" +
                "Check Console for detailed output.",
                failCount == 0 ? "Great!" : "View Errors");
        }

        [MenuItem("ChronoCiv/Tests/Test Documentation")]
        public static void ShowTestDocumentation()
        {
            string documentation = "CHRONOCIV TESTING DOCUMENTATION\n" +
                "================================\n\n" +
                "LOCATION\n" +
                "--------\n" +
                "Tests are located in: Source/Tests/\n\n" +
                "TEST STRUCTURE\n" +
                "--------------\n" +
                "- ChronoCiv.Core.Tests.asmdef - Test assembly definition\n" +
                "- ResourceTypeTests.cs - Tests for resource system (5 tests)\n" +
                "- EraTests.cs - Tests for era progression (11 tests)\n" +
                "- NPCTests.cs - Tests for NPC system (14 tests)\n" +
                "- Total: 30 unit tests\n\n" +
                "RUNNING TESTS\n" +
                "-------------\n" +
                "Option 1: Unity Test Runner\n" +
                "1. Open Test Runner: Window > General > Test Runner\n" +
                "2. Choose test mode:\n" +
                "   - Edit Mode: For unit tests (recommended)\n" +
                "   - Play Mode: For integration tests\n" +
                "3. Run tests:\n" +
                "   - Click Run All for all tests\n" +
                "   - Select specific tests and click Run Selected\n\n" +
                "Option 2: ChronoCiv Menu\n" +
                "1. Open menu: ChronoCiv > Tests\n" +
                "2. Choose test category:\n" +
                "   - Run All Tests - Shows test summary\n" +
                "   - Run Resource Tests - Tests resource system\n" +
                "   - Run Era Tests - Tests era system\n" +
                "   - Run NPC Tests - Tests NPC system\n" +
                "   - Run All Inline - Runs all tests with inline execution\n\n" +
                "TEST CATEGORIES\n" +
                "---------------\n" +
                "- Resource System - Tests for ResourceType class\n" +
                "- Era System - Tests for Era and EraTransition classes\n" +
                "- NPC System - Tests for NPC, Task, NPCProfile classes\n\n" +
                "TROUBLESHOOTING\n" +
                "---------------\n" +
                "If tests do not appear:\n" +
                "1. Reimport the project: Assets > Reimport All\n" +
                "2. Check assembly definition is set up correctly\n" +
                "3. Ensure Unity Test Framework package is installed\n\n" +
                "If tests fail:\n" +
                "1. Check Console for error details\n" +
                "2. Verify class structures match expected format\n" +
                "3. Ensure all required fields are initialized\n\n" +
                "ADDING NEW TESTS\n" +
                "----------------\n" +
                "1. Create new test file in Source/Tests/\n" +
                "2. Add [Test] attribute to test methods\n" +
                "3. Use [Category] for organization\n" +
                "4. Use [Description] for documentation\n" +
                "5. Follow AAA pattern: Arrange, Act, Assert\n\n" +
                "BEST PRACTICES\n" +
                "--------------\n" +
                "- Keep tests isolated and independent\n" +
                "- Use descriptive test names\n" +
                "- Test one thing per test\n" +
                "- Use assertions to verify expected outcomes\n" +
                "- Group related tests in same class\n" +
                "- Use [Category] for test organization";

            Debug.Log("ChronoCiv Test Documentation:\n" + documentation);
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
                validationReport.AppendLine("OK: Test assembly definition found");
            }
            else
            {
                validationReport.AppendLine("ERROR: Test assembly definition missing");
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
                validationReport.AppendLine("OK: Test files found: " + testFileCount);
            }
            else
            {
                validationReport.AppendLine("ERROR: Insufficient test files: " + testFileCount);
                isValid = false;
            }

            // Count test methods
            int testMethodCount = 0;
            foreach (string file in System.IO.Directory.GetFiles("Assets/Source/Tests", "*.cs"))
            {
                string content = System.IO.File.ReadAllText(file);
                testMethodCount += System.Text.RegularExpressions.Regex.Matches(content, @"\[Test\]").Count;
            }

            if (testMethodCount >= 10)
            {
                validationReport.AppendLine("OK: Test methods found: " + testMethodCount);
            }
            else
            {
                validationReport.AppendLine("ERROR: Insufficient test methods: " + testMethodCount);
                isValid = false;
            }

            validationReport.AppendLine("\nUnity Version: " + Application.unityVersion);
            validationReport.AppendLine("Test Framework: Unity Test Framework 1.1+");

            validationReport.AppendLine("\nOVERALL STATUS: " + (isValid ? "VALID" : "ISSUES FOUND"));

            Debug.Log(validationReport.ToString());

            EditorUtility.DisplayDialog("Test Setup Validation",
                validationReport.ToString(),
                isValid ? "Valid" : "Issues Found");
        }
    }
}

