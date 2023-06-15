using System.Collections.Generic;

namespace SharpieSdk.Library.Service;

struct TypeNames
{
    private static string PchpCore = "Pchp.Core.";
    private static string System = "System.";
    
    
    public static readonly Dictionary<string, string> Collection = new()
    {
        // System
        [$"{System}Void"] = "void",
        [$"{System}Int32"] = "int",
        [$"{System}Int64"] = "int",
        [$"{System}UInt32"] = "int",
        [$"{System}UInt32"] = "int",
        [$"{System}Boolean"] = "bool",
        [$"{System}Object"] = "object",
        
        // Peachpie
        [$"{PchpCore}PhpArray"] = "array",
        [$"{PchpCore}PhpValue"] = "mixed",
        [$"{PchpCore}PhpString"] = "string",
    };
}    