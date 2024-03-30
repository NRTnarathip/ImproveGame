using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using System.Collections.Generic;

namespace ImproveGame
{

    public sealed class ModEntry : Mod
    {
        public const string ManifestURL = "https://raw.githubusercontent.com/NRTnarathip/ImproveGame/master/manifest.json";
        GameMobilePatcher gameMobilePatcher;
        public static ModEntry Instance { get; private set; }
        public Harmony Harmony { get; private set; }

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
            Harmony = new Harmony(ModManifest.UniqueID);

            gameMobilePatcher = new();

            helper.ConsoleCommands.Add("lm", "Set langauge to Mod.", this.SetLanguageToMod);
            helper.ConsoleCommands.Add("en", "Set langauge to English.", this.SetLanguageToEnglish);
            helper.Events.Content.AssetReady += handleOnModLangageLoaded;
            ModUpdateNotify.CheckUpdateOnGithub(new(this), ManifestURL);
        }

        //Mod language
        void handleOnModLangageLoaded(object sender, AssetReadyEventArgs e)
        {
            //patch force use mod language if is found
            if (e.Name.ToString().Equals("Data/AdditionalLanguages"))
            {
                Log("Mod Langauage it's are ready!", LangaugeModLogLevel);
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
        public void PrintLanguageInfo(string arg1 = null, string[] arg2 = null)
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
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
            {
                Log($"Done set mod languageID: {targetModLanguage.ID}");
                return;
            }

            this.Monitor.Log($"Applying mod languageID: {targetModLanguage.ID}", LangaugeModLogLevel);
            LocalizedContentManager.SetModLanguage(targetModLanguage);
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
            {
                Log($"Done set mod languageID: {targetModLanguage.ID}");
            }
        }
    }
}
