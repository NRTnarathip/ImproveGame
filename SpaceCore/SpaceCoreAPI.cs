using HarmonyLib;
using System.Reflection;

namespace ImproveGame;
static class SpaceCoreAPI
{
    public static Assembly MainAssembly;
    public static Harmony harmony;
    public static void Init()
    {
        MainAssembly = Assembly.Load("SpaceCore");
        harmony = new(ModID);

        CustomPropertyInfoAPI.Init();
    }

    public const string ModID = "spacechase0.SpaceCore";
    public static bool IsLoaded() => ModEntry.Instance.Helper.ModRegistry.IsLoaded(ModID);
    public static bool IsNotLoad() => !IsLoaded();
    static Dictionary<string, Type> cacheTypes = new();
    public static Type TypeByName(string name)
    {
        if (!cacheTypes.TryGetValue(name, out Type type))
        {
            type = MainAssembly.GetType(name);
            if (type == null)
                return null;

            cacheTypes[name] = type;
        }
        return type;
    }
    public static FieldInfo FieldByName(Type type, string name)
    {
        return type.GetField(name, BasePatcher.AllFlags);
    }
    static Type CustomProperties_ValueType;
    public static Type GetCustomPropertiesValueType()
    {
        if (CustomProperties_ValueType == null)
            CustomProperties_ValueType = ReadField_CustomProperties().GetType();
        return CustomProperties_ValueType;
    }
    static FieldInfo CustomPropertiesType_FieldInfo;
    public static object ReadField_CustomProperties()
    {
        if (CustomPropertiesType_FieldInfo == null)
        {
            var spaceCoreType = TypeByName("SpaceCore.SpaceCore");
            CustomPropertiesType_FieldInfo = spaceCoreType.GetField("CustomProperties", BasePatcher.AllFlags);
        }
        return CustomPropertiesType_FieldInfo.GetValue(null);
    }
    public static void Unpatch(MethodBase method)
    {
        harmony.Unpatch(method, HarmonyPatchType.All);
        var logText = "unpatch SpaceCore at method: " + method;
        Logger.Log(logText);
        Console.WriteLine(logText);
    }
    public static void Unpatch(MethodBase[] methods)
    {
        foreach (var method in methods)
            Unpatch(method);
    }
}

