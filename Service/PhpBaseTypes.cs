using System.Collections.Generic;
using Pchp.Core;

namespace PhpieSdk.Library.Service;

public struct PhpBaseTypes
{
    private static Dictionary<string, string> Types = new Dictionary<string, string>()
    {
        ["system.void"] = "void",
        ["system.int32"] = "int",
        ["system.int64"] = "int",
        ["system.string"] = "string",
        ["system.object"] = "object",
        ["system.double"] = "double",
        ["system.float"] = "float",
        ["system.array"] = "array",
    };

    public static string Convert(string type)
    {
        var _type = type.ToLower();
        return Types.ContainsKey(_type) ? $"{type}|{Types[_type]}" : type;
    }
}