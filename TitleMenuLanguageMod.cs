using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ImproveGame;

//add button to TitleMenu
[HarmonyPatch]
static class TitleMenuLanguageMod
{
    static ClickableTextureComponent languageButton;
    readonly static int pixelZoom = TitleMenu.pixelZoom;
    static TitleMenuLanguageMod()
    {
        ModEntry.Instance.Helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
    }

    static void Display_RenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        var titleMenu = Game1.activeClickableMenu as TitleMenu;
        if (titleMenu is null)
            return;

        var subMenu = TitleMenu.subMenu;
        bool flag = subMenu == null || subMenu is AboutMenu || subMenu is LanguageSelectionMenu;
        if (subMenu == null && !titleMenu.isTransitioningButtons
            && titleMenu.titleInPosition
            && !titleMenu.transitioningCharacterCreationMenu
            && titleMenu.HasActiveUser && flag)
        {
            //draw
            var drawRectPos = new Rectangle(
                    titleMenu.width + -22 * pixelZoom - 8 * pixelZoom * 2,
                    titleMenu.height - 25 * pixelZoom * 2 - 16 * pixelZoom,
                    27 * pixelZoom, 25 * pixelZoom);
            drawRectPos.Y -= 120;
            languageButton.bounds = drawRectPos;
            languageButton.draw(e.SpriteBatch);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TitleMenu), ".ctor", MethodType.Constructor)]
    static void PostfixCtor(TitleMenu __instance)
    {
        var titleMenu = __instance;
        var titleButtonsTexture = __instance.titleButtonsTexture;
        Console.WriteLine("title btn: " + titleButtonsTexture.ActualWidth);

        languageButton = new ClickableTextureComponent(
            "Mod Language", new(0, 0, 100, 100), null, "",
            titleButtonsTexture, new Rectangle(52, 458, 27, 25), pixelZoom);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.releaseLeftClick))]
    static void releaseLeftClick(int x, int y)
    {
        if (languageButton.containsPoint(x, y))
        {
            ModLanguageChanger.Instance.TrySetLanguageMode();
        }
    }
}
