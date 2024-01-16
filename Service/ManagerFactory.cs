using System;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public abstract class ManagerFactory
{
    protected Settings Settings { get; set; }
    protected Assembly Assembly { get; set; }
    protected string AssemblyName { get; set; }
    protected Type Type { get; set; }
    protected string Name => Type.Name.ToOriginalName().Replace("`", "_");
    
    protected AssemblyType Cache = null;
    protected int ModelCount { get; set; } = 0;
    protected BindingFlags BindingFlags { get; set; } 
        = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

    protected bool IsEnumValue__(string name)
    {
        return (Cache.Model == "enum" && name == "value__");
    }
}