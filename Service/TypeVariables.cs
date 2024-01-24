using System;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public class TypeVariables
{
    public string Element { get; set; }
    public Type Type { get; set; }
    public Type CurrentType { get; set; }
    public string Modifier { get; set; }
    public int Number { get; set; }
    
    public bool _isStatic { get; set; } = false;
    public bool _isReadonly { get; set; } = false;
    public bool _isConst { get; set; } = false;
    public string Value { get; set; } = null;
}