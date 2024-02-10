using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using System.Collections.Generic;

namespace StardewValleyThaiMobile
{
    public class ModEntry : Mod
    {
        GameMobilePatcher gameMobilePatcher;
        public static ModEntry Instance { get; private set; }
        public Harmony harmony { get; private set; }
        public void Log(string msg) => this.Monitor.Log(msg, LogLevel.Info);

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            harmony = new Harmony("com.nrt.stardewvalley");
            Harmony.DEBUG = true;

            gameMobilePatcher = new();

            helper.ConsoleCommands.Add("lm", "Set langauge to Mod.", this.SetLanguageToMod);
            helper.ConsoleCommands.Add("en", "Set langauge to English.", this.SetLanguageToEnglish);
            helper.Events.Content.AssetReady += handleOnReady;
        }

        void handleOnReady(object sender, AssetReadyEventArgs e)
        {
            //patch force use mod language if is found
            if (e.Name.ToString().Equals("Data/AdditionalLanguages"))
            {
                Log("Mod Langauage it's are ready!");
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
                    $", mod config: useLatinFont: {LocalizedContentManager.CurrentModLanguage.UseLatinFont}", LogLevel.Info);
            }
            else
            {
                Log($"Current languageCode: {LocalizedContentManager.CurrentLanguageCode}");
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
            PrintLanguageInfo();
            List<ModLanguage> modLanguages = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
            if (modLanguages.Count == 0)
            {
                Log("Not found any mod language");
                return;
            }
            if (modLanguages.Count != 1)
            {
                Monitor.Log("Not support multi mod language!!. Please delete theme", LogLevel.Error);
                return;
            }

            var targetModLanguage = modLanguages[0];
            this.Monitor.Log($"Found mod languageID: {targetModLanguage}", LogLevel.Info);
            if (targetModLanguage == LocalizedContentManager.CurrentModLanguage)
            {
                this.Monitor.Log($"current is already mod languageID: {targetModLanguage.ID}", LogLevel.Info);
                return;
            }

            this.Monitor.Log($"Applying mod languageID: {targetModLanguage.ID}", LogLevel.Info);
            LocalizedContentManager.SetModLanguage(targetModLanguage);
            if (LocalizedContentManager.CurrentModLanguage == targetModLanguage)
            {
                this.Monitor.Log($"Done set mod languageID: {targetModLanguage.ID}", LogLevel.Alert);
            }
        }
    }
}
