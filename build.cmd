@echo off

set target=%~1
set packagePath=.\packages
set testResultsPath="%~dp0TestResults"
set codeCoveragePath=%testResultsPath%\Coverage

if "%target%" equ "" set "target=restore;build;test;pack"

:: Restore nuget packages
if "%target:restore=%" neq "%target%" (
  echo Restoring nuget packages
  dotnet restore CodeGerminator.sln --no-cache

  if ERRORLEVEL 1 exit /B 1
  echo ****************************************************************************************************************
)

:: Build solution 
if "%target:build=%" neq "%target%" (
  echo Building solution
  dotnet build CodeGerminator.sln -c Release --no-restore /fl /flp:logfile=out\build.log;verbosity=diagnostic -v=m
  if ERRORLEVEL 1 exit /B 1
  echo ****************************************************************************************************************
)

:: Test solution
if "%target:test=%" neq "%target%" (
  echo Testing solution
  
  dotnet test CodeGerminator.sln -c Release -f net6.0 /fl /flp:logfile=out\tests.log;verbosity=diagnostic -v=m

  if ERRORLEVEL 1 exit /B 1
  echo ****************************************************************************************************************
)

:: Generate nuget packages
if "%target:pack=%" neq "%target%" (
  echo Packaging
  nuget pack .\src\CodeGerminator\CodeGerminator.nuspec -OutputDirectory %packagePath%

  if ERRORLEVEL 1 exit /B 1
)

exit /B 0
