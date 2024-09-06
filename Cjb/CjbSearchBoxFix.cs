using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System.Reflection;

namespace ImproveGame.Cjb;

static class CjbSearchBoxFix
{
    static Type ItemMenu_Type;
    static FieldInfo Textbox_Field;
    static Type ThisType = typeof(CjbSearchBoxFix);
    public static void Init()
    {
        ItemMenu_Type = AccessTools.TypeByName("CJBItemSpawner.Framework.ItemMenu");
        Textbox_Field = AccessTools.Field(ItemMenu_Type, "Textbox");

        var harmony = ModEntry.Instance.harmony;
        var receiveLeftClickMethod = AccessTools.Method(ItemMenu_Type, nameof(InventoryMenu.receiveLeftClick));
        harmony.Patch(original: receiveLeftClickMethod,
            postfix: new(ThisType, nameof(Postfix_receiveLeftClick))
        );
    }
    static async void Postfix_receiveLeftClick(object __instance, int x, int y, bool playSound = true)
    {
        var Textbox = Textbox_Field.GetValue(__instance) as TextBox;
        Textbox.Selected = new Rectangle(Textbox.X, Textbox.Y, Textbox.Width, Textbox.Height).Contains(x, y);
        if (Textbox.Selected && !isShowKeyboard)
        {
            await ShowAndroidKeyboardAsync(Textbox);
        }
    }
    static bool isShowKeyboard = false;
    static async Task ShowAndroidKeyboardAsync(TextBox textBox)
    {
        isShowKeyboard = true;
        var resultTextInput = await KeyboardInput.Show("Search Item", "", textBox.Text);
        textBox.Selected = false;
        textBox.Text = resultTextInput;
        isShowKeyboard = false;
    }
}
