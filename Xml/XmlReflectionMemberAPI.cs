using HarmonyLib;
using System.Reflection;
using System.Xml.Serialization;

namespace ImproveGame.Xml;

static class XmlReflectionMemberAPI_MethodHelper
{
    public static void SetDeclaringType(this XmlReflectionMember rmember, Type type)
    {
        XmlReflectionMemberAPI.declaringType_Field.SetValue(rmember, type);
    }
    public static void SetMemberType(this XmlReflectionMember rmember, Type type)
    {
        XmlReflectionMemberAPI.memberType_Field.SetValue(rmember, type);
    }
}
internal class XmlReflectionMemberAPI
{
    static Type ThisType = typeof(XmlReflectionMember);
    public static FieldInfo declaringType_Field = AccessTools.Field(ThisType, "declaringType");
    public static FieldInfo memberType_Field = AccessTools.Field(ThisType, "memberType");
}
