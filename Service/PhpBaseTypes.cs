using System.Collections.Generic;

namespace PhpieSdk.Library.Service;

public struct PhpBaseTypes
{
    private static Dictionary<string, string> Types = new Dictionary<string, string>()
    {
        ["t"] = "object",
        ["system.void"] = "void",
        ["system.int32"] = "int",
        ["system.int64"] = "int",
        ["system.string"] = "string",
        ["system.object"] = "object",
        ["system.double"] = "double",
        ["system.float"] = "float",
        ["system.boolean"] = "bool",
        ["system.array"] = "array",
        ["system.int32[]"] = "array",
        ["system.int64[]"] = "array",
        ["system.string[]"] = "array",
        ["system.object[]"] = "array",
        ["system.double[]"] = "array",
        ["system.float[]"] = "array",
        ["system.boolean[]"] = "array",
        ["system.int32[][]"] = "array",
        ["system.int64[][]"] = "array",
        ["system.string[][]"] = "array",
        ["system.object[][]"] = "array",
        ["system.double[][]"] = "array",
        ["system.float[][]"] = "array",
        ["system.boolean[][]"] = "array",
    };

    private static string Convert(string type)
    {
        string _type = type.ToLower();
        return Types.ContainsKey(_type) ? $"{type}|{Types[_type]}" : type;
    }

    public static string Extract(string argType, bool isSeparator = false)
    {
        return (isSeparator ? "\\" : "") + Convert(argType)
            .ToReplaceDot("\\")
            .Replace("`", "_");
    }
}