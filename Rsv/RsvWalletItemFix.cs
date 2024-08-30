using HarmonyLib;
using Netcode;
using StardewValley;
using System.Reflection;

namespace ImproveGame.Rsv;
internal static class RsvWalletItemFix
{
    const string ModID = "Rafseazz.RidgesideVillage";
    public static bool IsLoaded => ModEntry.Instance.Helper.ModRegistry.IsLoaded(ModID);
    static Type WalletItem_Type;
    public static void Init()
    {
        var harmony = ModEntry.Instance.harmony;

        //replace new
        SerializerCustomPropertyAPI.RegisterCustomProperty(
            declaringType: typeof(Farmer),
            name: hasRiveraSecret_FieldName,
            propertyType: typeof(NetBool),
            getter: AccessTools.Method(typeof(RsvWalletItemFix), nameof(Getter_hasRiveraSecret)),
            setter: AccessTools.Method(typeof(RsvWalletItemFix), nameof(Setter_hasRiveraSecret))
        );
        WalletItem_Type = AccessTools.TypeByName("RidgesideVillage.WalletItem");
        Rsv_get_hasRiveraSecret_Method = AccessTools.Method(WalletItem_Type, "get_hasRiveraSecret");
        Rsv_set_hasRiveraSecret_Method = AccessTools.Method(WalletItem_Type, "set_hasRiveraSecret");
        Console.WriteLine("wallet item type: " + WalletItem_Type);
    }
    const string hasRiveraSecret_FieldName = "hasRiveraSecret";
    public static MethodInfo Rsv_get_hasRiveraSecret_Method;
    public static MethodInfo Rsv_set_hasRiveraSecret_Method;
    public static void Setter_hasRiveraSecret(Farmer farmer, object newValue)
    {
        Console.WriteLine("Setter_hasRiveraSecret: " + newValue);
        try
        {
            Rsv_set_hasRiveraSecret_Method.Invoke(null, [farmer.team, newValue]);
        }
        catch (Exception ex)
        {
            Console.WriteLine("error Setter_hasRiveraSecret: " + ex);
        }
    }
    public static NetBool Getter_hasRiveraSecret(Farmer farmer)
    {
        try
        {

            var result = Rsv_get_hasRiveraSecret_Method.Invoke(null, [farmer.team]) as NetBool;
            Console.WriteLine("Getter_hasRiveraSecret: " + result.Value);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("error Getter_hasRiverSecret: " + ex);
        }
        return null;
    }
}
