using HarmonyLib;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace ImproveGame;

[HarmonyPatch]
class SpriteTextPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpriteText), "shrinkFont")]
    static void AfterShrinkFont(bool shrink)
    {
        var langMod = LocalizedContentManager.CurrentModLanguage;
        if (langMod == null)
            return;

        SpriteText.fontPixelZoom = langMod.FontPixelZoom;
    }
}
