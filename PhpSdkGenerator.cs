using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public sealed class PhpSdkGenerator: PhpSdkProvider, PhpSdkProvider.ISettings
{
    public PhpSdkGenerator(PhpSdkSettings settings)
    {
        Settings = settings;
        Initialize();
    }

    public void Execute()
    {
        ReadAndDeleteFilesLoaded();
        
        ColdLoader();
        
        AssemblyLoader();
        AssemblyIterator();
        
        SaveFilesLoaded();
    }
    
    private void Initialize()
    {
        PhpSdkStorage.Type.EventType = Settings.EventType;
        SetPaths();
    }
    
    private void SetPaths()
    {
        var path = AppDomain.CurrentDomain.BaseDirectory.ToReversSlash();
        SetSettingCurrentPath(path);
        SetSettingRootPath(path);
    }
    
    private void SetSettingCurrentPath(string path)
    {
        if(string.IsNullOrEmpty(Settings.CurrentPath)) 
            Settings.CurrentPath = path;
    }
    
    private void SetSettingRootPath(string path)
    {
        if(string.IsNullOrEmpty(Settings.RootPath)) 
            Settings.RootPath = path.Split("bin")[0];
    }
    
    private void ReadAndDeleteFilesLoaded()
    {
        string path = GetPathLoaded(Settings.SdkFilesName);
    
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var line in File.ReadAllLines(path))
        {
            string[] items = line.Split(" ");
            
            if(items.Length != 2) continue;
            
            if (!PhpSdkStorage.Files.ContainsKey(items[0]))
            {
                PhpSdkStorage.Files.Add(items[0], new List<string>());
            }
            PhpSdkStorage.Files[items[0]].Add(items[1]);
        }
    
        File.WriteAllText(path, String.Empty);
    }

    private void SaveFilesLoaded()
    {
        var path = GetPathLoaded(Settings.SdkFilesName);
        
        if (!Settings.IsCached)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return;
        }
        
        using StreamWriter stream = new StreamWriter(path);
        foreach (var file in PhpSdkStorage.Files)
        {
            if(file.Value.Count != 2) continue;
            stream.WriteLine(file.Key + " " + file.Value[1]);
        }
        stream.Close();
    }
    
    private string GetPathLoaded(string name)
    {
        return Settings.RootPath + $"{name}";
    }

    private void ColdLoader()
    {
        List<object> listObjects = LoaderList.Concat(Settings.PreloadList).Distinct().ToList();
        foreach (var o in listObjects)
        {
            var s = o.ToString();
        }
        Settings.PreloadList.Clear();
    }
    
    public override PhpSdkSettings Settings { get; init; }
}


