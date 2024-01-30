using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PhpieSdk.Library.Service;

public class PhpModel
{
    public readonly PhpWrapper Wrapper = new();
    
    public string Get(Type type)
    {
        if (type.IsClass || type is { IsValueType: true, IsEnum: false })
        {
            return "class";
        }
        else if (type.IsInterface)
        {
            return "interface";
        }
        else if (type.IsEnum)
        {
            return "enum";
        }
        else
        {
            return null;
        }
    }

    public string GetExtends(Type type)
    {
        return type.BaseType == null
            ? ""
            : "\\" + (type.BaseType.Namespace + "." + type.BaseType.Name).ToReplaceDot("\\");
    }

    public string[] GetImplements(Type type)
    {
        return type.GetInterfaces().Select(i => ("\\" + i.Namespace + "." + i.Name).ToReplaceDot("\\")).ToArray();
    }
    
    public bool IsErrorName(string name)
    {
        return !Regex.IsMatch(name, "^[a-z0-9\\.`_]+$", RegexOptions.IgnoreCase);    
    }

    public string GetNamespace(Type type)
    {
        return type.Namespace?.ToString() ?? "";
    }
}