using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;

namespace ImproveGame;

class ModLanguageChanger
{
    Mod mod;
    public static LogLevel LangaugeModLogLevel = LogLevel.Debug;
    public ModLanguageChanger(Mod mod)
    {
        this.mod = mod;
        mod.Helper.Events.Content.AssetReady += handleOnModLangageLoaded;
    }
    //Mod language
    void handleOnModLangageLoaded(object? sender, AssetReadyEventArgs e)
    {
        //patch force use mod language if is found
        if (e.Name.ToString().Equals("Data/AdditionalLanguages"))
        {
            Logger.Log("detect load AdditionalLanguages!");
            TrySetLanguageMode();
        }
    }
    void TrySetLanguageMode()
    {
        //check if mod is not valid & restore lang with preference
        //fource mod lang

        Logger.Log("try set language with mode");
        var savePreference = new StartupPreferences();
        savePreference.loadPreferences(false, true);

        PrintLanguageInfo();

        if (CheckCanApplyModLanguage())
        {
            SetLanguageToMod();
            savePreference.savePreferences(false, true);
            PrintLanguageInfo();
        }

        //fix restore language if Mod language is error
        else
        {
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
            {
                SetLanguageToEnglish();
                savePreference.savePreferences(false, true);
                PrintLanguageInfo();
            }
        }
    }
    bool CheckCanApplyModLanguage()
    {
        List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
        if (modLanguages.Count == 0)
        {
            Logger.Log("Not found any mod language");
            return false;
        }
        if (modLanguages.Count != 1)
        {
            Logger.Log("Not support multi mod language!!. Please delete theme", LogLevel.Error);
            return false;
        }

        return true;
    }
    public void PrintLanguageInfo()
    {
        if (LocalizedContentManager.CurrentModLanguage != null)
        {
            Logger.Log($"LanguageID: {LocalizedContentManager.CurrentModLanguage.ID}" +
                $", mod config: FontPixelZoom: {LocalizedContentManager.CurrentModLanguage.FontPixelZoom}" +
                $", mod config: useLatinFont: {LocalizedContentManager.CurrentModLanguage.UseLatinFont}", LangaugeModLogLevel);
        }
        else
        {
            Logger.Log($"Current languageCode: {LocalizedContentManager.CurrentLanguageCode}", LangaugeModLogLevel);
        }
    }
    public void SetLanguageToEnglish(string arg1 = null, string[] arg2 = null)
    {
        LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
        Logger.Log($"Done set language: English", LogLevel.Info);
    }
    public void SetLanguageToMod(string cmd = default, string[] args = default)
    {
        Logger.Log($"Try set language to mod.", LogLevel.Info);
        List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
        if (modLanguages.Count == 0)
        {
            Logger.Log("Not found any mod language");
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
                SetLanguageToEnglish();
            return;
        }
        if (modLanguages.Count != 1)
        {
            Logger.Log("Not support multi mod language!!. Please delete theme", LangaugeModLogLevel);
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
                SetLanguageToEnglish();
            return;
        }

        var targetModLanguage = modLanguages[0];
        if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod)
        {
            Logger.Log($"Applying mod languageID: {targetModLanguage.ID}", LangaugeModLogLevel);
            LocalizedContentManager.SetModLanguage(targetModLanguage);
        }

        //check again
        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
        {
            Logger.Log($"Done set mod languageID: {targetModLanguage.ID}");
        }
        else
        {
            Logger.Log($"Error try to mod languageID: {targetModLanguage} ");
        }
    }
}
