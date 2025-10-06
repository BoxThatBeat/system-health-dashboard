@echo off
REM Script to generate HTML code coverage reports for .NET test projects on Windows
REM This script runs tests with code coverage and generates HTML reports using ReportGenerator

setlocal enabledelayedexpansion

echo === .NET Code Coverage Report Generator ===
echo.

REM Check if reportgenerator is installed
where reportgenerator >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: reportgenerator is not installed.
    echo Install it using: dotnet tool install --global dotnet-reportgenerator-globaltool
    exit /b 1
)

REM Get the script directory
set SCRIPT_DIR=%~dp0
cd /d "%SCRIPT_DIR%"

REM Clean up old coverage reports
echo Cleaning up old coverage reports...
if exist coverage-reports rmdir /s /q coverage-reports

REM Test projects to process
set TEST_PROJECTS=src\SystemHealthAPITests\SystemHealthAPITests.csproj src\OSMetricsRetrieverTests\OSMetricsRetrieverTests.csproj

for %%P in (%TEST_PROJECTS%) do (
    set TEST_PROJECT=%%P
    
    REM Extract project name from path
    for %%F in (!TEST_PROJECT!) do set PROJECT_NAME=%%~nF
    
    echo.
    echo Processing !PROJECT_NAME!...
    
    REM Clean up old test results
    set PROJECT_DIR=%%~dpP
    if exist "!PROJECT_DIR!bin\Release\net8.0\TestResults" rmdir /s /q "!PROJECT_DIR!bin\Release\net8.0\TestResults"
    
    REM Run tests with code coverage
    echo Running tests with code coverage...
    dotnet test "!TEST_PROJECT!" --configuration Release -- --coverage --coverage-output-format cobertura
    if !errorlevel! neq 0 (
        echo Failed to run tests for !PROJECT_NAME!
        continue
    )
    
    REM Find the generated cobertura.xml file
    for /f "delims=" %%i in ('dir /s /b "!PROJECT_DIR!bin\Release\*.cobertura.xml"') do set COVERAGE_FILE=%%i
    
    if "!COVERAGE_FILE!"=="" (
        echo No coverage file found for !PROJECT_NAME!
        continue
    )
    
    echo Coverage file: !COVERAGE_FILE!
    
    REM Generate HTML report
    echo Generating HTML report...
    set OUTPUT_DIR=coverage-reports\!PROJECT_NAME!
    reportgenerator -reports:"!COVERAGE_FILE!" -targetdir:"!OUTPUT_DIR!" -reporttypes:Html
    
    echo Report generated: !OUTPUT_DIR!\index.html
)

echo.
echo === Code coverage reports generated successfully! ===
echo.
echo To view the reports, open the following files in your browser:
for %%P in (%TEST_PROJECTS%) do (
    for %%F in (%%P) do (
        set PROJECT_NAME=%%~nF
        set REPORT_FILE=coverage-reports\!PROJECT_NAME!\index.html
        if exist "!REPORT_FILE!" echo   - !REPORT_FILE!
    )
)

endlocal
