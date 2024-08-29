using HarmonyLib;
using ImproveGame.Xml;
using ImproveGame.XmlAPI;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace ImproveGame;

internal static class SpaceCoreSerializerFix
{
    public static Type ThisType = typeof(SpaceCoreSerializerFix);
    static MethodInfo GetMethodPatch(string name) => AccessTools.Method(ThisType, name);
    public static void Init()
    {
        var harmony = ModEntry.Instance.harmony;
        Type importer = AccessTools.TypeByName("System.Xml.Serialization.XmlReflectionImporter");
        Type TypeData_Type = AccessTools.TypeByName("System.Xml.Serialization.TypeData");
        var WriterInterpreter_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
        var XmlTypeMapMember_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");


        harmony.Patch(
            original: AccessTools.Method(typeof(XmlSerializer), "Serialize", [typeof(XmlWriter), typeof(object)]),
            prefix: new(typeof(SpaceCoreSerializerFix), nameof(Pre_Serialize))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(XmlSerializer), "Deserialize", [typeof(XmlReader)]),
            prefix: new(typeof(SpaceCoreSerializerFix), nameof(Prefix_Deserialize))
        );

        harmony.Patch(
            original: AccessTools.Method(WriterInterpreter_Type, "WriteElementMembers"),
            prefix: new(typeof(SpaceCoreSerializerFix), nameof(Prefix_WriteElementMembers))
        );

        var ImportClassMapping_Method = AccessTools.Method(importer, "ImportClassMapping",
                [TypeData_Type, typeof(XmlRootAttribute), typeof(string), typeof(bool)]);
        SpaceCoreAPI.Unpatch(ImportClassMapping_Method);
        SpaceCoreAPI.Unpatch(AccessTools.Method(XmlTypeMapMember_Type, "GetValue", [typeof(object)]));
        SpaceCoreAPI.Unpatch(AccessTools.Method(XmlTypeMapMember_Type, "SetValue", [typeof(object), typeof(object)]));
        harmony.Patch(
            original: ImportClassMapping_Method,
            postfix: new(typeof(SpaceCoreSerializerFix), nameof(Postfix_ImportClassMapping))
        );
        harmony.Patch(
            original: AccessTools.Method(XmlTypeMapMember_Type, "GetValue", new Type[] { typeof(object) }),
            prefix: new(typeof(SpaceCoreSerializerFix), nameof(Prefix_GetValue))
        );

        harmony.Patch(
            original: AccessTools.Method(XmlTypeMapMember_Type, "SetValue", new Type[] { typeof(object), typeof(object) }),
            prefix: new(typeof(SpaceCoreSerializerFix), nameof(Prefix_SetValue))
        );
        harmony.Patch(
            original: ClassMapAPI.GetMethod("AddMember"),
            prefix: new(GetMethodPatch(nameof(Prefix_AddMember)))
        );


        ClassMap_elementMembers_FieldInfo = AccessTools.Field(ClassMapAPI.ThisType, "_elementMembers");
        XmlTypeMapMemberElement_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMemberElement");
        XmlTypeMapMemberElement_choiceMember_FieldInfo = AccessTools.Field(XmlTypeMapMemberElement_Type, "_choiceMember");

