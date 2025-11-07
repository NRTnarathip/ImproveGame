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
        Helper.Events.Content.AssetReady += Content_AssetReady;
    }

    void Content_AssetReady(object? sender, StardewModdingAPI.Events.AssetReadyEventArgs e)
    {
        // auto detect mod language 
        if (e.Name.Name.Equals("Data/AdditionalLanguages"))
        {
            modLanguageCore.ApplyModLanguage();
        }
    }

    private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
    {
        // remove this event
        Helper.Events.GameLoop.SaveLoaded -= GameLoop_SaveLoaded;

        if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod)
            return;

        // check if mod Thai then patch time format
        List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
        var targetModLanguage = modLanguages.FirstOrDefault();
        if (targetModLanguage?.Id == "ELL.StardewValleyTHAI")
            DayTimeMoneyBoxThaiFormat.ApplyPatch(Instance.harmony);
    }
}
