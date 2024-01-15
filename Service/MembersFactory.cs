using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public abstract class MembersFactory: ManagerFactory
{
    protected void AddFields()
    {
        var i = 0;
        foreach (var field in Type.GetFields(BindingFlags))
        {
            if(
                Cache.Fields.ContainsKey(field.Name) || 
                IsEnumValue__(field.Name)
            ) continue;
            Cache.Fields.Add(field.Name, new TypeVariables()
            {
                Element = "field",
                CurrentType = Type,
                Type = field.FieldType,
                Modifier = field.IsPublic ? "public" : (field.IsPrivate ? "private" : "protected"),
                _isReadonly = field.IsInitOnly,
                _isStatic = field.IsStatic,
                Number = i
            });
            i++;
        }
    }

    protected void AddProperties()
    {
        var i = 0;
        foreach (var property in Type.GetProperties(BindingFlags))
        {
            if(
                Cache.Properties.ContainsKey(property.Name) || 
                IsEnumValue__(property.Name)
            ) continue;

            string modifier;

            if (property.GetMethod != null && property.GetMethod.IsPublic)
                modifier = "public";
            else if (property.SetMethod != null && property.SetMethod.IsFamily)
                modifier = "protected";
            else 
                modifier = "private";
            
            Cache.Properties.Add( property.Name,new TypeVariables() {
                Element = "property",
                CurrentType = Type,
                Type = property.PropertyType,
                Modifier = modifier,
                _isReadonly = property.CanRead && !property.CanWrite,
                Number = i
            });
            i++;
        }
    }
    
    
    protected void AddConstructs()
    {
        foreach (var constructor in Type.GetConstructors())
        {
            var methodName = "__construct";

            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add(methodName, new List<TypeMethod>());
            }
            
            Cache.Methods[methodName].Add(new TypeMethod() {
                Name = methodName,
                OriginalName = methodName,
                Modifier = constructor.IsPublic ? "public" : (constructor.IsPrivate ? "private" : "protected"),
                IsAbstract = false,
                IsStatic = false,
                ReturnType = null,
                Args = GetArgs(constructor)
            });
        }
    }
    


    protected void AddMethods()
    {
        foreach (var method in Type.GetMethods(BindingFlags))
        {
            var methodName = method.Name.IsPhpNameFoundDot() 
                ? method.Name.GetPhpImplName() : method.Name;
            
            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add(methodName, new List<TypeMethod>());
            }
            
            Cache.Methods[methodName].Add(new TypeMethod()
            {
                Name = methodName,
                OriginalName = method.Name,
                ReturnType =  method.ReturnType.Namespace + "." + method.ReturnType.Name,
                Modifier = method.IsPublic ? "public" : (method.IsPrivate ? "private" : "protected"),
                IsAbstract = method.IsAbstract,
                IsStatic = method.IsStatic,
                Args = GetArgs(method)
            });
        }
    }
    
    private List<PhpArgs> GetArgs(ConstructorInfo method)
    {
        return method.GetParameters().Select(GetParameters).ToList();
    }
    
    private List<PhpArgs> GetArgs(MethodInfo method)
    {
        return method.GetParameters().Select(GetParameters).ToList();
    }

    private PhpArgs GetParameters(ParameterInfo parameter)
    {
        var phpArgs = new PhpArgs
        {
            Name = parameter.Name,
            Type = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name,
            Value = parameter.DefaultValue ?? parameter.RawDefaultValue
        };
        return phpArgs;
    }
}