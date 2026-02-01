#!/bin/bash

# ChronoCiv Test Runner Script
# Run this script to execute tests via Unity Editor

echo "=========================================="
echo "ChronoCiv Test Runner"
echo "=========================================="
echo ""
echo "This script helps you run Unity tests for ChronoCiv."
echo ""
echo "Options:"
echo "1. Run all tests"
echo "2. Run resource tests only"
echo "3. Run era tests only"
echo "4. Run NPC tests only"
echo "5. Validate test setup"
echo "6. Open Unity Test Runner"
echo ""
echo "Note: In Unity Editor, use the menu:"
echo "      ChronoCiv > Tests > Run All Inline"
echo ""
echo "Or use Unity Test Runner directly:"
echo "      Window > General > Test Runner"
echo ""

# Check if Unity Editor is available
if command -v unity &> /dev/null; then
    echo "Unity Editor found!"
else
    echo "Note: Unity Editor should be opened manually."
    echo "This script documents the testing workflow only."
fi

echo ""
echo "Test files location: Source/Tests/"
echo "- ResourceTypeTests.cs (5 tests)"
echo "- EraTests.cs (11 tests)"
echo "- NPCTests.cs (14 tests)"
echo ""
echo "Total: 30 unit tests"
echo "=========================================="

