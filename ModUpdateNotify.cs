using Newtonsoft.Json;
using StardewModdingAPI;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace ImproveGame
{
    internal class ModUpdateNotify
    {
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

        public class ModDumpInfo
        {
            public Mod modDLL;
            public IManifest manifest;
            public ModDumpInfo(Mod mod)
            {
                this.modDLL = mod;
                this.manifest = mod.ModManifest;
            }
        }
        static NotifyUpdateLevel m_notifyUpdateLeve = NotifyUpdateLevel.NewVersion;
        public static async void CheckUpdateOnGithub(ModDumpInfo mod, string manifestURL)
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
                ModEntry.Instance.Monitor.Log("Can't check update on github " + e.Message, LogLevel.Error);
            }
        }
        public static void PrintCheckUpdate(IManifest manifest, ManifestSerialize manifestLatest, NotifyUpdateLevel notifyLevel, Mod mod)
        {
            SemanticVersion lastVersion;
            try
            {
                lastVersion = new SemanticVersion(manifestLatest.Version);
            }
            catch (Exception e)
            {
                ModEntry.Instance.Monitor.Log("Can't parse semantic version " + e.Message, LogLevel.Alert);
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
                ModEntry.Instance.Monitor.Log($"[{manifest.Name} {manifestLatest.UploadHost}] " + stringBuilder.ToString(), LogLevel.Alert);
            }
        }
    }
}
