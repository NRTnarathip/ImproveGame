using HarmonyLib;
using System.Reflection;

namespace ImproveGame
{
    static class CustomPropertyInfoAPI
    {
        public static Type ThisType;
        public static PropertyInfo CustomPropertyInfo_PropertyType_FieldInfo;
        static PropertyInfo Getter_PropInfo;
        static PropertyInfo Setter_PropInfo;
        public static void Init()
        {
            ThisType = SpaceCoreAPI.TypeByName("SpaceCore.Framework.CustomPropertyInfo");
            CustomPropertyInfo_PropertyType_FieldInfo = ThisType.GetProperty("PropertyType", BasePatcher.AllFlags);
            Getter_PropInfo = AccessTools.Property(ThisType, "Getter");
            Setter_PropInfo = AccessTools.Property(ThisType, "Setter");
        }
        public static MethodInfo Get_Getter(object obj) => Getter_PropInfo.GetValue(obj, null) as MethodInfo;
        public static MethodInfo Get_Setter(object obj) => Setter_PropInfo.GetValue(obj, null) as MethodInfo;
        public static Type Get_PropertyType(object customPropertyInfoObject)
        {
            return CustomPropertyInfo_PropertyType_FieldInfo.GetValue(customPropertyInfoObject) as Type;
        }
    }
}

