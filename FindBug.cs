using HarmonyLib;
using System.Xml.Serialization;

namespace ImproveGame;

internal class FindBug : BasePatcher
{
    public FindBug()
    {
        var harmony = ModEntry.Instance.harmony;
        {
            var type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
            //PatchPrefix(GetMethod(type, "WriteRoot"), nameof(Prefix_WriteRoot));
        }

        {
            var type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializer");
            //PatchPrefix(GetMethod(type, "Serialize", ["object", "XmlSerializationWriter"]), nameof(Prefix_Serialize));
        }
        {
            //type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationReaderInterpreter");
            //method = type.GetMethod("GetValueFromXmlString",
            //    BindingFlags.Instance | BindingFlags.NonPublic);
            //harmony.Patch(method,
            //    transpiler: new(typeof(SpaceCoreCrashFix), nameof(Transpiler_GetValueFromXmlString)),
            //    prefix: new(typeof(SpaceCoreCrashFix), nameof(Prefix_GetValueFromXmlString)));

            //method = type.GetMethod("ReadPrimitiveValue", BindingFlags.Instance | BindingFlags.NonPublic);
            //harmony.Patch(method,
            //    prefix: new(typeof(SpaceCoreCrashFix), nameof(Prefix_ReadPrimitiveValue))
            //);

            //type = AccessTools.TypeByName("System.Xml.Serialization.XmlSerializationWriterInterpreter");
            //method = type.GetMethod("GetStringValue", BindingFlags.Instance | BindingFlags.NonPublic);
            //harmony.Patch(method,
            //    prefix: new(typeof(SpaceCoreCrashFix), nameof(Prefix_GetStringValue))
            //    );
            //method = type.GetMethod("WriteMemberElement", BindingFlags.Instance | BindingFlags.NonPublic);
            //harmony.Patch(method,
            //    prefix: new(typeof(SpaceCoreCrashFix), nameof(Prefix_WriteMemberElement))
            //);


            //type = AccessTools.TypeByName("System.Xml.Serialization.XmlCustomFormatter");
            //method = type.GetMethod("FromXmlString", BindingFlags.Static | BindingFlags.NonPublic);
            //harmony.Patch(method,
            //    prefix: new(typeof(SpaceCoreCrashFix), nameof(Prefix_FromXmlString)));

        }


    }
    public static void Init()
    {
        new FindBug();
    }
    static void Prefix_Serialize(object __instance, object o, XmlSerializationWriter writer)
    {
        Console.WriteLine($"qwe Serialize(), object: {o}:type:{o.GetType()}, writer: {writer}");
    }
    static void Prefix_WriteRoot(object ob)
    {
        //Console.WriteLine($"qwe; WriteRoot(); object: {ob}");
    }

    //static object GetTypeData_ElementName(object typeData)
    //{
    //    return TypeData_elementName_FieldInfo.GetValue(typeData);
    //}

    //static void WriteMemberElement(object elem, object memberValue)
    //{
    //    var elemName = GetXmlTypeMapElementInfo_ElementName(elem);
    //    Console.WriteLine($"qwe; WriteMemberElement(), elemName: {elemName}");
    //}


    //static Type XmlTypeMapElementInfo_Type = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapElementInfo");
    //static FieldInfo XmlTypeMapElementInfo_isNull_FieldInfo
    //    = XmlTypeMapElementInfo_Type.GetField("_isNullable", BindingFlags.Instance | BindingFlags.NonPublic);
    //static FieldInfo XmlTypeMapElementInfo_elementName_FieldInfo
    //    = XmlTypeMapElementInfo_Type.GetField("_elementName", BindingFlags.Instance | BindingFlags.NonPublic);
    //static string GetXmlTypeMapElementInfo_ElementName(object typeMap)
    //{
    //    return XmlTypeMapElementInfo_elementName_FieldInfo.GetValue(typeMap) as string;
    //}



    //static string GetTypeData_XmlType(object typeData) => GetTypeData_ElementName(typeData) as string;
    //static object GetTypeData_SchemaTypes(object typeData)
    //{
    //    return TypeData_sType_FieldInfo.GetValue(typeData);
    //}

    //static void Prefix_WriteMemberElement(object elem, object memberValue)
    //{
    //    var elemName = GetXmlTypeMapElementInfo_ElementName(elem);
    //    Console.WriteLine($"qwe; WriteMemberElement(), elemName: {elemName}, memberValue: {memberValue}");
    //}
    //static void Prefix_GetStringValue(XmlTypeMapping typeMap, object type, object value)
    //{
    //    var xmlType = GetTypeData_XmlType(type);
    //    var sType = GetTypeData_SchemaTypes(type);
    //    Console.WriteLine($"wqe; Writer.GetStringValue(), value: {value},valueType: {value?.GetType()}"
    //        + $", xmlType: {xmlType}, sType: {sType}");
    //}


    //static void Prefix_ReadPrimitiveValue(object elem)
    //{
    //    var elemName = XmlTypeMapElementInfo_elementName_FieldInfo.GetValue(elem) as string;
    //    Console.WriteLine($"qwe ReadPrimValue(),  elemName: {elemName}");
    //}

    //static void Prefix_FromXmlString(object type, string value)
    //{
    //    var typeInfo = TypeData_type_Field.GetValue(type) as Type;
    //    //Console.WriteLine($"qwe XmlCustomFormatter.FromXmlString(), type: {typeInfo},value: {value},valueType: {value.GetType()}");
    //    if (value == null)
    //    {
    //        //Console.WriteLine("qwe FromXMlString value is null");
    //        //PrintStack();
    //    }
    //    else if (value == String.Empty)
    //    {
    //        //Console.WriteLine("qwe FromXMlString value is empty");
    //        //PrintStack();
    //    }
    //}

    //static void Prefix_GetValueFromXmlString(string value, object typeData, XmlTypeMapping typeMap)
    //{
    //    var type = TypeData_type_Field.GetValue(typeData) as Type;
    //    var xmlType = TypeData_elementName_FieldInfo.GetValue(typeData);
    //    //Console.WriteLine($"qwe; GetValueFromXmlString(), value: {value}, "
    //    //    + $", valueType: {value.GetType()}"
    //    //    + $", typeMap.type: {type}, xmlType: {xmlType}");
    //}

}
