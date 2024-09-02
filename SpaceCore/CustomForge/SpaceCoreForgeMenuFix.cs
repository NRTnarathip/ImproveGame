using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace ImproveGame.SpaceCore.CustomForge;
using StardewModdingAPI.Events;
using System;


internal static class SpaceCoreForgeMenuFix
{
    static Type NewForgeMenu_Type;
    static MethodInfo IsLeftCraftIngredient_Method;
    static MethodInfo IsRightCraftIngredient_Method;
    static Type ThisType = typeof(SpaceCoreForgeMenuFix);
    public static void Init()
    {
        var patcher = SpaceCoreAPI.harmony;

        //Disable Custom Forge On SpaceCore
        const bool IsDisableCustomForge = true;
        if (IsDisableCustomForge)
        {
            var patcherMethods = patcher.GetPatchedMethods().ToArray();
            //unpatch all method in FogeMenuPatcher.cs
            for (int i = 0; i < patcherMethods.Length; i++)
            {
                var method = patcherMethods[i];
                if (method.Name == nameof(ForgeMenu.IsValidCraft))
                {
                    SpaceCoreAPI.Unpatch(patcherMethods[i..(i + 6)]);
                    break;
                }
            }
            //disable custom menu
            var SpaceCore_Type = SpaceCoreAPI.TypeByName("SpaceCore.SpaceCore");
            var OnMenuChange_Method = AccessTools.Method(SpaceCore_Type, "OnMenuChanged");
            patcher.Patch(
                original: OnMenuChange_Method,
                prefix: new(AccessTools.Method(ThisType, nameof(Prefix_OnMenuChanged_Disable)))
            );
        }
        else
        {
            ApplyCustomForgeMenu();
        }
    }
    static void ApplyCustomForgeMenu()
    {
        CustomForgeAPI.Init();
        var patcher = SpaceCoreAPI.harmony;
        NewForgeMenu_Type = SpaceCoreAPI.TypeByName("SpaceCore.Interface.NewForgeMenu");
        IsLeftCraftIngredient_Method = AccessTools.Method(NewForgeMenu_Type, "IsLeftCraftIngredient");
        IsRightCraftIngredient_Method = AccessTools.Method(NewForgeMenu_Type, "IsRightCraftIngredient");

        var SpaceCore_Type = SpaceCoreAPI.TypeByName("SpaceCore.SpaceCore");
        var OnMenuChange_Method = AccessTools.Method(SpaceCore_Type, "OnMenuChanged");
        patcher.Patch(
            original: OnMenuChange_Method,
            prefix: new(AccessTools.Method(ThisType, nameof(Prefix_OnMenuChanged)))
        );
    }
    static bool Prefix_OnMenuChanged_Disable(object sender, MenuChangedEventArgs e) => false;
    static bool Prefix_OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is ForgeMenu)
            Game1.activeClickableMenu = new NewForgeMenu();

        return false;
    }
}
