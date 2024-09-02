using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace ImproveGame;

static class DisableQuickSave
{
    static bool _initialized;
    public static void TryInit(Harmony harmony)
    {
        if (_initialized)
            return;

        _initialized = true;

        var updateContentPositionsMethod = OptionsPageType.GetMethod("updateContentPositions",
            BindingFlags.Instance | BindingFlags.NonPublic);
        harmony.Patch(updateContentPositionsMethod,
            prefix: new(typeof(DisableQuickSave).GetMethod(nameof(Prefix_updateContentPositions))));

        var saveWholeBackup = typeof(Game1).GetMethod(nameof(Game1.saveWholeBackup));
        harmony.Patch(saveWholeBackup,
            prefix: new(typeof(DisableQuickSave).GetMethod(nameof(Prefix_saveWholeBackup))));
    }
    static Type OptionsPageType = typeof(OptionsPage);
    static FieldInfo optionsFieldInfo = OptionsPageType.GetField("options",
        BindingFlags.Instance | BindingFlags.NonPublic);

    static Type OptionsButtonType = typeof(OptionsButton);
    static FieldInfo btnLabelFieldInfo = OptionsButtonType.GetField("_label",
        BindingFlags.Instance | BindingFlags.NonPublic);
    static FieldInfo btnPaddingYField = OptionsButtonType.GetField("paddingY",
        BindingFlags.Instance | BindingFlags.NonPublic);

    const string ButtonBlockLabelText = "Block QuickSave On Mod SpaceCore";
    static OptionsButton lastEditButton;
    public static void Prefix_updateContentPositions(OptionsPage __instance)
    {
        var options = optionsFieldInfo.GetValue(__instance) as List<OptionsElement>;
        if (options[2] is OptionsButton btn)
        {
            if (lastEditButton == btn)
                return;

            lastEditButton = btn;

            btnLabelFieldInfo.SetValue(btn, ButtonBlockLabelText);
            var paddingY = (int)btnPaddingYField.GetValue(btn);
            btn.enabled = false;

            int num = (int)Game1.dialogueFont.MeasureString(ButtonBlockLabelText).X + 64;
            int num2 = (int)Game1.dialogueFont.MeasureString(ButtonBlockLabelText).Y + paddingY * 2;
            btn.bounds = new Rectangle(btn.bounds.X, btn.bounds.Y, num, num2);
            btn.button = new ClickableComponent(btn.bounds, "OptionsButton_" + ButtonBlockLabelText);
        }
    }

    public static bool Prefix_saveWholeBackup()
    {
        //Console.WriteLine("fixbug Cancel call func saveWholeBackup()");
        return false;
    }
}
