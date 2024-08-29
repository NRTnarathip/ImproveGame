using HarmonyLib;
using System.Reflection;

namespace ImproveGame.XmlAPI;

internal class XmlTypeMapMemberAPI
{
    public const string _FullName = "System.Xml.Serialization.XmlTypeMapMember";
    static Type _Type = AccessTools.TypeByName(_FullName);
    static MethodInfo NamePropertyGetter = AccessTools.PropertyGetter(_Type, "Name");
    object obj;
    public XmlTypeMapMemberAPI(object obj)
    {
        this.obj = obj;
    }
    public string Name
    {
        get => NamePropertyGetter.Invoke(obj, []) as string;
    }
}
