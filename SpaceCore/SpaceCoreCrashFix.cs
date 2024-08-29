using HarmonyLib;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Serialization;

#pragma warning disable CS8601 // Possible null reference assignment.
namespace ImproveGame;

[HarmonyPatch]
class SpaceCoreCrashFix
{
    public static void Init()
    {
        {
            var harmonySpaceCore = new Harmony("spacechase0.SpaceCore");
            var spaceCoreAsm = SpaceCoreAPI.MainAssembly;
            {
                var method = spaceCoreAsm.GetType("SpaceCore.Patches.SaveGamePatcher")
                    .GetMethod("SerializeProxy", BindingFlags.Static | BindingFlags.NonPublic);
                ModEntry.Instance.harmony.Patch(method,
                    prefix: new(typeof(SpaceCoreCrashFix), nameof(Prefix_SerializeProxy)));
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
        var WriterInterpreter_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
        {
            var method = WriterInterpreter_Type.GetMethod("GetEnumXmlValue",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, [typeof(XmlTypeMapping), typeof(object)], null);
            harmony.Patch(method,
                new(typeof(SpaceCoreCrashFix).GetMethod(nameof(Prefix_GetEnumXmlValue))));
        }
        {
            var type = AccessTools.TypeByName("System.Xml.Serialization.XmlReflectionImporter");
            var method = type.GetMethod("GetReflectionMembers",
                BindingFlags.Instance | BindingFlags.NonPublic);
            harmony.Patch(method,
                postfix: new(typeof(SpaceCoreCrashFix), nameof(Postfix_Fixed_GetReflectionMembers)));
        }
        {

            //harmony.Patch(
            //    original: ImportClassMapping_MethodInfo,
            //    postfix: new(typeof(SpaceCoreCrashFix), nameof(Postfix_ImportClassMapping))
            //);




            //test save serialize

        }
    }





    //src code https://github.com/ZaneYork/SMAPI
    static IEnumerable<CodeInstruction> Transpiler_GetValueFromXmlString(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
    {
        List<CodeInstruction> newInsns = new();
        foreach (var insn in insns)
        {
            if (insn.opcode == OpCodes.Bne_Un_S)
            {
                if (newInsns[newInsns.Count - 1].opcode == OpCodes.Ldc_I4_2)
                {
                    var lastIns = newInsns[newInsns.Count - 2];
                    if (lastIns.opcode == OpCodes.Callvirt && lastIns.operand is MethodInfo minfo && minfo.DeclaringType.FullName == "System.Xml.Serialization.TypeData" && minfo.Name == "get_SchemaType")
                    {
                        newInsns.Add(insn);
                        Label continueLabel = gen.DefineLabel();
                        Label retLabel = gen.DefineLabel();
                        newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                        newInsns.Add(new CodeInstruction(OpCodes.Brfalse_S, retLabel));
                        newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                        newInsns.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(string), nameof(string.Length))));
                        newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                        newInsns.Add(new CodeInstruction(OpCodes.Cgt));
                        newInsns.Add(new CodeInstruction(OpCodes.Brfalse_S, retLabel));
                        newInsns.Add(new CodeInstruction(OpCodes.Br_S, continueLabel));

                        newInsns.Add(new CodeInstruction(OpCodes.Ldnull).WithLabels(retLabel));
                        newInsns.Add(new CodeInstruction(OpCodes.Ret));

                        CodeInstruction label = new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel);
                        newInsns.Add(label);
                        continue;
                    }
                }
            }
            newInsns.Add(insn);
        }
        return newInsns;
    }


    static void Postfix_Fixed_GetReflectionMembers(ref List<XmlReflectionMember> __result, Type type)
    {
        if (!type.FullName.StartsWith("StardewValley"))
            return;
        foreach (XmlReflectionMember member in __result)
        {
            if (member.MemberType.FullName.StartsWith("Netcode.NetEvent"))
            {
                member.XmlAttributes.XmlIgnore = true;
            }
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
    static FieldInfo TypeData_elementName_FieldInfo = TypeData_Type.GetField("elementName",
        BindingFlags.Instance | BindingFlags.NonPublic);
    static FieldInfo TypeData_sType_FieldInfo = TypeData_Type.GetField("sType",
        BindingFlags.Instance | BindingFlags.NonPublic);

    public static void Prefix_GetEnumXmlValue(XmlTypeMapping typeMap, ref object ob)
    {
        if (ob is string enumValueString)
        {
            var typeData = XmlTypeMapping_type_Field.GetValue(typeMap);
            var enumType = TypeData_type_Field.GetValue(typeData) as Type;
            ob = Enum.Parse(enumType, enumValueString);
        }
    }

    public static bool Prefix_NetEnum_Add(object __instance, ref object value)
    {
        value = value.ToString();
        return false;
    }
}
