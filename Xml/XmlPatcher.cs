using HarmonyLib;
using System.Globalization;
using System.Reflection;

namespace ImproveGame;
internal class XmlPatcher : BasePatcher
{
    public static void Init()
    {
        new XmlPatcher();
    }
    public XmlPatcher()
    {
        var harmony = ModEntry.Instance.harmony;
        harmony.Patch(
            original: AccessTools.PropertyGetter(
                AccessTools.TypeByName("System.Xml.Serialization.TypeData"),
                "ListItemType"),
            prefix: new HarmonyMethod(GetMethod(nameof(Prefix_Fixed_ListItemType)))
       );
    }
    //src https://github.com/ZaneYork/SMAPI
    static bool Prefix_Fixed_ListItemType(object __instance, ref Type __result)
    {
        var instanceType = __instance.GetType();
        string name = (string)AccessTools.Property(instanceType, "CSharpFullName").GetValue(__instance);
        bool isRewrite = name == "StardewValley.Network.NetIntDictionary<System.Int32,Netcode.NetInt>" ||
                         name == "StardewValley.Network.NetStringDictionary<System.String,Netcode.NetString>";

        if (!isRewrite) return true;

        Type runtimeType = (Type)AccessTools.Field(instanceType, "type").GetValue(__instance);
        if (runtimeType == null) throw new InvalidOperationException("Property ListItemType is not supported for custom types");
        FieldInfo listItemTypeField = AccessTools.Field(instanceType, "listItemType");
        Type listItemType = (Type)listItemTypeField.GetValue(__instance);
        if (listItemType != null)
        {
            __result = listItemType;
            return false;
        }

        Type type = null;
        if (runtimeType.IsArray)
        {
            listItemType = runtimeType.GetElementType();
            listItemTypeField.SetValue(__instance, listItemType);
        }
        else if (typeof(ICollection<object>).IsAssignableFrom(runtimeType))
        {
            if (typeof(IDictionary<object, object>).IsAssignableFrom(runtimeType))
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The type {0} is not supported because it implements IDictionary.", runtimeType.FullName));

            PropertyInfo indexerProperty = (PropertyInfo)AccessTools.Method(instanceType, "GetIndexerProperty", new[] { typeof(Type) }).Invoke(null, new[] { runtimeType });
            if (indexerProperty == null)
                throw new InvalidOperationException("You must implement a default accessor on " + runtimeType.FullName + " because it inherits from ICollection");

            listItemType = indexerProperty.PropertyType;
            listItemTypeField.SetValue(__instance, listItemType);

            if (runtimeType.GetMethod("Add", [listItemType]) == null)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "To be XML serializable, types which inherit from {0} must have an implementation of Add({1}) at all levels of their inheritance hierarchy. {2} does not implement Add({1}).",
                    "ICollection", listItemType.FullName, type.FullName));
        }
        else
        {
            MethodInfo methodInfo = runtimeType.GetMethod("GetEnumerator", Type.EmptyTypes);
            if (methodInfo == null)
                methodInfo = runtimeType.GetMethod("System.Collections.IEnumerable.GetEnumerator",
                    BindingFlags.Instance | BindingFlags.Public
                    | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

            PropertyInfo property = methodInfo.ReturnType.GetProperty("Current");
            if (property == null)
                listItemType = typeof(object);
            else
                listItemType = property.PropertyType;

            listItemTypeField.SetValue(__instance, listItemType);
            if (runtimeType.GetMethod("Add", [listItemType]) == null)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "To be XML serializable, types which inherit from {0} must have an implementation of Add({1}) at all levels of their inheritance hierarchy. {2} does not implement Add({1}).",
                    "IEnumerable", listItemType.FullName, type.FullName));
        }

        __result = listItemType;
        return false;
    }


}
