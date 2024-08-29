using HarmonyLib;
using System.Reflection;

namespace ImproveGame.Xml;
internal class XmlAPI
{
    public const string SerializationFullName = "System.Xml.Serialization";
}
internal class ClassMapAPI
{
    public static Type ThisType = AccessTools.TypeByName(XmlAPI.SerializationFullName + ".ClassMap");
    public static MethodInfo GetMethod(string name, Type[] paramTypes = null)
    {
        return AccessTools.Method(ThisType, name, paramTypes);
    }
    public static void AddMember(object obj, object member)
    {
        GetMethod("AddMember").Invoke(obj, [member]);
    }
}
