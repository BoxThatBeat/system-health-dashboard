#!/bin/bash

# Script to generate HTML code coverage reports for .NET test projects
# This script runs tests with code coverage and generates HTML reports using ReportGenerator

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Get the directory where the script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo -e "${BLUE}=== .NET Code Coverage Report Generator ===${NC}\n"

# Check if reportgenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo -e "${RED}Error: reportgenerator is not installed.${NC}"
    echo "Install it using: dotnet tool install --global dotnet-reportgenerator-globaltool"
    exit 1
fi

# Clean up old coverage reports
echo -e "${BLUE}Cleaning up old coverage reports...${NC}"
rm -rf coverage-reports

# Test projects to process
TEST_PROJECTS=(
    "src/SystemHealthAPITests/SystemHealthAPITests.csproj"
    "src/OSMetricsRetrieverTests/OSMetricsRetrieverTests.csproj"
)

# Process each test project
for TEST_PROJECT in "${TEST_PROJECTS[@]}"; do
    PROJECT_NAME=$(basename "$TEST_PROJECT" .csproj)
    PROJECT_DIR=$(dirname "$TEST_PROJECT")
    
    echo -e "\n${BLUE}Processing $PROJECT_NAME...${NC}"
    
    # Skip OSMetricsRetrieverTests if not on Windows
    if [[ "$PROJECT_NAME" == "OSMetricsRetrieverTests" ]] && [[ "$(uname -s)" != "CYGWIN"* ]] && [[ "$(uname -s)" != "MINGW"* ]] && [[ "$(uname -s)" != "MSYS"* ]]; then
        echo -e "${RED}Skipping $PROJECT_NAME (Windows-only project, current OS: $(uname -s))${NC}"
        continue
    fi
    
    # Clean up old test results
    rm -rf "$PROJECT_DIR/bin/Release/net8.0/TestResults"
    
    # Run tests with code coverage
    echo "Running tests with code coverage..."
    if ! dotnet test "$TEST_PROJECT" --configuration Release -- --coverage --coverage-output-format cobertura; then
        echo -e "${RED}Failed to run tests for $PROJECT_NAME${NC}"
        continue
    fi
    
    # Find the generated cobertura.xml file
    COVERAGE_FILE=$(find "$PROJECT_DIR/bin/Release" -name "*.cobertura.xml" | head -1)
    
    if [ -z "$COVERAGE_FILE" ]; then
        echo -e "${RED}No coverage file found for $PROJECT_NAME${NC}"
        continue
    fi
    
    echo "Coverage file: $COVERAGE_FILE"
    
    # Generate HTML report
    echo "Generating HTML report..."
    OUTPUT_DIR="coverage-reports/$PROJECT_NAME"
    reportgenerator \
        -reports:"$COVERAGE_FILE" \
        -targetdir:"$OUTPUT_DIR" \
        -reporttypes:Html
    
    echo -e "${GREEN}✓ Report generated: $OUTPUT_DIR/index.html${NC}"
done

echo -e "\n${GREEN}=== Code coverage reports generated successfully! ===${NC}"
echo -e "\nTo view the reports, open the following files in your browser:"
for TEST_PROJECT in "${TEST_PROJECTS[@]}"; do
    PROJECT_NAME=$(basename "$TEST_PROJECT" .csproj)
    REPORT_FILE="coverage-reports/$PROJECT_NAME/index.html"
    if [ -f "$REPORT_FILE" ]; then
        echo "  - $REPORT_FILE"
    fi
done
