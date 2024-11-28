﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

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

public static class DayTimeMoneyBoxThaiFormat
{
    public static void Init(Harmony harmony)
    {
        {
            var DayTimeMoneyBoxDrawMethod = typeof(DayTimeMoneyBox).GetMethod("draw", [typeof(SpriteBatch)]);
            harmony.Patch(
                original: DayTimeMoneyBoxDrawMethod,
                prefix: new(typeof(DayTimeMoneyBoxThaiFormat).GetMethod(nameof(PrefixSpriteBatchDraw))));
        }
        {
            var UtilityTypeInfo = typeof(Utility);
            Type[] paramTypes =
            [
                typeof(SpriteBatch), typeof(string), typeof(SpriteFont),
                typeof(Vector2), typeof(Color), typeof(float), typeof(float),
                typeof(int), typeof(int), typeof(float), typeof(int)
            ];
            var drawTextWithShadowMethod = UtilityTypeInfo.GetMethod(nameof(Utility.drawTextWithShadow), paramTypes);
            harmony.Patch(
                original: drawTextWithShadowMethod,
                prefix: new(typeof(DayTimeMoneyBoxThaiFormat).GetMethod(nameof(PrefixDrawTextWithShadow))));
        }
    }
    public static int CallStack_drawTextWithShadow_Count = 0;

    public static void PrefixSpriteBatchDraw(SpriteBatch b)
    {
        CallStack_drawTextWithShadow_Count = 0;
    }

    public static void PrefixDrawTextWithShadow(SpriteBatch b, ref string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f,
        int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
    {
        CallStack_drawTextWithShadow_Count++;

        //public override void draw(SpriteBatch b)
        //line:297: Utility.drawTextWithShadow(b, dateText,
        if (CallStack_drawTextWithShadow_Count == 2)
        {
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
            {
                text = LocalizedContentManager.FormatTimeString(Game1.timeOfDay,
                    LocalizedContentManager.CurrentModLanguage.TimeFormat).ToString();
            }
        }
    }

}
