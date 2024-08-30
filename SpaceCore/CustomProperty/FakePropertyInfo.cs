using System.Globalization;
using System.Reflection;

namespace ImproveGame;

internal class FakePropertyInfo : PropertyInfo
{
    private CustomPropertyInfo customPropInfo;
    readonly Module _module;
    readonly Type _declaringType;
    public FakePropertyInfo(CustomPropertyInfo customProp)
    {
        this.customPropInfo = customProp;
        _declaringType = customPropInfo.DeclaringType;
        _module = _declaringType.Module;
    }

    //public override CustomAttributeData[] CustomAttributes => [];
    public override Module Module => customPropInfo.DeclaringType.Module;
    public override Type PropertyType => customPropInfo.PropertyType;
    public override PropertyAttributes Attributes => PropertyAttributes.None;
    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override string Name => customPropInfo.Name;
    public override Type DeclaringType => _declaringType;
    public override Type ReflectedType => _declaringType; // TODO: Will this work for subclasses? Like, GameLocation -> BuildableGameLocation -> Farm ?
    public override MethodInfo[] GetAccessors(bool nonPublic)
    {
        return
        [
            customPropInfo.Getter, customPropInfo.Setter,
            customPropInfo.Getter, customPropInfo.Setter,
        ];
    }
    public override object[] GetCustomAttributes(bool inherit)
    {
        return [];
    }
    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return [];
    }


    public override ParameterInfo[] GetIndexParameters()
    {
        return [];
    }

    public override MethodInfo GetGetMethod(bool nonPublic)
    {
        //Console.WriteLine("on GetGetMethod() prop: " + customPropInfo.Name);
        return customPropInfo.Getter;
    }
    public override MethodInfo GetSetMethod(bool nonPublic)
    {
        //Console.WriteLine("on GetSetMethod() prop: " + customPropInfo.Name);
        return customPropInfo.Setter;
    }


    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return false;
    }
    public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
    {
        //Console.WriteLine("on FakeProp.Getvalue()");
        return customPropInfo.Getter.Invoke(null, [obj]);
    }

    public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
    {
        //Console.WriteLine("on FakeProp.SetValue()");
        customPropInfo.Setter.Invoke(null, [obj, value]);
    }
}
