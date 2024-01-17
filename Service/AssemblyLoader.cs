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
        string libName = GetLibraryExtension();
        string libPath = GetLibraryPath(settings);

        if (!settings.isViewLibsLoaded)
        {
            "".WriteLn("loading libraries from " + libPath);
        }
        
        foreach (var f in Directory.GetFiles(libPath, $"*.{libName}"))
        {
            if (File.Exists(f.ToReversSlash()))
            {
                Assembly.LoadFile(f.ToReversSlash());
            }
        }
    }
}