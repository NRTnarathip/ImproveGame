using System.Reflection;

namespace ImproveGame
{
    static class CustomPropertyInfoAPI
    {
        public static Type CustomPropertyInfo_Type;
        public static PropertyInfo CustomPropertyInfo_PropertyType_FieldInfo;
        public static void Init()
        {
            CustomPropertyInfo_Type = SpaceCoreAPI.TypeByName("SpaceCore.Framework.CustomPropertyInfo");
            CustomPropertyInfo_PropertyType_FieldInfo = CustomPropertyInfo_Type.GetProperty("PropertyType", BasePatcher.AllFlags);
        }
        public static Type Get_PropertyType(object customPropertyInfoObject)
        {
            return CustomPropertyInfo_PropertyType_FieldInfo.GetValue(customPropertyInfoObject) as Type;
        }
    }
}

