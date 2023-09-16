using System.Collections.Generic;

namespace PhpieSdk.Library.Service;

public class TypeMethod
{
    public string Name { get; set; }
    public string ReturnType { get; set; }
    public string Modifier { get; set; }

    public int CountOverride { get; set; } = 0;
    
    public bool IsStatic { get; set; } = false;

    public bool IsAbstract { get; set; } = false;
    public List<PhpArgs> Args { get; set; }
    public string OriginalName { get; set; }
}