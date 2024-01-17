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
            if (!Settings.isViewLibsIgnore)
            {
                "".WriteLn("library ignoring: " + AssemblyName);
            }
            return;
        }
        foreach (var type in ExtractTypes())
        {
            Type = type;
            TypeCreate();
            TypeClear();
        }

        if (!Settings.isViewLibsLoaded)
        {
            "".WriteLn($"loaded library: {AssemblyName}");
        }
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
        catch 
        {
            PhpieLibrary.Failed.Add(AssemblyName);
            if (!Settings.isViewLibsLoaded)
            {
                "".WriteLn($" --- error loading {AssemblyName} --- ");
            }
        }
        return types;
    }
}