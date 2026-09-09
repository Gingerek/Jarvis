@echo off
setlocal
set "SOURCE=%~dp0..\integrations\lightroom\Jarvis.lrplugin"
set "TARGET=%APPDATA%\Adobe\Lightroom\Modules\Jarvis.lrplugin"

if not exist "%SOURCE%\Info.lua" (
  echo Jarvis Lightroom Bridge source not found.
  exit /b 2
)
if not exist "%TARGET%" mkdir "%TARGET%"
copy /y "%SOURCE%\Info.lua" "%TARGET%\Info.lua" >nul
copy /y "%SOURCE%\InitPlugin.lua" "%TARGET%\InitPlugin.lua" >nul
copy /y "%SOURCE%\MenuStatus.lua" "%TARGET%\MenuStatus.lua" >nul
if errorlevel 1 exit /b 1
echo Jarvis Lightroom Bridge installed.
echo Restart Lightroom Classic to load updates.
exit /b 0