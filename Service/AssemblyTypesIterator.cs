using System;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public class AssemblyTypesIterator: AssemblyTypesIteratorFactory
{
    public AssemblyTypesIterator(Assembly assembly, Settings settings)
    {
        Assembly = assembly;
        Settings = settings;
    }
    
    public void Run()
    {
        Type[] types = new Type[]{};
        try
        {
            types = Assembly.GetTypes();
        }
        catch (Exception)
        {
            "".WriteLn($" --- Error loading {Assembly.GetName().Name} --- ");
        }

        foreach (var type in types)
        {
            Type = type;
            TypeCreate();
            TypeClear();
        }
        "".WriteLn($"Loaded library: {Assembly.GetName().Name}");
    }
}