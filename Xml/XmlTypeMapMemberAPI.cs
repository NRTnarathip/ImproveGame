using HarmonyLib;
using System.Reflection;

namespace ImproveGame.XmlAPI;

internal class XmlTypeMapMemberAPI
{
    public const string _FullName = "System.Xml.Serialization.XmlTypeMapMember";
    public static Type ThisType = AccessTools.TypeByName(_FullName);
    public static MethodInfo NamePropertyGetter = AccessTools.PropertyGetter(ThisType, "Name");
    public static FieldInfo _member_FieldInfo = AccessTools.Field(ThisType, "_member");

    object obj;
    public XmlTypeMapMemberAPI(object obj)
    {
        this.obj = obj;
    }
    public string Name
    {
        get => GetName(obj);
    }
    public static string GetName(object obj) => NamePropertyGetter.Invoke(obj, []) as string;

    public static MemberInfo GetMemberInfo(object obj)
    {
        return _member_FieldInfo.GetValue(obj) as MemberInfo;
    }
    public static void PrintMemberInfo(object typeMapMember)
    {
        var member = GetMemberInfo(typeMapMember);
        if (member == null)
        {
            Console.WriteLine("MemberInfo: is null!!");
            return;
        }

        try
        {
            Console.WriteLine($"MemberInfo: name: {member.Name}, module: memType: {member.MemberType}"
              + ", ");
            //var json = JsonConvert.SerializeObject(member);
            //Console.WriteLine($"MemberInfo: " + json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("failed try print MemberInfo: " + ex);
        }
    }
}
