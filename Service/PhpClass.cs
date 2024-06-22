using System;
using System.Linq;

namespace PhpieSdk.Library.Service;

public class PhpClass: PhpFactory
{
    private bool isStruct = false;
    
    protected override void Comments()
    {

    }

    protected override void Models()
    {
        string impls = GetImpsInterfaces();
        var modifier = GetModifierAbstract();
        modifier = string.IsNullOrEmpty(modifier) ? GetModifierFinal() : "";
        Header = modifier + PhpSdkStorage.Type.Model.Name + " " + PhpSdkStorage.Type.Name;
        Header += $"{GetExtends()}{impls}";
    }

    protected override void Properties()
    {
        var groupedFields = 
            PhpSdkStorage.Type.Instance.GetFields(Flags).GroupBy(f => f.Name);
        foreach (var group in groupedFields)
        {
            var fields = group.ToArray();
            if (fields.Length > 1)
            {
                FieldsCompileGroup(fields);
            }
            else
            {
                FieldsCompile(fields);
            }
        }

        var groupedProps = 
            PhpSdkStorage.Type.Instance.GetProperties(Flags).GroupBy(f => f.Name);
        foreach (var group in groupedProps)
        {
            var properties = group.ToArray();
            if (properties.Length > 1)
            {
                PropertiesCompileGroup(properties);
            }
            else
            {
                PropertiesCompile(properties);
            }
        }
    }

    protected override void Methods()
    {
        var groupedMethods = 
            PhpSdkStorage.Type.Instance.GetMethods(Flags).GroupBy(f => f.Name.Split(".").Last());
        foreach (var group in groupedMethods)
        {
            var methods = group.ToArray();
            if (methods.Length > 1)
            {
                SetNameOverride();
                MethodsCompileGroup(group.Key, methods);
            }
            else {
                MethodsCompile(methods);
            }
        }

        CtorCompile(PhpSdkStorage.Type.Instance.GetConstructors());
    }

    protected override void Events()
    {
        var groupedEvents = PhpSdkStorage.Type.Instance.GetEvents().ToList();
        if ( PhpSdkStorage.Type.Name != "Program")
        {
            return;
        }
        EventsCompile(groupedEvents.ToArray());
    }
    
    private string GetExtends()
    {
        string extends = string.IsNullOrEmpty(PhpSdkStorage.Type.Model.Extends) 
            ? "" 
            : " extends " + PhpSdkStorage.Type.Model.Extends.Replace("`", "_");
        return extends;
    }
    
    private string GetModifierFinal()
    {
        return isStruct || PhpSdkStorage.Type.Instance.IsSealed ? "final " : "";
    }
    
    private string GetModifierAbstract()
    {
        return 
            PhpSdkStorage.Type.Instance.IsAbstract && 
            PhpSdkStorage.Type.Model.Name == "class" 
            ? "abstract " : "";
    }
}