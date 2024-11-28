using HarmonyLib;
using StardewModdingAPI;
using StardewValley.GameData;
using StardewValley;

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

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
    }

    private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
    {
        if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod)
            return;

        if (DayTimeMoneyBoxThaiFormat.IsApplyPatch)
            return;

        //check if mod thai then patch time format
        List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
        var targetModLanguage = modLanguages.FirstOrDefault();
        if (targetModLanguage != null & targetModLanguage.Id == "ELL.StardewValleyTHAI")
            DayTimeMoneyBoxThaiFormat.Apply(ModEntry.Instance.harmony);

    }
}
