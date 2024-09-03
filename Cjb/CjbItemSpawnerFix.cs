using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System.Reflection;

namespace ImproveGame.Cjb;
static class SearchBoxFix
{
    static Type ItemMenu_Type;
    static FieldInfo Textbox_Field;
    static Type ThisType = typeof(SearchBoxFix);
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
static class CjbItemSpawnerAPI
{
    public const string ModID = "CJBok.ItemSpawner";
    public static void Init()
    {

    }
}
internal static class CjbItemSpawnerFix
{
    public static void TryInit()
    {
        if (ModEntry.IsModLoaded(CjbItemSpawnerAPI.ModID) == false)
            return;

        Console.WriteLine("try init Cjb Item Spawner Fix");
        CjbItemSpawnerAPI.Init();
        SearchBoxFix.Init();
    }
}
