using Microsoft.Xml.Serialization.GeneratedAssembly;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;

namespace ImproveGame;

public sealed class ModLanguageChanger
{
    public static ModLanguageChanger Instance { get; private set; }
    Mod mod;
    public static LogLevel LangaugeModLogLevel = LogLevel.Debug;
    public ModLanguageChanger(Mod mod)
    {
        this.mod = mod;
        Instance = this;
    }

    public void TrySetLanguageMode()
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
    void PrintLanguageInfo()
    {
        if (LocalizedContentManager.CurrentModLanguage != null)
        {
            Logger.Log($"LanguageID: {LocalizedContentManager.CurrentModLanguage.Id}" +
                $", mod config: FontPixelZoom: {LocalizedContentManager.CurrentModLanguage.FontPixelZoom}" +
                $", mod config: useLatinFont: {LocalizedContentManager.CurrentModLanguage.UseLatinFont}", LangaugeModLogLevel);
        }
        else
        {
            Logger.Log($"Current languageCode: {LocalizedContentManager.CurrentLanguageCode}", LangaugeModLogLevel);
        }
    }
    void SetLanguageToEnglish(string arg1 = null, string[] arg2 = null)
    {
        LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
        Logger.Log($"Done set language: English", LogLevel.Info);
    }
    void SetLanguageToMod(string cmd = default, string[] args = default)
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
            Logger.Log($"Applying mod languageID: {targetModLanguage.Id}", LangaugeModLogLevel);
            LocalizedContentManager.SetModLanguage(targetModLanguage);
        }

        //check again
        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
        {
            Logger.Log($"Done set mod languageID: {targetModLanguage.Id}");

        }
        else
        {
            Logger.Log($"Error try to mod languageID: {targetModLanguage} ");
        }
    }
}
