using System;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public class AssemblyTypesIterator: AssemblyTypesIteratorFactory
{
    public AssemblyTypesIterator(Settings settings)
    {
        Settings = settings;
    }
    
    public AssemblyTypesIterator SetAssembly(Assembly assembly)
    {
        Assembly = assembly;
        return this;
    }
    
    public void Run()
    {
        AssemblyName = Assembly.GetName().Name;
        if (IsIgnore())
        {
            "".WriteLn("library ignoring: " + AssemblyName);
            return;
        }
        foreach (var type in ExtractTypes())
        {
            Type = type;
            TypeCreate();
            TypeClear();
        }
        "".WriteLn($"loaded library: {AssemblyName}");
        PhpieLibrary.Loaded.Add(AssemblyName);
    }

    private bool IsIgnore()
    {
        return Settings.ListIgnore.Contains(AssemblyName);
    }
    
    private Type[] ExtractTypes()
    {
        Type[] types = new Type[]{};
        try
        {
            types = Assembly.GetTypes();
        }
        catch (Exception)
        {
            PhpieLibrary.Failed.Add(AssemblyName);
            "".WriteLn($" --- error loading {AssemblyName} --- ");
        }
        return types;
    }
}