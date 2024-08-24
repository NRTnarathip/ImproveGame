using HarmonyLib;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using Netcode;
using StardewValley;
using System.Reflection;
using System.Xml.Serialization;

#pragma warning disable CS8601 // Possible null reference assignment.
namespace ImproveGame
{
    [HarmonyPatch]
    class SpaceCoreCrashFix
    {
        static string SpaceCoreID = "spacechase0.SpaceCore";
        static XmlSerializerContract Contract = new XmlSerializerContract();
        static Type Contract_Type = typeof(XmlSerializerContract);
        static MethodInfo Contract_GetSerializer_Method = Contract_Type.GetMethod("GetSerializer",
            BindingFlags.Instance | BindingFlags.Public);
        //public static bool Prefix_GetSerializer(Type type, ref object __result)
        //{
        //    Console.WriteLine("qwe; hook spacecore GetSerialzier");
        //    //Console.WriteLine("qwe;");
        //    //Console.WriteLine("qwe; hook SaveGame.GetSerialzier");
        //    //Console.WriteLine("qwe; Hook GetSerializ | type: " + type);

        //    //var baseType = type;
        //    //__result = Contract_GetSerializer_Method.Invoke(Contract, [baseType]);
        //    //__result = new XmlSerializer(baseType);
        //    return true;
        //}
        public static void Init()
        {

            {
                //var SpaceCoreAsm = Assembly.GetAssembly(typeof(SpaceCore.AnimatedSpriteDrawExtrasPatch1));
                //var harmonySpaceCore = new Harmony(SpaceCoreID);
                //var methods = harmonySpaceCore.GetPatchedMethods().ToArray();
                //for (int i = 0; i < methods.Length; i++)
                //{
                //    var method = methods[i];
                //    var name = method.Name;
                //    if (name == "GetSerializer")
                //    {
                //        Console.WriteLine($"qwe; index:[{i}/{methods.Length - 1}] try unpatch: " + method.Name);
                //        harmonySpaceCore.Unpatch(method, HarmonyPatchType.All);
                //    }
                //}
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
                //    var type = AccessTools.TypeByName("System.Xml.Serialization.EnumMap");
                //    var method = type.GetMethod("GetXmlName",
                //        BindingFlags.Instance | BindingFlags.Public,
                //        null, [typeof(string), typeof(object)], null);
                //    harmony.Patch(method,
                //        new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_GetXmlName))));
            }

            {
                var type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
                var method = type.GetMethod("GetEnumXmlValue",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null, [typeof(XmlTypeMapping), typeof(object)], null);
                harmony.Patch(method,
                    new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_GetEnumXmlValue))));
            }


            //{
            //    var type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationReaderInterpreter");
            //    var method = type.GetMethod("AddListValue", BindingFlags.Instance | BindingFlags.NonPublic);
            //    harmony.Patch(method, new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_AddListValue))));
            //}
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
                //Console.WriteLine("qwe; hook GetEnumXmlValue");
                //Console.WriteLine("qwe; TypeData_Type: " + TypeData_Type);
                //Console.WriteLine("qwe; TypeData_type_Field: " + TypeData_type_Field);
                //Console.WriteLine("qwe; XmlTypeMapping_type_Field: " + XmlTypeMapping_type_Field);
                var typeData = XmlTypeMapping_type_Field.GetValue(typeMap);
                var type = TypeData_type_Field.GetValue(typeData) as Type;
                //Console.WriteLine("qwe; GetEnumXmlValue type= " + type);
                ob = Enum.Parse(type, enumValueString);
            }
        }

        public static void Prefix_GetXmlName(string typeName, ref object enumValue)
        {
            if (enumValue is string enumValueString)
            {
                Console.WriteLine("qwe; hook GetXmlName: " + typeName + ", enumValue: " + enumValue);
                //var enumType = AccessTools.TypeByName(typeName);
                //Console.WriteLine("qwe; enum type: " + enumType);
                //enumValue = Enum.Parse(enumType, enumValueString);
                //Console.WriteLine("qwe; fix GetXmlName: " + typeName + ", " + enumValue);
            }
        }
        public static bool Prefix_NetEnum_Add(object __instance, ref object value)
        {
            //Console.WriteLine("qwe; hook NetEnum.Add()");
            //Console.WriteLine("qwe; value: " + value
            //    + $"value type: {value.GetType()}");
            //foreach (var f in new StackTrace().GetFrames())
            //    Console.WriteLine("qwe; frame: " + f.GetMethod().Name);
            value = value.ToString();
            return false;
        }



        //public static void Prefix_AddListValue(object listType, ref object list,
        //    int index, ref object value, bool canCreateInstance)
        //{
        //    //Console.WriteLine("qwe; hook AddListValue");
        //    //Console.WriteLine("qwe; listType: " + listType);
        //    //Console.WriteLine("qwe; listType Type: " + listType.GetType());
        //    //Console.WriteLine("qwe; index: " + index);
        //    //Console.WriteLine("qwe; value: " + value);
        //    //Console.WriteLine("qwe; value type: " + value.GetType());
        //    //Console.WriteLine("qwe; is enum: " + value.GetType().IsEnum);
        //    var valueType = value.GetType();
        //    if (valueType.IsEnum)
        //    {
        //        Console.WriteLine("qwe; try add value " + value);
        //    }
        //}

        //static string lastLoadFileName;
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(SaveGame), "Load")]
        //public static void PrefixSaveGameLoad(string filename,
        //    bool loadEmergencySave = false, bool loadBackupSave = false)
        //{
        //    Console.WriteLine("qwe; prefix save game load");
        //    Console.WriteLine("qwe; filename=" + filename
        //        + ", load emerg=" + loadEmergencySave
        //        + ", load backup=" + loadBackupSave);
        //    lastLoadFileName = filename;
        //}

        //static readonly string SaveGameFilename = "spacecore-serialization.json";
        //static readonly string FarmerFilename = "spacecore-serialization-farmer.json";

        //public static bool Prefix_DeserializeProxy(ref object __result, XmlSerializer serializer,
        //    Stream stream, string farmerPath, bool fromSaveGame)
        //{
        //    Console.WriteLine("qwe: hook DeserializeProxy: farmerPath=" + farmerPath + ", from save game: " + fromSaveGame);

        //    string filePath;
        //    if (fromSaveGame)
        //    {

        //        var farmerSerializer = typeof(SaveGame).GetField("farmerSerializer").GetValue(null);
        //        farmerPath = Path.Combine(Constants.SavesPath, lastLoadFileName);
        //        var isSaveFarmer = serializer == farmerSerializer;
        //        string filename = isSaveFarmer ? FarmerFilename : SaveGameFilename;
        //        filePath = Path.Combine(farmerPath, filename);
        //        Console.WriteLine("qwe; is save farmer serialier: " + isSaveFarmer);
        //        Console.WriteLine("qwe; fix file path 1=" + filePath);
        //    }
        //    else
        //    {
        //        filePath = Path.Combine(farmerPath, FarmerFilename);
        //        Console.WriteLine("qwe; fix file path 2=" + filePath);
        //    }
        //    XmlDocument doc = new XmlDocument();
        //    try
        //    {
        //        doc.Load(stream);
        //        Console.WriteLine("qwe; done xml doc loaded: " + doc.Name);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("qwe; failed docs load stream: " + ex.Message);
        //    }
        //    using XmlTextReader reader = new XmlTextReader(new StringReader(doc.OuterXml));
        //    try
        //    {
        //        Console.WriteLine("qwe; serializeer type: " + serializer);
        //        __result = serializer.Deserialize(reader);
        //        Console.WriteLine("qwe; done deserialize: " + __result);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("qwe; error try dese: " + e.Message);
        //    }

        //    //foreach (var f in new StackTrace().GetFrames())
        //    //{
        //    //    Console.WriteLine("qwe: in frame: " + f.GetMethod());
        //    //}

        //    return false;
        //}


        //public static void PrefixDeserialize(Stream stream, object __instance)
        //{
        //    Console.WriteLine("qwe; xml serializer type: " + __instance);
        //    foreach (var f in new StackTrace().GetFrames())
        //    {
        //        var m = f.GetMethod();
        //        Console.WriteLine("qwe; call method: " + m.Name);
        //    }
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(XmlSerializationReader1), "Read306_SaveGame")]
        //static void PrefixRead306_SaveGame(
        //    bool isNullable, bool checkType)
        //{
        //    Console.WriteLine("qwe; hook Read306_SaveGame");
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(XmlSerializationReader1), "Read306_SaveGame")]
        //static void Postfix_Read306_SaveGame(
        //    bool isNullable, bool checkType)
        //{
        //    Console.WriteLine("qwe; postfix Read306_SaveGame");
        //}
    }
}
