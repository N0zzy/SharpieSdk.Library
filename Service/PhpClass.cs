﻿using System.Linq;

namespace PhpieSdk.Library.Service;

public class PhpClass: PhpFactory
{
    private bool isStruct = false;
    private string Xml = null;
    
    protected override void Comments()
    {
        //Xml = GetXml( PhpSdkStorage.Type.Instance.Assembly );
    }

    protected override void Models()
    {
        string impls = GetImpsInterfaces();
        var modifier = GetModifierAbstract();
        modifier = string.IsNullOrEmpty(modifier) ? GetModifierFinal() : "";
        Header = "/**\n";
        Header += " * " + Xml + "\n";
        Header += " */\n";
        Header += modifier + PhpSdkStorage.Type.Model.Name + " " + PhpSdkStorage.Type.Name;
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
        EventsCompile(PhpSdkStorage.Type.Instance.GetEvents());
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