@echo off
setlocal
color 07
pushd %~dp0
cd ..
cls

ECHO ----------------------------
ECHO Restore Tools
ECHO ----------------------------
dotnet tool restore --no-cache
if errorlevel 1 (
  GOTO :end
)

ECHO ----------------------------
ECHO Update 15below.cake
ECHO ----------------------------
dotnet tool update cake.tool --no-cache
if errorlevel 1 (
  GOTO :end
)

ECHO ----------------------------
ECHO Update dotnet-outdated-tool
ECHO ----------------------------
dotnet tool update dotnet-outdated-tool --no-cache
if errorlevel 1 (
  GOTO :end
)

ECHO ----------------------------
ECHO Update NuGets
ECHO ----------------------------
dotnet dotnet-outdated -u .\src
if errorlevel 1 (
  GOTO :end
)

:end
exit /b %errorlevel%
