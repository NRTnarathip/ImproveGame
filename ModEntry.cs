using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;

namespace ImproveGame;

public sealed class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; }
    public Harmony harmony { get; private set; }

    public static bool EnableMultiThreadLog = false;
    public static void Log(string msg)
    {
        if (EnableMultiThreadLog)
            lock (Instance)
                Instance.Monitor.Log(msg, LogLevel.Info);
        else
            Instance.Monitor.Log(msg, LogLevel.Info);

    }
    public static void Log(object msg) => Log(msg.ToString());
    public static void Log(string msg, LogLevel level)
    {
        if (EnableMultiThreadLog)
            lock (Instance)
                Instance.Monitor.Log(msg, level);
        else
            Instance.Monitor.Log(msg, level);
    }

    public static void Alert(string msg) => Log(msg, LogLevel.Alert);
    public static void Error(string msg) => Log(msg, LogLevel.Error);
    public static LogLevel LangaugeModLogLevel = LogLevel.Debug;
    public override void Entry(IModHelper helper)
    {
        Instance = this;
        harmony = new Harmony("NRTnarathip.ImproveGame");
        harmony.PatchAll();
        helper.Events.Content.AssetReady += handleOnModLangageLoaded;
        DayTimeMoneyBoxThaiFormat.Init(harmony);
        helper.Events.Specialized.LoadStageChanged += LoadedStateChanged;
    }

    private void LoadedStateChanged(object? sender, LoadStageChangedEventArgs e)
    {
        if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Ready)
        {
            DisableQuickSave.TryInitialize(harmony);
        }
    }


    //Mod language
    void handleOnModLangageLoaded(object? sender, AssetReadyEventArgs e)
    {
        //patch force use mod language if is found
        if (e.Name.ToString().Equals("Data/AdditionalLanguages"))
        {
            Log("This mod ready to check mod language!", LangaugeModLogLevel);
            //check if mod is not valid & restore lang with preference
            //fource mod lang

            var savePreference = new StartupPreferences();
            savePreference.loadPreferences(false, true);

            PrintLanguageInfo();

            if (VerifyCanApplyModLanguage())
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
    }
    bool VerifyCanApplyModLanguage()
    {
        List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
        if (modLanguages.Count == 0)
        {
            Log("Not found any mod language");
            return false;
        }
        if (modLanguages.Count != 1)
        {
            Monitor.Log("Not support multi mod language!!. Please delete theme", LogLevel.Error);
            return false;
        }

        return true;
    }
    public void PrintLanguageInfo()
    {
        if (LocalizedContentManager.CurrentModLanguage != null)
        {
            this.Monitor.Log($"LanguageID: {LocalizedContentManager.CurrentModLanguage.ID}" +
                $", mod config: FontPixelZoom: {LocalizedContentManager.CurrentModLanguage.FontPixelZoom}" +
                $", mod config: useLatinFont: {LocalizedContentManager.CurrentModLanguage.UseLatinFont}", LangaugeModLogLevel);
        }
        else
        {
            this.Monitor.Log($"Current languageCode: {LocalizedContentManager.CurrentLanguageCode}", LangaugeModLogLevel);
        }
    }
    public void SetLanguageToEnglish(string arg1 = null, string[] arg2 = null)
    {
        LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
        this.Monitor.Log($"Done set language: English", LogLevel.Info);
    }
    public void SetLanguageToMod(string cmd = default, string[] args = default)
    {
        this.Monitor.Log($"Try set language to mod.", LogLevel.Info);
        List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
        if (modLanguages.Count == 0)
        {
            Log("Not found any mod language");
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
                SetLanguageToEnglish();
            return;
        }
        if (modLanguages.Count != 1)
        {
            Log("Not support multi mod language!!. Please delete theme", LangaugeModLogLevel);
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
                SetLanguageToEnglish();
            return;
        }

        var targetModLanguage = modLanguages[0];
        if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod)
        {
            this.Monitor.Log($"Applying mod languageID: {targetModLanguage.ID}", LangaugeModLogLevel);
            LocalizedContentManager.SetModLanguage(targetModLanguage);
        }

        //check again
        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
        {
            Log($"Done set mod languageID: {targetModLanguage.ID}");
        }
        else
        {
            Log($"Error try to mod languageID: {targetModLanguage} ");
        }
    }
}
