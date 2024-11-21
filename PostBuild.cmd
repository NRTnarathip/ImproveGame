set AppName=abc.smapi.gameloader
adb shell am force-stop %AppName%

if errorlevel 1 (
    echo ADB is not connected.
    exit /b
)

 adb shell mkdir  "/storage/emulated/0/Android/data/%AppName%/files/Mods/ImproveGame"
 adb push "bin/ARM64/Release/net8.0/ImproveGame.dll" "/storage/emulated/0/Android/data/%AppName%/files/Mods/ImproveGame"
 adb push "bin/ARM64/Release/net8.0/manifest.json" "/storage/emulated/0/Android/data/%AppName%/files/Mods/ImproveGame"


adb shell am start %AppName%"/crc644389b739a03c2b33.EntryActivity"
