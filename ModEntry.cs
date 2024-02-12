using HarmonyLib;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImproveGame
{
    public class ManifestSerialize
    {
        public enum UploadHostEnum
        {
            NoneInit, Github, Nexus,
        }
        public string Name;
        public string Author;
        public string Version;
        public UploadHostEnum UploadHost = UploadHostEnum.NoneInit;
    }

    public class NexusModManifest
    {
        public string author;
        public string version;
        public string name;
        public ManifestSerialize manifest => new()
        {
            Name = name,
            Author = author,
            Version = version,
        };
    }
    [Flags]
    public enum NotifyUpdateLevel
    {
        None = 1 << 1,
        NewVersion = 1 << 2,
        CurrentVersion = 1 << 3,
        IsLatest = 1 << 4, //show if is no need update
        Hello = 1 << 5,
        All = int.MaxValue,
    }
    public sealed class ModEntry : Mod
    {
        public const string ManifestURL = "https://raw.githubusercontent.com/NRTnarathip/ImproveGame/master/manifest.json";
        GameMobilePatcher gameMobilePatcher;
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


        public static LogLevel LangaugeModLogLevel = LogLevel.Trace;

        public NotifyUpdateLevel m_notifyUpdateLeve = NotifyUpdateLevel.NewVersion;
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            harmony = new Harmony(ModManifest.UniqueID);

            gameMobilePatcher = new();

            helper.ConsoleCommands.Add("lm", "Set langauge to Mod.", this.SetLanguageToMod);
            helper.ConsoleCommands.Add("en", "Set langauge to English.", this.SetLanguageToEnglish);
            helper.Events.Content.AssetReady += handleOnModLangageLoaded;

            CheckUpdateOnGithub(new(this, ModManifest, null), ManifestURL);
            helper.Events.GameLoop.GameLaunched += handleCheckModsUpdate;
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


        //Mod Update Notifyer
        public class ModDumpInfo
        {
            public Mod modDLL;
            public IManifest manifest;
            public IContentPack contentPack;
            public ModDumpInfo(Mod mod, IManifest manifest, IContentPack contentPack)
            {
                this.modDLL = mod;
                this.manifest = manifest;
                this.contentPack = contentPack;
            }
        }
        public List<ModDumpInfo> GetAllMods()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetName().Name == "StardewModdingAPI");
            var coreType = asm.GetType("StardewModdingAPI.Framework.SCore");

            var instanceField = coreType.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField);
            var core = instanceField.GetValue(null);

            var modRegistryField = coreType.GetField("ModRegistry", BindingFlags.Instance | BindingFlags.NonPublic);
            var modRegistryObj = modRegistryField.GetValue(core);

            var ModRegistryType = asm.GetType("StardewModdingAPI.Framework.ModRegistry");
            var modRegistry_Mods = ModRegistryType.GetField("Mods", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(modRegistryObj);
            var ModMetadataType = asm.GetType("StardewModdingAPI.Framework.ModLoading.ModMetadata");

            PropertyInfo ModMetadata_ModProperty = null;
            PropertyInfo ModMetadata_ContentPackProperty = null;
            PropertyInfo ModMetadata_ManifestProperty = null;
            foreach (var prop in ModMetadataType.GetProperties(BindingFlags.GetField | BindingFlags.GetProperty
                | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.SetField))
            {
                if (prop.Name == "Mod")
                    ModMetadata_ModProperty = prop;
                else if (prop.Name == "ContentPack")
                    ModMetadata_ContentPackProperty = prop;
                else if (prop.Name == "Manifest")
                    ModMetadata_ManifestProperty = prop;
            }

            List<ModDumpInfo> mods = new();

            foreach (var modMetadata in (IEnumerable)modRegistry_Mods)
            {
                var manifest = ModMetadata_ManifestProperty.GetValue(modMetadata);
                if (manifest == null)
                    continue;

                var modDLL = ModMetadata_ModProperty.GetValue(modMetadata);
                var contentPack = ModMetadata_ContentPackProperty.GetValue(modMetadata);
                mods.Add(new ModDumpInfo(modDLL as Mod, manifest as IManifest, contentPack as IContentPack));
            }
            return mods;
        }
        object checkUpdateLock = new object();
        private void handleCheckModsUpdate(object sender, GameLaunchedEventArgs e)
        {
            try
            {

                Task.Run(() =>
                {
                    var mods = GetAllMods();
                    Log($"Checking mod update total: {mods.Count}");
                    EnableMultiThreadLog = true;
                    CountdownEvent countdownEvent = new CountdownEvent(mods.Count);

                    Parallel.ForEach(mods, mod =>
                    {
                        try
                        {
                            foreach (var key in mod.manifest.UpdateKeys)
                            {
                                if (key.Contains("Nexus:"))
                                {
                                    CheckUpdateOnNexus(mod);
                                }
                            }
                        }
                        finally
                        {
                            countdownEvent.Signal();
                        }
                    });
                    countdownEvent.Wait();
                    Log($"Successfully check all mod update: {mods.Count}");
                });
            }
            catch (Exception ex)
            {
                Log("Error On Task.Run: " + ex.Message);
            }
        }
        public async void CheckUpdateOnGithub(ModDumpInfo mod, string manifestURL)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(manifestURL);
                var respone = request.GetResponse();
                var content = respone.GetResponseStream();
                using (var reader = new StreamReader(content))
                {
                    string manifestContent = reader.ReadToEnd();
                    var manifestLatest = JsonConvert.DeserializeObject<ManifestSerialize>(manifestContent);
                    manifestLatest.UploadHost = ManifestSerialize.UploadHostEnum.Github;
                    PrintCheckUpdate(mod.manifest, manifestLatest, m_notifyUpdateLeve, mod.modDLL);
                }
            }
            catch (Exception e)
            {
                Error("Can't check update on Github");
                //Error(e.Message);
            }
        }
        public async void CheckUpdateOnNexus(ModDumpInfo mod)
        {
            try
            {
                if (mod.manifest.UpdateKeys.Length == 0)
                    return;

                int modID = 0;
                foreach (var key in mod.manifest.UpdateKeys)
                {
                    if (key.Contains("Nexus:"))
                    {
                        try
                        {
                            modID = int.Parse(key[6..]);
                            break;
                        }
                        catch (Exception e)
                        {
                            //Error(e.Message);
                            return;
                        }
                    }
                }

                if (modID == 0)
                {
                    //Log("mod not found nexusID");
                    return;
                }

                string apiUrl = $"https://api.nexusmods.com/v1/games/stardewvalley/mods/{modID}.json";
                var request = (HttpWebRequest)WebRequest.Create(apiUrl);
                const string apiKey = "mI/MmwKntQ1Bou+s7jJbUVWaa5jrWqD9mcWwh9yGnwboPoJB+1c=--b4qUXku//sG9Pgnz--PEjpOpXVo2z0BWeQwC9HsQ==";
                request.Headers.Add("apikey", apiKey);
                request.Headers.Add("appVersion", ModManifest.Version.ToString());
                request.Headers.Add("appName", ModManifest.UniqueID);
                //Log("create request nexus api mod: " + mod.manifest.Name);
                var st = Stopwatch.StartNew();
                using (var response = request.GetResponse())
                {
                    st.Stop();
                    //Log($"respone time: {st.Elapsed.TotalMilliseconds}ms");
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        // Read the content
                        StreamReader reader = new StreamReader(dataStream);
                        string content = reader.ReadToEnd();
                        //Log($"content size: {content.Length} byte");
                        var manifestLatest = JsonConvert.DeserializeObject<NexusModManifest>(content).manifest;
                        manifestLatest.UploadHost = ManifestSerialize.UploadHostEnum.Nexus;
                        PrintCheckUpdate(mod.manifest, manifestLatest, m_notifyUpdateLeve, mod.modDLL);
                    }
                }
            }
            catch (Exception e)
            {
                Log("Can't check update on Nexus");
                Error(e.Message);
            }
        }
        public void PrintCheckUpdate(IManifest manifest, ManifestSerialize manifestLatest, NotifyUpdateLevel notifyLevel, Mod mod)
        {
            SemanticVersion lastVersion;
            try
            {
                lastVersion = new SemanticVersion(manifestLatest.Version);
            }
            catch (Exception e)
            {
                Alert("Can't parse semantic version " + e.Message);
                return;
            }
            if (lastVersion == null)
                return;

            StringBuilder stringBuilder = new StringBuilder();
            bool isNewer = lastVersion.IsNewerThan(manifest.Version);

            if (notifyLevel.HasFlag(NotifyUpdateLevel.CurrentVersion))
            {
                stringBuilder.Append($"Current V. {manifest.Version}");
            }

            if (isNewer && notifyLevel.HasFlag(NotifyUpdateLevel.NewVersion))
            {
                stringBuilder.Append($"!!Found New Version {manifestLatest.Version} [==] current: {manifest.Version}");
            }

            if (stringBuilder.Length == 0) return;

            if (mod != null)
            {
                mod.Monitor.Log($"[{manifestLatest.UploadHost}] " + stringBuilder.ToString(), LogLevel.Alert);
            }
            else
            {
                Alert($"[{manifest.Name} {manifestLatest.UploadHost}] " + stringBuilder.ToString());
            }
        }
    }
}
