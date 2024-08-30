using HarmonyLib;
using ImproveGame.Xml;
using ImproveGame.XmlAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System.Reflection;
using System.Xml.Serialization;

namespace ImproveGame;

internal static class SpaceCoreSerializerCustom
{
    static readonly Type[] VanillaMainTypes = {
            typeof(Tool),
            typeof(GameLocation),
            typeof(Duggy),
            typeof(Bug),
            typeof(BigSlime),
            typeof(Ghost),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Quest),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
            typeof(TerrainFeature)
        };
    static readonly Type[] VanillaFarmerTypes =
    {
        typeof(Tool)
    };
    static readonly Type[] VanillaGameLocationTypes =
    {
            typeof(Tool),
            typeof(Duggy),
            typeof(Ghost),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Bug),
            typeof(BigSlime),
            typeof(BreakableContainer),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
            typeof(TerrainFeature)
        };
    public static Type ThisType = typeof(SpaceCoreSerializerCustom);
    public static void Init()
    {
        SerializerCustomPropertyAPI.Init();
        ApplyPatcher();


        //ready
        var mod = ModEntry.Instance;
        mod.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
    }
    static void ApplyPatcher()
    {
        var harmony = ModEntry.Instance.harmony;
        Type importer = AccessTools.TypeByName("System.Xml.Serialization.XmlReflectionImporter");
        Type TypeData_Type = AccessTools.TypeByName("System.Xml.Serialization.TypeData");
        var WriterInterpreter_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
        var XmlTypeMapMember_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");


        var ImportClassMapping_Method = AccessTools.Method(importer, "ImportClassMapping",
                [TypeData_Type, typeof(XmlRootAttribute), typeof(string), typeof(bool)]);
        SpaceCoreAPI.Unpatch(ImportClassMapping_Method);

        var XmlTypeMapMember_GetValue_Method = AccessTools.Method(
            XmlTypeMapMember_Type, "GetValue", [typeof(object)]);
        SpaceCoreAPI.Unpatch(XmlTypeMapMember_GetValue_Method);

        var XmlTypeMapMember_SetValue_Method = AccessTools.Method(
            XmlTypeMapMember_Type, "SetValue", [typeof(object), typeof(object)]);
        SpaceCoreAPI.Unpatch(XmlTypeMapMember_SetValue_Method);

        harmony.Patch(
            original: XmlTypeMapMember_GetValue_Method,
            prefix: new(typeof(SpaceCoreSerializerCustom), nameof(Prefix_TypeMapMember_GetValue))
        );
        harmony.Patch(
            original: XmlTypeMapMember_SetValue_Method,
            prefix: new(typeof(SpaceCoreSerializerCustom), nameof(Prefix_TypeMapMember_SetValue))
        );
        harmony.Patch(
            original: ImportClassMapping_Method,
            postfix: new(typeof(SpaceCoreSerializerCustom), nameof(Postfix_ImportClassMapping))
        );

        harmony.Patch(
            original: AccessTools.Method(XmlTypeMapMember_Type, "InitMember"),
            prefix: new(AccessTools.Method(ThisType, nameof(Prefix_InitMember)))
        );

        var ReaderInterpreterType = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationReaderInterpreter");
        harmony.Patch(
            original: AccessTools.Method(ReaderInterpreterType, "SetMemberValue",
                [XmlTypeMapMemberAPI.ThisType, typeof(object), typeof(object), typeof(bool)]),
            prefix: new(AccessTools.Method(ThisType, nameof(PrefixSetMemberValue)))
        );
        harmony.Patch(
            original: AccessTools.Method(ReaderInterpreterType, "GetMemberValue",
                [XmlTypeMapMemberAPI.ThisType, typeof(object), typeof(bool)]),
            prefix: new(AccessTools.Method(ThisType, nameof(PrefixGetMemberValue)))
        );
        harmony.Patch(
            original: AccessTools.Method(ReaderInterpreterType, "SetListMembersDefaults"),
            prefix: new(AccessTools.Method(ThisType, nameof(Prefix_SetListMembersDefaults)))
        );

        harmony.Patch(
            AccessTools.Method(ClassMapAPI.ThisType, "AddMember"),
            prefix: new(AccessTools.Method(ThisType, nameof(ClassMap_AddMember)))
        );
    }
    static void ClassMap_AddMember(object __instance, object member)
    {
        return;
        var xmlMapMember = member;
        var name = XmlTypeMapMemberAPI.GetName(xmlMapMember);
        if (name == "hasRustyKey" || name == "hasCake" || name == "hasRiveraSecret")
        {
            Console.WriteLine("");
            Console.WriteLine("ClassMap.AddMember(), name: " + name);
            XmlTypeMapMemberAPI.PrintMemberInfo(member);
            Console.WriteLine("");
        }
    }

    static void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
    {
        //Recreate XmlSerializer with inject custom fields
        AccessTools.Field(typeof(SaveGame), "serializer")
            .SetValue(null, CreateSerializerWithExtraType(typeof(SaveGame), VanillaMainTypes));
        AccessTools.Field(typeof(SaveGame), "farmerSerializer")
            .SetValue(null, CreateSerializerWithExtraType(typeof(Farmer), VanillaFarmerTypes));
        AccessTools.Field(typeof(SaveGame), "locationSerializer")
            .SetValue(null, CreateSerializerWithExtraType(typeof(GameLocation), VanillaGameLocationTypes));
    }

    static MethodInfo GetMethodPatch(string name) => AccessTools.Method(ThisType, name);
    static XmlSerializer CreateSerializerWithExtraType(Type baseType, Type[] extraType)
    {
        //Console.WriteLine("try create serializer for baseType: " + baseType
        //    + ", extraTypesCount: " + extraType.Length);
        var serializer = new XmlSerializer(baseType, extraType);
        //Console.WriteLine("done create serializer for baseType: " + baseType);
        return serializer;
    }
    static void Prefix_SetListMembersDefaults(object map, object ob, bool isValueList)
    {
        return;
        var listMembers = ClassMapAPI.GetListMembers(map);
        if (listMembers != null)
        {
            Console.WriteLine("SetListMembersDefaults(), " + ob + ", listMembers: " + listMembers);
            //foreach (var member in listMembers)
            //{
            //    Console.WriteLine("found member: " + member
            //        + ", memName: " + XmlTypeMapMemberAPI.GetName(member));
            //}
        }
        else
        {
            Console.WriteLine("SetListMembersDefaults(), " + ob + ", without list members");

        }
    }
    static void PrefixGetMemberValue(object member, object ob, bool isValueList)
    {
        return;
        var name = XmlTypeMapMemberAPI.GetName(member);
        if (name.StartsWith("has"))
            Console.WriteLine("ReaderInterpreter.GetMemberValue(), fieldName: " + name + ", isValueList: " + isValueList);
    }

    static void PrefixSetMemberValue(object member, object ob, object value, bool isValueList)
    {
        return;

        var name = XmlTypeMapMemberAPI.GetName(member);
        if (name.StartsWith("has"))
        {
            Console.WriteLine("ReaderInterpreter.SetMemberValue(), value: " + value
                + ", memName: " + name + ", isValueList: " + isValueList);
        }
    }
    static bool Prefix_InitMember(object __instance, Type type)
    {
        if (!SerializerCustomPropertyAPI.CustomPropertyMap.TryGetValue(type, out var props))
            return true;

        if (props.TryGetValue(XmlTypeMapMemberAPI.GetName(__instance), out var prop))
        {
            Console.WriteLine("init member: " + prop.Name);
            XmlTypeMapMemberAPI._member_FieldInfo
                .SetValue(__instance, prop.GetFakePropertyInfo());
            return false;
        }

        return true;
    }

    static void Postfix_ImportClassMapping(ref XmlTypeMapping __result,
       object typeData, XmlRootAttribute root, string defaultNamespace, bool isBaseType = false)
    {
        var type = (Type)AccessTools.Field(typeData.GetType(), "type").GetValue(typeData);
        if (!SerializerCustomPropertyAPI.CustomPropertyMap.ContainsKey(type))
            return;

        //Console.WriteLine("import class mapping for type: " + type);
        var XmlTypeMapMember_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
        var XmlTypeMapMember_Ctor = XmlTypeMapMember_Type.GetConstructor([]);

        object mapObject = AccessTools.Field(typeof(XmlTypeMapping), "map").GetValue(__result);
        var map_AddMember_Method = AccessTools.Method(mapObject.GetType(), "AddMember");
        var reflectionImporter = new XmlReflectionImporter();
        foreach (var propKvp in SerializerCustomPropertyAPI.CustomPropertyMap[type])
        {
            var prop = propKvp.Value;
            //Console.WriteLine("try add custom prop: " + prop.Name + ", type: " + prop.PropertyType);
            try
            {
                var rmember = new XmlReflectionMember();
                rmember.MemberName = prop.Name;
                rmember.SetMemberType(prop.PropertyType);
                rmember.SetDeclaringType(prop.DeclaringType);

                try
                {
                    var mapMember = reflectionImporter.CreateMapMember(prop.DeclaringType, rmember, "");
                    XmlTypeMapMemberAPI._member_FieldInfo.SetValue(mapMember, prop.GetFakePropertyInfo());
                    ClassMapAPI.AddMember(mapObject, mapMember);
                }
                catch (Exception ex)
                {
                    //just skip error, because you already this member
                    //Console.WriteLine("fail try to classMap.AddMember(): " + ex);
                }
                Console.WriteLine("addded custom prop: " + prop.Name + ", in class: " + type);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed error try add custom prop: " + ex);
            }
        }
    }
    static bool Prefix_TypeMapMember_SetValue(object __instance, object ob, object value)
    {
        var memName = XmlTypeMapMemberAPI.GetName(__instance);
        if (memName == "hasRiveraSecret" || memName == "hasRustyKey" || memName == "hasCake")
        {
            Console.WriteLine("TypeMapMember.SetValue() value: " + value
                + ", objType: " + ob.GetType() + ", memName: " + memName);
            XmlTypeMapMemberAPI.PrintMemberInfo(__instance);
            //int i = 0;
            //foreach (var f in new StackTrace().GetFrames())
            //{
            //    Console.WriteLine("frame: " + f.GetMethod());
            //    i++;
            //    if (i >= 5)
            //        break;
            //}
        }
        if (!SerializerCustomPropertyAPI.CustomPropertyMap.TryGetValue(ob.GetType(), out var props))
            return true;

        {
            //Console.WriteLine("call stack trace");
            //foreach (var f in new StackTrace().GetFrames())
            //{
            //    Console.WriteLine("frame: " + f.GetMethod().FullName());
            //}
        }
        if (props.TryGetValue(memName, out var prop))
        {
            prop.Setter.Invoke(null, [ob, value]);
            return false;
        }

        return true;
    }
    static bool Prefix_TypeMapMember_GetValue(object __instance, object ob, ref object __result)
    {
        var memName = XmlTypeMapMemberAPI.GetName(__instance);
        if (memName == "hasRiveraSecret" || memName == "hasRustyKey" || memName == "hasCake")
        {
            var member = XmlTypeMapMemberAPI.GetMemberInfo(__instance);
            Console.WriteLine("TypeMapMember.GetValue(): memName: " + memName
                + ", member is PropInfo: " + member is PropertyInfo);
        }


        if (!SerializerCustomPropertyAPI.CustomPropertyMap.TryGetValue(ob.GetType(), out var props))
            return true;

        if (props.TryGetValue(memName, out var prop))
        {
            __result = prop.Getter.Invoke(null, [ob]);
            return false;
        }

        return true;
    }
}
