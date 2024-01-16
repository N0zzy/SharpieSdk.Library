using System;

namespace PhpieSdk.Library.Service;

public class AssemblyIterator
{
    private readonly Settings _settings;
    
    public AssemblyIterator(Settings settings)
    {
        _settings = settings;
    }

    public void Run()
    {
        AssemblyTypesIterator assemblyTypesIterator = new AssemblyTypesIterator(_settings);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            assemblyTypesIterator.SetAssembly(assembly).Run();
        }
    }
}
