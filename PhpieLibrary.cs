using System.Collections.Generic;
using System.IO;

namespace PhpieSdk.Library;

public class PhpieLibrary
{
    public static List<string> Loaded = new();
    public static List<string> Failed = new();

    public static void MakeLog(string path)
    {
        using StreamWriter log = new StreamWriter(path);
        foreach (var _ in Loaded)
        {
            log.WriteLine(_);
        }
        log.Close();
    }
}