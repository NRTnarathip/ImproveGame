using HarmonyLib;
using StardewValley;
using System.Collections;
using System.Reflection;

namespace ImproveGame.SpaceCore.CustomForge;

static class CustomForgeAPI
{
    static Type CustomForgeRecipe_Type;
    static Type IngredientMatcher_Type;
    static PropertyInfo Recipes_Property;
    static MethodInfo IngredientItem_Getter;
    static MethodInfo BaseItem_Getter;
    static MethodInfo HasEnoughFor_Method;
    static MethodInfo CinderShardCost_Getter;
    public static void Init()
    {
        CustomForgeRecipe_Type = SpaceCoreAPI.TypeByName("SpaceCore.CustomForgeRecipe");
        IngredientMatcher_Type = SpaceCoreAPI.TypeByName("SpaceCore.CustomForgeRecipe.IngredientMatcher");

        Recipes_Property = AccessTools.Property(CustomForgeRecipe_Type, "Recipes");
        BaseItem_Getter = AccessTools.PropertyGetter(CustomForgeRecipe_Type, "BaseItem");
        IngredientItem_Getter = AccessTools.PropertyGetter(CustomForgeRecipe_Type, "IngredientItem");

        HasEnoughFor_Method = AccessTools.Method(IngredientMatcher_Type, "HasEnoughFor");
        CinderShardCost_Getter = AccessTools.PropertyGetter(CustomForgeRecipe_Type, "CinderShardCost");
    }
    public static int Get_CinderShardCost(object recipe)
        => (int)CinderShardCost_Getter.Invoke(recipe, []);

    public static object GetRecipes()
    {
        return Recipes_Property.GetValue(null);
    }

    public static bool Recipe_BaseItem_HasEnoughFor(object recipe, Item item)
    {
        var baseItem = BaseItem_Getter.Invoke(recipe, []);
        return (bool)HasEnoughFor_Method.Invoke(baseItem, [item]);
    }
    public static bool Recipe_IngredientItem_HasEnoughFor(object recipe, Item item)
    {
        var ingredientItem = IngredientItem_Getter.Invoke(recipe, []);
        return (bool)HasEnoughFor_Method.Invoke(ingredientItem, [item]);
    }

    public static bool IsLeftCraftIngredient(Item item)
    {
        var recipes = GetRecipes() as IList;

        foreach (var recipe in recipes)
        {
            if (Recipe_BaseItem_HasEnoughFor(recipe, item))
                return true;
        }

        return false;
    }

    public static bool IsRightCraftIngredient(Item item)
    {
        var recipes = GetRecipes() as IList;

        foreach (var recipe in recipes)
        {
            if (Recipe_IngredientItem_HasEnoughFor(recipe, item))
                return true;
        }

        return false;
    }
}
