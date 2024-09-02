using HarmonyLib;
using ImproveGame.Xml;
using ImproveGame.XmlAPI;
using Netcode;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;

namespace ImproveGame;

internal static class SpaceCoreSerializerCustomFix
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
    public static Type ThisType = typeof(SpaceCoreSerializerCustomFix);
    public static void Init()
    {
        //Disable custom proerties on Space Core
        //Work In Progress, should disable it
        const bool IsDisableCustomProperties = true;
        UnpatchSpaceCoreCustomProperties();
        if (!IsDisableCustomProperties)
        {
            //Init my sustom properties
            SpaceCoreCustomPropertyAPI.Init();
            ApplyPatcher();
            //notify custom save
            var mod = ModEntry.Instance;
            mod.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }
    }
    static void UnpatchSpaceCoreCustomProperties()
    {
        Type importer = AccessTools.TypeByName("System.Xml.Serialization.XmlReflectionImporter");
        Type TypeData_Type = AccessTools.TypeByName("System.Xml.Serialization.TypeData");
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
    }
    static void ApplyPatcher()
    {
        var harmony = ModEntry.Instance.harmony;
        Type importer = AccessTools.TypeByName("System.Xml.Serialization.XmlReflectionImporter");
        Type TypeData_Type = AccessTools.TypeByName("System.Xml.Serialization.TypeData");
        var WriterInterpreter_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
        var XmlTypeMapMember_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
        var XmlTypeMapMember_GetValue_Method = AccessTools.Method(
                   XmlTypeMapMember_Type, "GetValue", [typeof(object)]);
        var XmlTypeMapMember_SetValue_Method = AccessTools.Method(
          XmlTypeMapMember_Type, "SetValue", [typeof(object), typeof(object)]);
        var ImportClassMapping_Method = AccessTools.Method(importer, "ImportClassMapping",
                       [TypeData_Type, typeof(XmlRootAttribute), typeof(string), typeof(bool)]);

        harmony.Patch(
            original: XmlTypeMapMember_GetValue_Method,
            prefix: new(typeof(SpaceCoreSerializerCustomFix), nameof(Prefix_TypeMapMember_GetValue))
        );
        harmony.Patch(
            original: XmlTypeMapMember_SetValue_Method,
            prefix: new(typeof(SpaceCoreSerializerCustomFix), nameof(Prefix_TypeMapMember_SetValue))
        );
        harmony.Patch(
            original: ImportClassMapping_Method,
            postfix: new(typeof(SpaceCoreSerializerCustomFix), nameof(Postfix_ImportClassMapping))
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
    static bool ShouldLog(string memName, object obj = null)
    {
        if (memName == "hasRiveraSecret" || memName == "hasRustyKey"
               || memName == "hasCake" || memName == "isMale")
            return true;

        if (obj != null)
        {
            var objType = obj.GetType();
            if (objType == typeof(NetBool))
                return true;
        }
        return false;
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
        => new XmlSerializer(baseType, extraType);
    static void Prefix_SetListMembersDefaults(object map, object ob, bool isValueList)
    {
        var listMembers = ClassMapAPI.GetListMembers(map);
        if (listMembers != null)
        {
            Console.WriteLine("SetListMembersDefaults(), object:" + ob + ", listMembersCount: " + listMembers.Count);
            foreach (var member in listMembers)
            {
                Console.WriteLine("found member: " + member
                    + ", memName: " + XmlTypeMapMemberAPI.GetName(member));
            }
        }
        else
        {
            Console.WriteLine("SetListMembersDefaults(), object: " + ob);

        }
    }
    static void PrefixGetMemberValue(object member, object ob, bool isValueList)
    {
        var name = XmlTypeMapMemberAPI.GetName(member);
        if (ShouldLog(name))
            Console.WriteLine("ReaderInterpreter.GetMemberValue(), fieldName: " + name + ", isValueList: " + isValueList);
    }
    static void PrefixSetMemberValue(object member, object ob, object value, bool isValueList)
    {
        var name = XmlTypeMapMemberAPI.GetName(member);
        if (ShouldLog(name))
        {
            Console.WriteLine("ReaderInterpreter.SetMemberValue(), value: " + value
                + ", memName: " + name + ", isValueList: " + isValueList);
        }
    }
    public static void PrintStace()
    {

        int i = 0;
        foreach (var f in new StackTrace().GetFrames())
        {
            if (i >= 1)
            {
                var m = f.GetMethod();
                Console.WriteLine($"frame: {m.DeclaringType.Name}::{m}");
            }
            i++;
            if (i >= 12)
                break;
        }
    }
    static void Postfix_ImportClassMapping(ref XmlTypeMapping __result,
       object typeData, XmlRootAttribute root, string defaultNamespace, bool isBaseType = false)
    {
        var type = (Type)AccessTools.Field(typeData.GetType(), "type").GetValue(typeData);
        if (!SpaceCoreCustomPropertyAPI.CustomPropertyMap.ContainsKey(type))
            return;

        //Console.WriteLine("import class mapping for type: " + type);
        var XmlTypeMapMember_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
        var XmlTypeMapMember_Ctor = XmlTypeMapMember_Type.GetConstructor([]);

        object mapObject = AccessTools.Field(typeof(XmlTypeMapping), "map").GetValue(__result);
        var map_AddMember_Method = AccessTools.Method(mapObject.GetType(), "AddMember");
        var reflectionImporter = new XmlReflectionImporter();
        foreach (var propKvp in SpaceCoreCustomPropertyAPI.CustomPropertyMap[type])
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
                    Console.WriteLine("addded custom prop: " + prop.Name + ", in class: " + type);
                }
                catch (Exception ex)
                {
                    //just skip error, because you already this member
                    //Console.WriteLine("fail try to classMap.AddMember(): " + ex);
                }
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
        if (ShouldLog(memName, ob))
        {
            Console.WriteLine("TypeMapMember.SetValue() memNam: " + memName);
            XmlTypeMapMemberAPI.PrintMemberInfo(__instance);
            PrintStace();
        }
        if (!SpaceCoreCustomPropertyAPI.CustomPropertyMap.TryGetValue(ob.GetType(), out var props))
            return true;
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
        if (ShouldLog(memName))
        {
            Console.WriteLine("TypeMapMember.GetValue():");
            XmlTypeMapMemberAPI.PrintMemberInfo(__instance);
            PrintStace();
        }


        if (!SpaceCoreCustomPropertyAPI.CustomPropertyMap.TryGetValue(ob.GetType(), out var props))
            return true;

        if (props.TryGetValue(memName, out var prop))
        {
            __result = prop.Getter.Invoke(null, [ob]);
            return false;
        }

        return true;
    }
}
