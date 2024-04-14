using HarmonyLib;
using ImproveGame.Patcher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SMAPI.Toolkit")]



namespace ImproveGame.Patcher
{
    [HarmonyPatch(typeof(SpriteText))]
    class SpriteTextPatch : IModPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("shrinkFont")]
        public static void AfterShrinkFont(bool shrink)
        {
            var langMod = LocalizedContentManager.CurrentModLanguage;
            if (langMod == null) return;

            SpriteText.fontPixelZoom = langMod.FontPixelZoom;
        }
    }
    public class DayTimeMoneyBoxPatch : IModPatcher
    {
        public DayTimeMoneyBoxPatch()
        {
            {
                var DayTimeMoneyBoxType = typeof(DayTimeMoneyBox);
                var DayTimeMoneyBoxDrawMethod = DayTimeMoneyBoxType.GetMethod("draw", new Type[] { typeof(SpriteBatch) });
                Patch(DayTimeMoneyBoxDrawMethod, prefix: nameof(HookStartDraw), postfix: nameof(HookEndDraw));
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
                Patch(drawTextWithShadowMethod, nameof(PrefixDrawTextWithShadow));
            }
        }
        public static int CallStack_drawTextWithShadow_Count = 0;

        public static void HookStartDraw(DayTimeMoneyBox __instance, SpriteBatch b)
        {
            CallStack_drawTextWithShadow_Count = 0;
        }
        public static void HookEndDraw(DayTimeMoneyBox __instance, SpriteBatch b)
        {
            //Log($"done draw stack: {CallStack_drawTextWithShadow_Count}");
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
                    return;
                }
            }

        }
    }

}
namespace ImproveGame
{
    public abstract class IModPatcher
    {

        public static ModEntry modEntry => ModEntry.Instance;
        public static Harmony harmony => modEntry.Harmony;
        public static void Log(string msg) => ModEntry.Log(msg);
        public static MethodInfo Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
            => harmony.Patch(original, prefix, postfix);

        public static MethodInfo Patch(MethodBase original, Type patcherType, string prefix = default, string postfix = default)
            //prefix function with self name
            => harmony.Patch(original,
                prefix != default ? new(patcherType.GetMethod(prefix)) : null,
                postfix != default ? new(patcherType.GetMethod(postfix)) : null);

        public Type[] GetParameterTypes(Type classType, string funcName)
            => classType.GetMethod(funcName).GetParameters().
            Where(p => p.Name[0] != '_').
            Select(p => p.ParameterType).ToArray();

        public Type[] GetParameterTypes(string funcName) => GetParameterTypes(GetType(), funcName);

        public MethodInfo Patch(MethodBase original, string prefix = default, string postfix = default)
            => Patch(original, this.GetType(), prefix, postfix);
    }

    //Fix Bug & Pactch Code within Internal Game System
    public class GameMobilePatcher : IModPatcher
    {
        public static GameMobilePatcher Instance { get; private set; }
        Harmony harmony;
        DayTimeMoneyBoxPatch dayTimeMoneyBoxPatch;
        public GameMobilePatcher()
        {
            Instance = this;
            harmony = modEntry.Harmony;
            //patch all with attribute
            harmony.PatchAll();

            //Patch all manual
            dayTimeMoneyBoxPatch = new();
        }
    }
}
