using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Pchp.Core;


namespace PchpSdkLibrary.Service;

public class ExtractMembers
{
    public Type Type { get; set; }
    
    private readonly BindingFlags _flags = BindingFlags.Instance | 
                                           BindingFlags.Public | 
                                           BindingFlags.Static |
                                           BindingFlags.NonPublic;

    public  List<PhpMemberProperty> ExtractFields()
    {
        List<PhpMemberProperty> fields = new List<PhpMemberProperty>();
        
        foreach (var prop in Type.GetFields(_flags))
        {
            if (prop.IsPrivate) continue;
            
            fields.Add(new PhpMemberProperty()
            {
                Name = prop.Name,
                TypeName = prop.FieldType.Name,
                TypeNamespace = prop.FieldType.Namespace,
                Dollar = "$",
                Member = "field",
                ModifierBase = prop.IsPublic ? "public" : "protected",
                ModifierStatic = prop!.IsStatic ? "static " : "",
                IsLiteral = prop.IsLiteral,
                IsInitOnly = prop.IsInitOnly,
                IsNameError = prop.Name.IndexOf('<') != -1 
            });
        }

        return fields;
    }
    
    public  List<PhpMemberProperty> ExtractProperties()
    {
        List<PhpMemberProperty> properties = new List<PhpMemberProperty>();
        
        foreach (var prop in Type.GetProperties(_flags))
        {
            var accessors = prop.GetAccessors();
            
            string modifierBase = null;
            string modifierStatic = null;
            List<string> methods = new();
            Boolean isAdd = true;

            foreach (var accessor in accessors)
            {
                if (accessor.IsPrivate)
                {
                    isAdd = false; continue;
                }
                isAdd = true;
                modifierBase = accessor.IsPublic ? "public" : "protected";
                modifierStatic = accessor.IsStatic ? "static" : "";
                methods.Add("\t * @method " + accessor.Name + "()");
            }

            if (!isAdd) return null;

            properties.Add(new PhpMemberProperty()
            {
                Name = prop.Name,
                Dollar = "$",
                Member = "property",
                TypeName = prop.PropertyType.Name,
                TypeNamespace = prop.PropertyType.Namespace,
                ModifierBase = modifierBase,
                ModifierStatic = modifierStatic,
                Methods = methods,
                IsLiteral = false,
                IsInitOnly = false
            });
        }

        return properties;
    }
    
    public  List<PhpMemberMethod> ExtractMethods()
    {
        List<PhpMemberMethod> methods = new List<PhpMemberMethod>();
        
        foreach (var method in Type.GetMethods(_flags))
        {
            Dictionary<string, string> args = new();
            List<PhpReturnType> returnTypes = new();
            
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.Name == null) continue;
                args.Add(
                    parameter.Name,
                    JsonConvert.SerializeObject(new TypeArgument()
                    {
                        Namespace = parameter.ParameterType.Namespace,
                        Name = parameter.ParameterType.Name,
                        Value = null
                    })
                );
            }
            returnTypes = GetReturnTypes(method.ReturnType);
            methods.Add(new PhpMemberMethod()
            {
                Name = method.Name,
                ReturnType = returnTypes,
                Arguments = args
            });
        }
        return methods;
    }

    private List<PhpReturnType> GetReturnTypes(Type t)
    {
        List<PhpReturnType> RTypes = new();
        var phpValue = PhpValue.FromClr(t);
        
        if (phpValue.IsPhpNumber())
        {
            RTypes.Add(new PhpReturnType()
            {
                Name = "int"
            });
            RTypes.Add(new PhpReturnType()
            {
                Name = "float"
            });
        }
        if (phpValue.IsPhpValue())
        {
            RTypes.Add(new PhpReturnType()
            {
                Name = "mixed"
            });
        }
        else if (phpValue.IsObject)
        {
            RTypes.Add(new PhpReturnType()
            {
                Name = t.Name,
                Namespace = t.Namespace
            });
            RTypes.Add(new PhpReturnType()
            {
                Name = "object",
            });
        }
        return RTypes;
    }
}