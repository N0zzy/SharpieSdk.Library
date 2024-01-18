using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PhpieSdk.Library.Service;

public abstract class MembersFactory: ManagerFactory
{
    protected void AddFields()
    {
        int i = 0;
        TypeVariables tv = new TypeVariables();
        foreach (var field in Type.GetFields(BindingFlags))
        {
            if(
                Cache.Fields.ContainsKey(field.Name) || 
                IsEnumValue__(field.Name)
            ) continue;
            
            tv.Element = "field";
            tv.CurrentType = Type;
            tv.Type = field.FieldType;
            tv.Modifier = field.IsPublic ? "public" : (field.IsPrivate ? "private" : "protected");
            tv._isReadonly = field.IsInitOnly;
            tv._isStatic = field.IsStatic;
            tv.Number = i;
            
            Cache.Fields.Add(field.Name, tv);
            i++;
        }
    }

    protected void AddProperties()
    {
        int i = 0;
        TypeVariables tv = new TypeVariables();
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
            
            tv.Element = "property";
            tv.CurrentType = Type;
            tv.Type = property.PropertyType;
            tv.Modifier = modifier;
            tv._isReadonly = property.CanRead && !property.CanWrite;
            tv._isStatic = property.GetMethod != null && property.GetMethod.IsStatic;
            tv.Number = i;
            
            Cache.Properties.Add( property.Name, tv);
            i++;
        }
    }
    
    protected void AddConstructs()
    {
        TypeMethod tm = new TypeMethod();
        foreach (var constructor in Type.GetConstructors())
        {
            string methodName = "__construct";

            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add(methodName, new List<TypeMethod>());
            }
            
            tm.Name = methodName;
            tm.OriginalName = methodName;
            tm.Modifier = constructor.IsPublic ? "public" : (constructor.IsPrivate ? "private" : "protected");
            tm.IsAbstract = false;
            tm.IsStatic = false;
            tm.ReturnType = null;
            tm.Args = GetArgs(constructor);
            
            Cache.Methods[methodName].Add(tm);
        }
    }
    
    protected void AddMethods()
    {
        TypeMethod tm = new TypeMethod();
        foreach (var method in Type.GetMethods(BindingFlags))
        {
            string methodName = method.Name.IsPhpNameFoundDot() 
                ? method.Name.GetPhpImplName() : method.Name;
            
            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add(methodName, new List<TypeMethod>());
            }

            tm.Name = methodName;
            tm.OriginalName = method.Name;
            tm.ReturnType = method.ReturnType.Namespace + "." + method.ReturnType.Name;
            tm.Modifier = method.IsPublic ? "public" : (method.IsPrivate ? "private" : "protected");
            tm.IsAbstract = method.IsAbstract;
            tm.IsStatic = method.IsStatic;
            tm.Args = GetArgs(method);
            
            Cache.Methods[methodName].Add(tm);
        }
    }
    
    private List<PhpArgs> GetArgs(ConstructorInfo method)
    {
        return GetParameters(method.GetParameters());
    }
    
    private List<PhpArgs> GetArgs(MethodInfo method)
    {
        return GetParameters(method.GetParameters());
    }

    private List<PhpArgs> GetParameters(ParameterInfo[] parameterInfos)
    {
        List<PhpArgs> list = new List<PhpArgs>();
        if (parameterInfos.Length == 0)
        {
            return list;
        }
        
        PhpArgs pArgs = new PhpArgs();
        foreach (var vInfo in parameterInfos)
        {
            pArgs.Name = vInfo.Name;
            pArgs.Value = vInfo.DefaultValue ?? vInfo.RawDefaultValue;
            if (vInfo.ParameterType.GetProperty("Namespace") != null)
            {
                pArgs.Type = vInfo.ParameterType.Namespace + "." + vInfo.ParameterType.Name;
            }
            else
            {
                pArgs.Type = vInfo.ParameterType.Name;
            }
            list.Add(pArgs);
        }
        return list;
    }
}