@echo off
setlocal
color 07
cls

ECHO ----------------------------
ECHO Restore Tools
ECHO ----------------------------
dotnet tool restore
if errorlevel 1 (
  GOTO :end
)

ECHO ----------------------------
ECHO Run Cake Bootstrap
ECHO ----------------------------
dotnet cake build.cake --bootstrap
if errorlevel 1 (
  GOTO :end
)

ECHO ----------------------------
ECHO Run Cake
ECHO ----------------------------
dotnet cake build.cake %*

:end
exit /b %errorlevel%
