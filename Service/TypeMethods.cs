using System;
using System.Collections.Generic;

namespace PhpieSdk.Library.Service;

public class TypeMethod
{

    public string Name { get; set; } = String.Empty;
    public string ReturnType { get; set; } = String.Empty;
    public string Modifier { get; set; }= String.Empty;

    public int CountOverride { get; set; } = 0;
    
    public bool IsStatic { get; set; } = false;

    public bool IsAbstract { get; set; } = false;
    public List<PhpArgs> Args { get; set; } = null;
    public string OriginalName { get; set; } = null;
}