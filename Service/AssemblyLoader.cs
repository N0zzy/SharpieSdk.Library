using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace PhpieSdk.Library.Service;

public class AssemblyLoader
{
    public string CurrentPath { get; set; }
    public bool IsViewMessageAboutLoaded { get; set; }
    public HashSet<string> LibrariesListLoaded { get; set; }
    
    private string GetLibraryExtension()
    {
        return IsWindows() ? "dll" : "so";
    }

    private bool IsWindows()
    {
        return Environment.OSVersion.ToString().Contains("Windows");
    }
    
    private HashSet<string> GetDynamicLibs(string libPath)
    {
        var dlls = new HashSet<string>();
        var assems = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assem in assems)
        {
            foreach (var module in assem.GetModules())
            {
                dlls.Add(libPath + module);
            }
        }
        return dlls;
    }

    public void Run(string libsPath = null)
    {
        string libName = GetLibraryExtension();
        string libPath = libsPath ?? CurrentPath;

        if (IsViewMessageAboutLoaded)
        {
            "".WriteLn("loading libraries from " + libPath);
        }
        
        HashSet<string> libs = GetDynamicLibs(libPath);

        LibrariesListLoaded = new HashSet<string>();
        
        foreach (var f in libs)
        {
            var frs = f.ToReversSlash();
            if (File.Exists(frs))
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(frs);
                LibrariesListLoaded.Add(frs);
                
                if (IsViewMessageAboutLoaded)
                {
                    frs.WriteLn("library loaded:");
                }
            }
        }
        
        
        foreach (var f in Directory.GetFiles(libPath, $"*.{libName}"))
        {
            var frs = f.ToReversSlash();
            if (File.Exists(frs))
            {
                Assembly assembly = Assembly.LoadFrom(frs);
                LibrariesListLoaded.Add(frs);
            }
        }
        
        Assembly.Load("System.Private.CoreLib");
        
        SaveHashSummary();
    }

    private void SaveHashSummary()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var fileStream in assembly.GetFiles())
            {
                var path = fileStream.Name.ToReversSlash();
                var key = path.GetMD5();
                if (!PhpSdkStorage.Files.ContainsKey(key))
                {
                    PhpSdkStorage.Files.Add(key, new List<string>());
                    PhpSdkStorage.Files[key].Add(null);
                }
                PhpSdkStorage.Files[key].Add(File.ReadAllBytes(path).GetMD5());
            }
        }
    }
}
