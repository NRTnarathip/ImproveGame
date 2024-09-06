using HarmonyLib;
using StardewValley;

namespace ImproveGame.Cjb;

static class CjbItemSpawnerUIFix
{
    static Type ThisType = typeof(CjbItemSpawnerUIFix);
    public static void Init()
    {
        var harmony = ModEntry.Instance.harmony;
        var ItemMenu_Type = AccessTools.TypeByName("CJBItemSpawner.Framework.ItemMenu");
        var ItemMenuCtor = ItemMenu_Type.GetConstructors()[0];
        harmony.Patch(
            original: ItemMenuCtor,
            prefix: new(ThisType, nameof(PrefixCtor)),
            postfix: new(ThisType, nameof(PostfixCtor))
        );
    }

    static xTile.Dimensions.Rectangle oldViewport;
    static void PrefixCtor()
    {
        oldViewport = Game1.viewport;

        //info debug for device resolution: W.2400, H.1080 POCO F3
        Game1.viewport.Width = 1300;
        Game1.viewport.Height = 680;
    }
    static void PostfixCtor()
    {
        Game1.viewport = oldViewport;
    }
}
