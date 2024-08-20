using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System.Diagnostics;
using System.Reflection;

namespace ImproveGame
{
    [HarmonyPatch]
    class SpriteTextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpriteText), "shrinkFont")]
        static void AfterShrinkFont(bool shrink)
        {
            var langMod = LocalizedContentManager.CurrentModLanguage;
            Console.WriteLine("SV: lang mod?: " + langMod);
            if (langMod == null)
                return;

            SpriteText.fontPixelZoom = langMod.FontPixelZoom;
            Console.WriteLine("SV: set font pixel zoom: " + SpriteText.fontPixelZoom);
        }
    }

    [HarmonyPatch]
    class FixJoystickCrash
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), "shouldClampGamePadCursor")]
        static void PurchaseAnimalsMenushouldClampGamePadCursor(ref bool __result)
        {
            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CarpenterMenu), "shouldClampGamePadCursor")]
        static void CarpenterMenushouldClampGamePadCursor(ref bool __result)
        {
            __result = false;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MuseumMenu), "shouldClampGamePadCursor")]
        static void MeseumMenushouldClampGamePadCursor(ref bool __result)
        {
            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RenovateMenu), "shouldClampGamePadCursor")]
        static void RenovateMenushouldClampGamePadCursor(ref bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch]
    class MuseumMenuPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputState), "SetMousePosition")]
        static void PostfixSetMousePosition(int x, int y)
        {
            Console.WriteLine($"[Fixbug] Postfix Set Mouse Pos: : " + x + ", " + y);
            var stacktrace = new StackTrace();
            bool flag3 = false;
            if (Game1.options.gamepadControls && Game1.activeClickableMenu != null && Game1.activeClickableMenu.shouldClampGamePadCursor())
            {
                flag3 = true;
                Console.WriteLine("Fixbug] is should set mosue pos raw");
            }
            //foreach (var f in stacktrace.GetFrames())
            //{
            //    var method = f.GetMethod();
            //    Console.WriteLine($"[Fixbug] frame: {method.DeclaringType}.{method.Name}");
            //}
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputState), "UpdateStates")]
        static void PostfixUpdateStates()
        {
            //return;

            var touchState = Game1.input.GetTouchState;
            if (touchState.Count > 0)
            {
                Console.WriteLine($"[Fixbug] Postfix Update States Ticks: " + Game1.ticks);
                Console.WriteLine($"[Fixbug] touch state count: {touchState.Count}");
                TouchLocation touchLocation = touchState[0];
                Console.WriteLine("[Fixbug] touch 0 pos: " + touchLocation.Position);
                var mouseState = Game1.input.GetMouseState();
                Console.WriteLine("[Fixbug] GmaeInput mouse state pose: " + mouseState.Position);

                var inputStateType = typeof(InputState);
                var fieldInfo = inputStateType.GetField("_currentMouseState",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var currentMouseStateValue = (MouseState)fieldInfo.GetValue(Game1.input);
                Console.WriteLine("[Fixbug] current mouse state pose: " + mouseState.Position);
                Console.WriteLine("[Fixbug] is game pad controlls: " + Game1.options.gamepadControls);
                var menu = Game1.activeClickableMenu;
                if (menu != null)
                {
                    Console.WriteLine("[Fixbug] shouldClampGamePadCursor: " + Game1.activeClickableMenu.shouldClampGamePadCursor());
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MuseumMenu), "receiveLeftClick")]
        static void PrefixreceiveLeftClick(int x, int y, bool playSound = true)
        {
            return;

            var menu = Game1.activeClickableMenu as MuseumMenu;
            Console.WriteLine("[NRT Fixbug] Prefix receiveLeftClick");

            var btn = menu.upperRightCloseButton;
            bool isCanClose = btn != null && btn.containsPoint(x, y);
            bool isClickAtRectBTN = btn.containsPoint(x, y);
            Console.WriteLine("[Fixbug] isCanClose: " + isCanClose);
            Console.WriteLine($"[Fixbug] click x: {x},y: {y}");
            Console.WriteLine($"[Fixbug] mouse pos: {Game1.getMousePosition()}");
            var touchState = Game1.input.GetTouchState;
            Console.WriteLine($"[Fixbug] touch state count: {touchState.Count}");
            TouchLocation touchLocation = touchState[0];
            Console.WriteLine("[Fixbug] touch 0 pos: " + touchLocation.Position);
            var mouseState = Game1.input.GetMouseState();
            Console.WriteLine("Mouse State position: " + mouseState.Position);
            if (!Game1.game1.IsMainInstance)
            {
                Console.WriteLine("is not main instance!!!");
            }
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
        public static void HookEndDraw(SpriteBatch b)
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
                }
            }
        }

    }
}
