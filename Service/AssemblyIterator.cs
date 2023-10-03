﻿using System;

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
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            AssemblyTypesIterator iterator = new AssemblyTypesIterator(assembly, _settings);
            iterator.Run();
        }
    }
}