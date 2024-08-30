using HarmonyLib;
using Netcode;
using StardewValley;

namespace ImproveGame;


internal static class TestCustomProperty
{

    static NetBool hasCake_Me = new(true);
    public static NetBool Getter_hasCake(this Farmer farmer)
    {
        Console.WriteLine("Getter_hasCake(); value: " + hasCake_Me);
        return hasCake_Me;
    }
    public static void Setter_hasCake(this Farmer farmer, NetBool value)
    {
        Console.WriteLine("Setter_hasCake(); value: " + value.Value);
        farmer.Getter_hasCake().Value = value.Value;
    }
    public static void Apply()
    {
        //Regiser Class Type

        const string hasCake_MemberName = "hasCake";
        SerializerCustomPropertyAPI.RegisterCustomProperty(
            typeof(Farmer),
            hasCake_MemberName,
            hasCake_Me.GetType(),
            getter: AccessTools.Method(typeof(TestCustomProperty), nameof(Getter_hasCake)),
            setter: AccessTools.Method(typeof(TestCustomProperty), nameof(Setter_hasCake))
        );
    }
}
