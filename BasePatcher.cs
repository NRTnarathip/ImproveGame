using HarmonyLib;
using StardewModdingAPI;
using System.Reflection;

namespace ImproveGame;

public class BasePatcher
{
    public static BindingFlags AllFlags = BindingFlags.Static | BindingFlags.Public
         | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

    Type thisType;
    public BasePatcher()
    {
        thisType = GetType();
    }

    public ConstructorInfo GetConstructor(Type type, Type[] paramTypes)
    {
        return GetConstructor(type, paramTypes.Select(p => p.Name).ToArray());
    }
    public ConstructorInfo GetConstructor(Type type, string[] paramTypeNames)
    {
        var all = type.GetConstructors(AllFlags);
        foreach (var ctor in all)
        {
            var _params = ctor.GetParameters();
            if (_params.Length == paramTypeNames.Length)
            {
                bool isMatch = true;
                for (int i = 0; i < _params.Length; i++)
                {
                    var paramTypeName = _params[i].ParameterType.Name.ToLower();
                    if (paramTypeName != paramTypeNames[i].ToLower())
                        isMatch = false;
                }
                if (isMatch)
                {
                    return ctor;
                }
            }
        }
        LogError("error not found consturctor: " + type);
        return null;
    }
    public MethodInfo GetMethod(string name) => GetMethod(thisType, name);
    public MethodInfo GetMethod(Type type, string name, string[] paramTypeNames)
    {
        var methods = type.GetMethods(AllFlags).Where(m => m.Name == name).ToArray();
        foreach (var method in methods)
        {
            var _params = method.GetParameters();
            if (_params.Length == paramTypeNames.Length)
            {
                bool isMatch = true;
                for (int i = 0; i < _params.Length; i++)
                {
                    var paramTypeName = _params[i].ParameterType.Name.ToLower();
                    if (paramTypeName != paramTypeNames[i].ToLower())
                        isMatch = false;
                }
                if (isMatch)
                {
                    return method;

                }
            }
        }
        Log("error not found method: " + type + "::" + name + ", paramTypeCount: " + paramTypeNames.Length);
        return null;
    }
    public MethodInfo GetMethod(Type type, string name) => type.GetMethod(name, AllFlags);
    Dictionary<string, Assembly> cacheAssembly = new();
    public Type GetType(string asmName, string fullTypeName)
    {
        if (!cacheAssembly.TryGetValue(asmName, out Assembly assembly))
        {
            assembly = Assembly.Load(asmName);
            if (assembly == null)
                return null;

            cacheAssembly.Add(asmName, assembly);
        }
        return assembly.GetType(fullTypeName);
    }
    public object? ReadField(object obj, string fieldName)
    {
        var field = obj?.GetType().GetField(fieldName, AllFlags);
        var val = field?.GetValue(obj);
        if (val == null)
        {
            Log($"error read field:{fieldName},obj:{obj} is null");
            return default;
        }
        return val;
    }
    public T ReadField<T>(object obj, string fieldName)
    {
        var val = ReadField(obj, fieldName);
        return (T)val;
    }
    public List<T>? ReadFieldList<T>(object obj, string fieldName)
    {
        var val = ReadField(obj, fieldName);
        if (val == null)
            return null;

        return (List<T>)val;
    }

    public void WriteField(object obj, string fieldName, object value)
    {
        var field = obj?.GetType().GetField(fieldName, AllFlags);
        field?.SetValue(obj, value);
    }
    public PropertyInfo GetProperty(Type type, string name)
    {
        return type.GetProperty(name, AllFlags);
    }

    static Harmony harmony => ModEntry.Instance.harmony;
    public void PatchPrefix(MethodInfo original, string prefixMethodName)
    {
        harmony.Patch(original, prefix: new(GetMethod(prefixMethodName)));
        Log("patched method prefix: " + original, LogLevel.Info);
    }

    public void PatchPostfix(MethodInfo original, string postfixMethodName)
    {
        harmony.Patch(original, postfix: new(GetMethod(postfixMethodName)));
        Log("patched method postfix: " + original, LogLevel.Info);
    }
    public void PatchPostfix(ConstructorInfo ctor, string postfixMethodName)
    {
        harmony.Patch(ctor, postfix: new(GetMethod(postfixMethodName)));
        Log("patched ctor postfix: " + ctor, LogLevel.Info);
    }


    public static void Log(string msg, LogLevel logLevel = LogLevel.Trace)
    {
        msg = $"[Patcher Tool] {msg}";
        Logger.Log(msg, logLevel);
    }
    public static void LogError(string msg) => Log(msg, LogLevel.Error);
    public static void LogWarn(string msg) => Log(msg, LogLevel.Warn);
    public static void Log(object obj)
    {
        if (obj == null)
            Log("obj null");
        else
            Log(obj);
    }
}
