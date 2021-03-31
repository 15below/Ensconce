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
ECHO dotnet Restore
ECHO ----------------------------
dotnet restore .\src --no-cache
if errorlevel 1 (
  GOTO :end
)

:end
exit /b %errorlevel%
