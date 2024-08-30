using HarmonyLib;
using ImproveGame.XmlAPI;
using System.Collections;
using System.Reflection;

namespace ImproveGame.Xml;
internal class XmlAPI
{
    public const string SerializationFullName = "System.Xml.Serialization";
}
internal class ClassMapAPI
{
    public static Type ThisType = AccessTools.TypeByName(XmlAPI.SerializationFullName + ".ClassMap");
    public static FieldInfo _listMembers_FieldInfo = AccessTools.Field(ThisType, "_listMembers");
    public static ArrayList GetListMembers(object obj) => _listMembers_FieldInfo.GetValue(obj) as ArrayList;
    public static MethodInfo GetMethod(string name, Type[] paramTypes = null)
    {
        return AccessTools.Method(ThisType, name, paramTypes);
    }
    public static void AddMember(object obj, object member)
    {
        GetMethod("AddMember", [XmlTypeMapMemberAPI.ThisType]).Invoke(obj, [member]);
    }
}
