using System.Collections.Generic;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public abstract class MembersFactory: ManagerFactory
{
    protected void AddFields()
    {
        int i = 0;
        foreach (var field in Type.GetFields(BindingFlags))
        {
            if(
                Cache.Fields.ContainsKey(field.Name) || 
                IsEnumValue__(field.Name)
            ) continue;
            
            TypeVariables tv = new TypeVariables
            {
                Element = "field",
                CurrentType = Type,
                Type = field.FieldType,
                Modifier = field.IsPublic ? "public" : (field.IsPrivate ? "private" : "protected"),
                _isReadonly = field.IsInitOnly,
                _isConst = field.IsLiteral,
                _isStatic = field.IsStatic,
                Number = i
            };

            Cache.Fields.Add(field.Name, tv);
            i++;
        }
    }

    protected void AddProperties()
    {
        int i = 0;
        
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
            
            TypeVariables tv = new()
            {
                Element = "property",
                CurrentType = Type,
                Type = property.PropertyType,
                Modifier = modifier,
                _isReadonly = property.CanRead && !property.CanWrite,
                _isStatic = property.GetMethod != null && property.GetMethod.IsStatic,
                _isConst = false,
                Number = i
            };

            Cache.Properties.Add(property.Name, tv);
            i++;
        }
    }
    
    protected void AddConstructs()
    {
        string methodName = "__construct";
        
        foreach (var constructor in Type.GetConstructors())
        {
            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add(methodName, new List<TypeMethod>());
            }
            TypeMethod tm = new()
            {
                Name = methodName,
                OriginalName = methodName,
                Modifier = constructor.IsPublic ? "public" : (constructor.IsPrivate ? "private" : "protected"),
                IsAbstract = false,
                IsStatic = false,
                ReturnType = null,
                Args = GetArgs(constructor)
            };
            Cache.Methods[methodName].Add(tm);
        }
        
    }
    
    protected void AddMethods()
    {
        foreach (var method in Type.GetMethods(BindingFlags))
        {
            string methodName = method.Name.IsPhpNameFoundDot() 
                ? method.Name.GetPhpImplName() : method.Name;
            TypeMethod tm = new()
            {
                Name = methodName,
                OriginalName = method.Name,
                ReturnType = method.ReturnType.Namespace + "." + method.ReturnType.Name,
                Modifier = method.IsPublic ? "public" : (method.IsPrivate ? "private" : "protected"),
                IsAbstract = method.IsAbstract,
                IsStatic = method.IsStatic,
                Args = GetArgs(method)
            };
            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add( methodName, new List<TypeMethod>());
            }
            Cache.Methods[methodName].Add(tm);
        }
    }

    private List<PhpArgs> GetArgs(ConstructorInfo method)
    {
        return GetParameters(method.GetParameters(), true);
    }
    
    private List<PhpArgs> GetArgs(MethodInfo method)
    {
        return GetParameters(method.GetParameters());
    }

    private List<PhpArgs> GetParameters(ParameterInfo[] parameterInfos, bool isConstructor = false)
    {
        List<PhpArgs> list = new List<PhpArgs>();
        if (parameterInfos.Length == 0)
        {
            return list;
        }
        foreach (var vInfo in parameterInfos)
        {
            PhpArgs pArgs = new()
            {
                Name = vInfo.Name,
                Value = vInfo.DefaultValue ?? vInfo.RawDefaultValue,
                Type = vInfo.ParameterType.Namespace + "." + vInfo.ParameterType.Name
            };
            list.Add(pArgs);
        }
        return list;
    }
}