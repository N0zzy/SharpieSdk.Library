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
        if (IsIgnore())
        {
            Console.WriteLine("Ignored: " + Assembly.GetName().Name);
            return;
        }
        
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

    private bool IsIgnore()
    {
        return Settings.ListIgnore.Contains(Assembly.GetName().Name);
    }
}