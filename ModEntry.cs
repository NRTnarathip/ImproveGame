using HarmonyLib;
using StardewModdingAPI;

namespace ImproveGame;
public sealed partial class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; }
    public Harmony harmony { get; private set; }
    public static bool IsModLoaded(string id) => Instance.Helper.ModRegistry.IsLoaded(id);

    ModLanguageChanger modLanguageCore;
    public override void Entry(IModHelper helper)
    {
        //Initialize
        Instance = this;
        Logger.Init(this);

        //ready
        harmony = new Harmony(Helper.ModRegistry.ModID);
        harmony.PatchAll();
        modLanguageCore = new(this);
    }
}
