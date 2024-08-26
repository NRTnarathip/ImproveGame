using HarmonyLib;
using System.Reflection;

namespace ImproveGame;

public class BasePatcher
{
    static BindingFlags MethodFlags = BindingFlags.Static | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

    Type thisType;
    public BasePatcher()
    {
        thisType = GetType();
    }

    public MethodInfo GetMethod(string name) => GetMethod(thisType, name);
    public MethodInfo GetMethod(Type type, string name, string[] paramTypeNames)
    {
        var methods = type.GetMethods(MethodFlags).Where(m => m.Name == name).ToArray();
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
        return null;
    }
    public MethodInfo GetMethod(Type type, string name) => type.GetMethod(name, MethodFlags);
    static Harmony harmony => ModEntry.Instance.harmony;
    public void PatchPrefix(MethodInfo original, string prefixMethodName)
    {
        harmony.Patch(original, prefix: new(GetMethod(prefixMethodName)));
        ModEntry.Log("patched prefix: " + original);
    }

    public void PatchPostfix(MethodInfo original, string postfixMethodName)
    {
        harmony.Patch(original, postfix: new(GetMethod(postfixMethodName)));
        ModEntry.Log("patched postfix: " + original);
    }
}
