using HarmonyLib;

namespace ImproveGame;

[HarmonyPatch]
internal static class PerformanceTester
{
    public static void Init()
    {
        var harmony = ModEntry.Instance.harmony;


    }
}
