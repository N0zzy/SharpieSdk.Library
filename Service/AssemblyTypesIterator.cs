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
        var name = Assembly.GetName().Name;
        if (IsIgnore())
        {
            Console.WriteLine("[SDK] Library ignoring: " + name);
            return;
        }
        
        Type[] types = new Type[]{};
        try
        {
            types = Assembly.GetTypes();
        }
        catch (Exception)
        {
            PhpieLibrary.Failed.Add(name);
            "".WriteLn($" --- Error loading {name} --- ");
        }

        foreach (var type in types)
        {
            Type = type;
            TypeCreate();
            TypeClear();
        }
        "".WriteLn($"Loaded library: {name}");
        PhpieLibrary.Loaded.Add(name);
    }

    private bool IsIgnore()
    {
        return Settings.ListIgnore.Contains(Assembly.GetName().Name);
    }
}