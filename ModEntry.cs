using HarmonyLib;
using ImproveGame.Cjb;
using ImproveGame.SpaceCore.CustomForge;
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
        DayTimeMoneyBoxThaiFormat.Init(harmony);
        PerformanceTester.Init();
        FindBug.Init();

        //patch fix other mods
        if (SpaceCoreAPI.IsLoaded())
        {
            helper.Events.Specialized.LoadStageChanged += (sender, e) =>
            {
                if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Ready)
                    DisableQuickSave.TryInit(harmony);
            };

            SpaceCoreAPI.Init();
            SpaceCoreCrashFix.Init();
            SpaceCoreWalletUIFix.Init();
            SpaceCoreSerializerCustomFix.Init();
            XmlPatcherFix.Init(); //fix for new XmlSerializer();
            SpaceCoreForgeMenuFix.Init();
        }

        CjbItemSpawnerFix.TryInit();
    }
}
