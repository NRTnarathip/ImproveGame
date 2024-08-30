using HarmonyLib;

namespace ImproveGame;

[HarmonyPatch]
internal class FindBug : BasePatcher
{
    public FindBug()
    {
    }

    public static void Init()
    {
        new FindBug();
    }
}
