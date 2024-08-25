using HarmonyLib;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

#pragma warning disable CS8601 // Possible null reference assignment.
namespace ImproveGame
{
    [HarmonyPatch]
    class SpaceCoreCrashFix
    {
        static string MethodFullName(MethodInfo method)
        {
            return $"{method.DeclaringType}::{method}";
        }
        static string MethodFullName(MethodBase method)
        {
            return $"{method.DeclaringType}::{method}";
        }

        static void PrintStack()
        {
            foreach (var frame in new StackTrace().GetFrames())
            {
                Console.WriteLine("qwe frame: " + MethodFullName(frame.GetMethod()));
            }
        }
        public static void Init()
        {
            {
                var harmonySpaceCore = new Harmony("spacechase0.SpaceCore");
                var spaceCoreAsm = Assembly.Load("SpaceCore");
                {
                    var method = spaceCoreAsm.GetType("SpaceCore.Patches.SaveGamePatcher")
                        .GetMethod("SerializeProxy", BindingFlags.Static | BindingFlags.NonPublic);
                    ModEntry.Instance.harmony.Patch(method,
                        prefix: new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_SerializeProxy))));
                }
            }

            var harmony = ModEntry.Instance.harmony;
            {
                var enumTypes = Assembly.GetAssembly(typeof(SaveGame))
                        .GetTypes().Where(t => t.IsEnum && t.FullName.StartsWith("StardewValley.")).ToArray();
                foreach (var enumType in enumTypes)
                {
                    var netEnumType = typeof(NetEnum<>);
                    netEnumType = netEnumType.MakeGenericType(enumType);
                    var Add_Method = netEnumType.GetMethod("Add",
                        BindingFlags.Instance | BindingFlags.Public,
                        null, [typeof(object)], null);
                    harmony.Patch(Add_Method,
                        prefix: new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_NetEnum_Add))));
                }
            }

            {
                var type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
                var method = type.GetMethod("GetEnumXmlValue",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null, [typeof(XmlTypeMapping), typeof(object)], null);
                harmony.Patch(method,
                    new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_GetEnumXmlValue))));
            }
        }

        private static bool FindAndRemoveModNodes(XmlNode node, List<KeyValuePair<string, string>> modNodes, string currPath = "")
        {
            if (node.HasChildNodes)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; --i)
                {
                    var child = node.ChildNodes[i];
                    if (FindAndRemoveModNodes(child, modNodes, $"{currPath}/{i}"))
                    {
                        modNodes.Add(new KeyValuePair<string, string>($"{currPath}/{i}", child.OuterXml));
                        node.RemoveChild(child);
                    }
                }
            }

            var attr = node.Attributes?["xsi:type"];
            if (attr == null)
                return false;

            if (attr.Value.StartsWith("Mods_"))
                return true;
            return false;
        }

        static readonly string Filename = "spacecore-serialization.json";
        static readonly string FarmerFilename = "spacecore-serialization-farmer.json";
        public static bool Prefix_SerializeProxy(XmlSerializer serializer, XmlWriter origWriter, object obj)
        {
            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, new XmlWriterSettings { CloseOutput = false });

            serializer.Serialize(writer, obj);
            XmlDocument doc = new XmlDocument();
            ms.Position = 0;
            doc.Load(ms);

            var modNodes = new List<KeyValuePair<string, string>>();
            FindAndRemoveModNodes(doc, modNodes, "/1"); // <?xml ... ?> is /0

            doc.WriteContentTo(origWriter);
            // To fix serialize bug in mobile platform
            origWriter.Flush();
            string filename = serializer.GetType() == typeof(FarmerSerializer) ? FarmerFilename : Filename;

            //fix bug mobile
            //we should create folder Save Game before File.WriteAllText
            //becuase original code game it's Create Folder SaveGame after called saveSerializer.Serialize();
            string saveGameName = Game1.GetSaveGameName();
            string filenameNoTmpString = saveGameName + "_" + Game1.uniqueIDForThisGame;
            string saveGameFolderFullPath = Path.Combine(Game1.savesPath, filenameNoTmpString);
            Directory.CreateDirectory(saveGameFolderFullPath);
            File.WriteAllText(Path.Combine(saveGameFolderFullPath, filename),
                JsonConvert.SerializeObject(modNodes));
            return false;
        }


        static Type XmlTypeMapping_Type = typeof(XmlTypeMapping);
        static FieldInfo XmlTypeMapping_type_Field = XmlTypeMapping_Type.GetField("type",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static Type TypeData_Type = AccessTools.TypeByName("System.Xml.Serialization.TypeData");
        static FieldInfo TypeData_type_Field = TypeData_Type.GetField("type",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Prefix_GetEnumXmlValue(XmlTypeMapping typeMap, ref object ob)
        {
            if (ob is string enumValueString)
            {
                var typeData = XmlTypeMapping_type_Field.GetValue(typeMap);
                var enumType = TypeData_type_Field.GetValue(typeData) as Type;
                ob = Enum.Parse(enumType, enumValueString);
                ModEntry.Log("Fix GetEnumXmlValue(typeMap, obj); type:" + enumType + ", value: " + ob);
                //PrintStack();
            }
        }

        public static bool Prefix_NetEnum_Add(object __instance, ref object value)
        {
            //Console.WriteLine("qwe; hook NetEnum.Add()");
            //Console.WriteLine("qwe; value: " + value
            //    + $"value type: {value.GetType()}");
            //foreach (var f in new StackTrace().GetFrames())
            //    Console.WriteLine("qwe; frame: " + f.GetMethod().Name);
            ModEntry.Log("Fix NetEnum.Add(value) cast value to string; value: " + value);
            value = value.ToString();
            return false;
        }
    }
}
