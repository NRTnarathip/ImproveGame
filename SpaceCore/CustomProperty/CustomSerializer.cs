using System.Reflection;

namespace ImproveGame;

static class CustomSerializer
{
    public static Dictionary<Type, Dictionary<string, CustomPropertyInfo>> CustomProperties = new();
    public static void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter)
    {
        if (!CustomProperties.ContainsKey(declaringType))
            CustomProperties.Add(declaringType, new());

        CustomProperties[declaringType].Add(name, new CustomPropertyInfo()
        {
            DeclaringType = declaringType,
            Name = name,
            PropertyType = propType,
            Getter = getter,
            Setter = setter,
        });
    }
}
