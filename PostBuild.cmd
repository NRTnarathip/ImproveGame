echo my current folder excute %cd%
adb shell am force-stop com.nrt.fakestardewgame

if errorlevel 1 (
    echo ADB is not connected.
    exit /b
)

 adb shell mkdir  "/storage/emulated/0/Android/data/com.nrt.fakestardewgame/files/Mods/ImproveGame"
 adb push "bin/ARM64/Release/net8.0/ImproveGame.dll" "/storage/emulated/0/Android/data/com.nrt.fakestardewgame/files/Mods/ImproveGame"
 adb push "bin/ARM64/Release/net8.0/manifest.json" "/storage/emulated/0/Android/data/com.nrt.fakestardewgame/files/Mods/ImproveGame"


 adb shell am start com.nrt.fakestardewgame/crc641b6952874c843248.SMainActivity

