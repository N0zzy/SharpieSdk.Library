using System;
using System.IO;
using System.Reflection;

namespace PhpieSdk.Library.Service;

struct AssemblyLoader
{
    private string GetLibraryExtension()
    {
        return IsWindows() ? "dll" : "so";
    }
    
    private bool IsWindows()
    {
        return Environment.OSVersion.ToString().Contains("Windows");
    }
    
    public void Run(Settings settings)
    {
        "".WriteLn("Loading libraries from " + settings.currentPath);
        var libName = GetLibraryExtension();
        foreach (var f in Directory.GetFiles(settings.currentPath, $"*.{libName}"))
        {
            try
            {
                Assembly.LoadFile(f);
            }
            catch (Exception)
            {
                //ignore
            }
        }
        "".WriteLn("Libraries loaded...");
    }
}