        //add simple custom type
        SuperPlayerCustomPropTest.Apply();
    }
    static Type ClassMap_Type;
    static FieldInfo ClassMap_elementMembers_FieldInfo;
    static Type XmlTypeMapMemberElement_Type;
    static FieldInfo XmlTypeMapMemberElement_choiceMember_FieldInfo;

    static void Prefix_AddMember(object __instance, object member)
    {
        XmlTypeMapMemberAPI memberApi = new XmlTypeMapMemberAPI(member);
        //Console.WriteLine("addMember: " + memberApi.Name);
    }
    static void Prefix_Deserialize(object __instance, XmlReader xmlReader)
    {
        //Console.WriteLine("qwe prefix XmlSerializer.Deserialize(): " + xmlReader + ", instance: " + __instance);
    }
    static void Pre_Serialize(XmlWriter xmlWriter, object o)
    {
        //Console.WriteLine("qwe prefix XmlSerializer.Serialize(): " + xmlWriter + ", object: " + o);
    }

    static void Prefix_WriteElementMembers(object map, object ob, bool isValueList)
    {
        return;
        ArrayList elementMembers = (ArrayList)ClassMap_elementMembers_FieldInfo.GetValue(map);
        if (elementMembers == null)
            return;

        Console.WriteLine("qwe prefix WriteElemMembers():: classMapValue: " + map + ", obValue: " + ob);
        var XmlTypeMapMemer = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
        var XmlTypeMapMbmer_name = AccessTools.Field(XmlTypeMapMemer, "_name");

        foreach (object memberElement in elementMembers)
        {
            var memberType = memberElement.GetType();
            var memName = XmlTypeMapMbmer_name.GetValue(memberElement);
            Console.WriteLine("qwe found mem type: " + memberType + $", memName: {memName}");
        }
    }

    static void Postfix_ImportClassMapping(object __instance, ref XmlTypeMapping __result,
     object typeData, XmlRootAttribute root, string defaultNamespace, bool isBaseType = false)
    {
        var type = (Type)AccessTools.Field(typeData.GetType(), "type").GetValue(typeData);
        if (!CustomSerializer.CustomProperties.ContainsKey(type))
            return;

        Console.WriteLine("found custom field for class type: " + type);
        var XmlTypeMapMember = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
        var XmlTypeMapMember_Ctor = XmlTypeMapMember.GetConstructor([]);

        object mapObject = AccessTools.Field(typeof(XmlTypeMapping), "map").GetValue(__result);
        //Console.WriteLine("xmlTypeMap_mapFieldValue: " + mapObject);
        var map_AddMember_Method = AccessTools.Method(mapObject.GetType(), "AddMember");
        //Console.WriteLine("map_AddMethod(): " + map_AddMember_Method);
        foreach (var propKvp in CustomSerializer.CustomProperties[type])
        {
            var prop = propKvp.Value;
            Console.WriteLine("try add custom prop: " + prop.Name + ", type: " + prop.PropertyType);
            try
            {
                var reflectionImporter = new XmlReflectionImporter();
                XmlReflectionMember rmember = new XmlReflectionMember();
                rmember.MemberName = prop.Name;
                rmember.SetMemberType(prop.PropertyType);
                rmember.SetDeclaringType(prop.DeclaringType);

                var member = reflectionImporter.CreateMapMember(prop.DeclaringType, rmember, "");
                var memberAPI = new XmlTypeMapMemberAPI(member);
                Console.WriteLine("member name: " + memberAPI.Name);
                ClassMapAPI.AddMember(mapObject, member);
                //map_AddMember_Method.Invoke(mapObject, [member]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed error try add custom prop: " + ex);
            }
            Console.WriteLine("addded custom prop: " + prop.Name);
        }
    }
    static bool Prefix_SetValue(object __instance, object ob, object value, string ____name)
    {
        if (!CustomSerializer.CustomProperties.TryGetValue(ob.GetType(), out var props))
            return true;

        if (props.TryGetValue(____name, out var prop))
        {
            Console.WriteLine("prefix on setValue: " + ____name + ", objType:" + ob.GetType());
            prop.Setter.Invoke(null, [ob, value]);
            return false;
        }

        return true;
    }
    static bool Prefix_GetValue(object __instance, object ob, string ____name, ref object __result)
    {
        if (!CustomSerializer.CustomProperties.TryGetValue(ob.GetType(), out var props))
            return true;

        if (props.TryGetValue(____name, out var prop))
        {
            Console.WriteLine("prefix on getvalue: " + ____name + ", objType:" + ob.GetType());
            __result = prop.Getter.Invoke(null, [ob]);
            return false;
        }

        return true;
    }

    //public static void Postfix_ImportClassMapping(object __instance, ref XmlTypeMapping __result,
    //  object typeData, XmlRootAttribute root, string defaultNamespace, bool isBaseType = false)
    //{
    //Type type = AccessTools.Field(typeData.GetType(), "type").GetValue(typeData) as Type;
    //var customProperties = SpaceCoreAPI.ReadField_CustomProperties();
    //var customPropertiesValueType = SpaceCoreAPI.GetCustomPropertiesValueType();
    //var ContainsKey_Method = customPropertiesValueType.GetMethod("ContainsKey", BindingFlags.Public | BindingFlags.Instance);
    //if (!__result.TypeFullName.StartsWith("StardewValley."))
    //    return;
    //Console.WriteLine("qwe import class mapping: for type " + type + ", xmpTypeMapping TypeFullName: " + __instance);
    //foreach (DictionaryEntry entry in (IDictionary)customProperties)
    //{
    //    Console.WriteLine("qwe found custom field type: " + entry.Key);
    //}
    //if (!(bool)(ContainsKey_Method.Invoke(customProperties, [type])))
    //    return;


    //Console.WriteLine("qwe try add CustomField in class: " + type);

    //var memberType = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
    //var memberTypeConstructor = memberType.GetConstructor([]);

    //object map = AccessTools.Field(typeof(XmlTypeMapping), "map").GetValue(__result);
    //var mapAddMethod = AccessTools.Method(map.GetType(), "AddMember");
    //MethodInfo getItemMethod = customPropertiesValueType.GetMethod("get_Item");
    //var innerDictionary = (IDictionary)getItemMethod.Invoke(customProperties, [type]);


    //foreach (DictionaryEntry customPropertyInfoKvp in innerDictionary)
    //{
    //    Console.WriteLine("qwe custom prop kvp: " + customPropertyInfoKvp);
    //    continue;

    //    object member = memberTypeConstructor.Invoke([]);

    //    var customPropertyInfo = customPropertyInfoKvp.Value;
    //    var propertyType = CustomPropertyInfoAPI.Get_PropertyType(customPropertyInfo);
    //    Console.WriteLine($"qwe customProp name: {customPropertyInfoKvp.Key}, propType: {propertyType}");

    //    AccessTools.Property(memberType, "Name").SetValue(member, customPropertyInfoKvp.Key);
    //    AccessTools.Property(memberType, "TypeData").SetValue(member, AccessTools.Method("System.Xml.Serialization.TypeTranslator:GetTypeData", [typeof(Type)]).Invoke(null, [propertyType]));
    //    mapAddMethod.Invoke(map, [member]);
    //}

}
