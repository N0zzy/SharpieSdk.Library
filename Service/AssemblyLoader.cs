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

    private string GetLibraryPath(Settings settings)
    {
        string libsPath = String.Empty;
        if (String.IsNullOrEmpty(settings.libsPath))
        {
            string b = !String.IsNullOrEmpty(settings.targetBuild)
                ? $"/bin/{settings.targetBuild}"
                : "";
            string f = !String.IsNullOrEmpty(settings.targetFramework)
                ? $"/{settings.targetFramework}"
                : "";
            libsPath = settings.rootPath + $"{b}{f}";
        }
        return Directory.Exists(libsPath) ? libsPath : settings.rootPath;
    }
    
    private bool IsWindows()
    {
        return Environment.OSVersion.ToString().Contains("Windows");
    }
    
    public void Run(Settings settings)
    {
        var libName = GetLibraryExtension();
        var libPath = GetLibraryPath(settings);
        
        "".WriteLn("loading libraries from " + libPath);
        
        foreach (var f in Directory.GetFiles(libPath, $"*.{libName}"))
        {
            try
            {
                Assembly.LoadFile(f.ToReversSlash());
            }
            catch (Exception)
            {
                //ignore
            }
        }
        "".WriteLn("libraries loaded...");
    }
}