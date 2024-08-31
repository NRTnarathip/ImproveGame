using HarmonyLib;
using System.Collections;
using System.Reflection;

namespace ImproveGame;

internal static class SpaceCoreCustomPropertyAPI
{
    public static Dictionary<Type, Dictionary<string, CustomPropertyInfo>> CustomPropertyMap = new();
    public static void Init()
    {
        var customProps = (IDictionary)SpaceCoreAPI.ReadField_CustomProperties();
        Console.WriteLine("found spaceCore custom props: " + customProps.Count);
        foreach (DictionaryEntry propKvp in customProps)
        {
            var delcaringType = propKvp.Key as Type;
            var customPropMap = (IDictionary)propKvp.Value;
            foreach (DictionaryEntry customPropKvp in customPropMap)
            {
                var fieldName = customPropKvp.Key as string;
                var prop = customPropKvp.Value;
                RegisterCustomProperty(
                    delcaringType,
                    fieldName,
                    CustomPropertyInfoAPI.Get_PropertyType(prop),
                    CustomPropertyInfoAPI.Get_Getter(prop),
                    CustomPropertyInfoAPI.Get_Setter(prop)
                );
            }
        }

        var harmony = ModEntry.Instance.harmony;
        harmony.Patch(
            original: AccessTools.Method(SpaceCoreAPI.TypeByName("SpaceCore.Api"), "RegisterCustomProperty"),
            prefix: new(AccessTools.Method(typeof(SpaceCoreCustomPropertyAPI), nameof(Prefix_RegisterCustomProp)))
        );
    }
    static void Prefix_RegisterCustomProp(Type declaringType, string name,
        Type propType, MethodInfo getter, MethodInfo setter)
    {
        RegisterCustomProperty(declaringType, name, propType, getter, setter);
    }
    public static void RemoveClassMapType(Type type)
    {
        if (CustomPropertyMap.ContainsKey(type))
        {
            CustomPropertyMap.Remove(type);
            Console.WriteLine("remove all CustomProp for class type: " + type);
        }
    }
    public static void RegisterCustomProperty(Type declaringType, string name, Type propertyType, MethodInfo getter, MethodInfo setter)
    {
        if (!CustomPropertyMap.ContainsKey(declaringType))
            CustomPropertyMap.Add(declaringType, new());

        CustomPropertyMap[declaringType].Add(name, new CustomPropertyInfo()
        {
            DeclaringType = declaringType,
            Name = name,
            PropertyType = propertyType,
            Getter = getter,
            Setter = setter,
        });
        Logger.Log($"register custom prop; class:{declaringType},name:{name},propType:{propertyType}");
    }
}
