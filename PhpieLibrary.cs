using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhpieSdk.Library;

public class PhpieLibrary
{
    public static List<string> Loaded = new();
    public static List<string> Failed = new();
    public static Dictionary<string, string> Files = new ();
    
    public static void MakeLibrariesLoadedLog(Settings settings)
    {
        if (settings.isMakeSdkList)
        {
            MakeLog(GetPathLoaded(
                settings.logLibsLoadedPath, 
                settings.rootPath,
                "sdklibs"
            ));
        }
    }
    
    public static void MakeFilesLoaded(Settings settings)
    {
        if (!settings.isMakeSdkFiles) return;
        
        string path = GetPathLoaded(
            settings.filesLoadedPath,
            settings.rootPath,
            "sdkfiles"
        );

        var stream = MakeLogFiles(path);
        SaveFilesLoaded(stream);
        stream.Close();
        Files.Clear();
    }

    public static void ReadAndDeleteFilesLoaded(Settings settings)
    {
        string path = GetPathLoaded(
            settings.filesLoadedPath,
            settings.rootPath,
            "sdkfiles"
        );
        
        if (!File.Exists(path: path))
        {
            return;
        }
        
        string[] lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var items = line.Split(" ");
            Files.Add(items[0], items[1]);
        }
        
        File.WriteAllText(path, String.Empty);
    }

    private static void MakeLog(string path)
    {
        File.WriteAllTextAsync(path, "");
        
        using StreamWriter log = new StreamWriter(path);
        foreach (var _ in Loaded.Distinct().ToArray())
        {
            log.WriteLine(_);
        }
        log.Close();
    }

    public static string GetPathLoaded(string path, string rootPath, string name)
    {
        return string.IsNullOrEmpty(path)
            ? rootPath + $"/.{name}"
            : path;
    }
    
    private static FileStream MakeLogFiles(string path)
    {
        if (!File.Exists(path))
        {
            if (path == null)
            {
                throw new FileNotFoundException("path is null");
            }
            return File.Create(path);
        }
        else
        {
            return File.OpenWrite(path);
        }
    }
    
    private static void SaveFilesLoaded(FileStream path)
    {
        using StreamWriter stream = new StreamWriter(path);
        foreach (var file in Files)
        {
            stream.WriteLine(file.Key + " " + file.Value);
        }
    }
}