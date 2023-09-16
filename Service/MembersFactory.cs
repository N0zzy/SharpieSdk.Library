using System;
using System.Collections.Generic;
using System.Linq;

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
                IsEnumFieldValue__(field.Name)
            ) continue;
            Cache.Fields.Add(field.Name, new TypeFields()
            {
                CurrentType = Type,
                Type = field.FieldType,
                Modifier = field.IsPublic ? "public" : (field.IsPrivate ? "private" : "protected"),
                Number = i
            });
            i++;
        }
    }

    protected void AddProperties()
    {
        // foreach (var property in Type.GetProperties(BindingFlags))
        // {
        //     if (!Cache.Properties.ContainsKey(property.Name))
        //     {
        //         Cache.Properties.Add(property.Name, new List<TypeProperties>());
        //     }
        //     
        //     Cache.Properties[property.Name].Add(new TypeProperties() {
        //         
        //     });
        // }
    }

    protected void AddMethods()
    {
        foreach (var method in Type.GetMethods(BindingFlags))
        {
            var methodName = method.Name.IsPhpNameFoundDot() ? method.Name.GetPhpImplName() : method.Name;
            
            if (!Cache.Methods.ContainsKey(methodName))
            {
                Cache.Methods.Add(methodName, new List<TypeMethod>());
            }
            
            Cache.Methods[methodName].Add(new TypeMethod()
            {
                Name = methodName,//текущее имя метода после проверки на оригинальность
                OriginalName = method.Name,//может не совпадать из за имплементации
                ReturnType =  method.ReturnType.Namespace + "." + method.ReturnType.Name,
                Modifier = method.IsPublic ? "public" : (method.IsPrivate ? "private" : "protected"),
                IsAbstract = method.IsAbstract,
                IsStatic = method.IsStatic,
                Args = method.GetParameters().Select((p ) =>
                {
                    try
                    {
                        return new PhpArgs()
                        {
                            Name = p.Name,
                            Type = p.ParameterType.Namespace + "." + p.ParameterType.Name ,
                            Value = p.DefaultValue
                        };
                    }
                    catch (Exception)
                    {
                        return new PhpArgs()
                        {
                            Name = p.Name,
                            Type = p.ParameterType.Namespace + "." + p.ParameterType.Name,
                            Value = p.RawDefaultValue
                        };
                    }

                }).ToList(),
            });
        }
    }
}