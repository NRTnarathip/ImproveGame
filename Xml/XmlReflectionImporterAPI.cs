using HarmonyLib;
using System.Reflection;
using System.Xml.Serialization;

namespace ImproveGame.Xml;

internal static class XmlReflectionImporterAPI
{
    public static Type ThisType = AccessTools.TypeByName(XmlAPI.SerializationFullName + ".XmlReflectionImporter");
    public static Type GetThisType() => AccessTools.TypeByName(XmlAPI.SerializationFullName + ".XmlReflectionImporter");

    public static MethodInfo Method(string name, Type[] paramTypes = null)
        => AccessTools.Method(ThisType, name, paramTypes);

    public static object CreateMapMember(this XmlReflectionImporter importer,
        Type declaringType, XmlReflectionMember rmember, string defaultNamespace)
    {
        return Method("CreateMapMember").Invoke(importer, [declaringType, rmember, defaultNamespace]);
    }
    public static List<XmlReflectionMember> GetReflectionMembers(this XmlReflectionImporter importer, Type type)
    {
        return Method("GetReflectionMembers").Invoke(importer, [type]) as List<XmlReflectionMember>;
    }

}
