using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhpieSdk.Library;

public class PhpieLibrary
{
    public static List<string> Loaded = new();
    public static List<string> Failed = new();

    public static void MakeLog(string path)
    {
        File.WriteAllTextAsync(path, "");
        
        using StreamWriter log = new StreamWriter(path);
        foreach (var _ in Loaded.Distinct().ToArray())
        {
            log.WriteLine(_);
        }
        log.Close();
    }
}