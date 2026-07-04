using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ImproveGame;

public static class DayTimeMoneyBoxThaiFormat
{
    public static bool IsApplyPatch { get; private set; } = false;
    public static void ApplyPatch(Harmony harmony)
    {
        if (IsApplyPatch)
            return;

        IsApplyPatch = true;
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
            // the HUD clock should use ClockTimeFormat (like desktop DayTimeMoneyBox does),
            // falling back to TimeFormat if the language pack doesn't define it
            var modLanguage = LocalizedContentManager.CurrentModLanguage;
            var format = !string.IsNullOrWhiteSpace(modLanguage.ClockTimeFormat)
                ? modLanguage.ClockTimeFormat
                : modLanguage.TimeFormat;
            text = LocalizedContentManager.FormatTimeString(Game1.timeOfDay, format).ToString();
        }
    }

}
