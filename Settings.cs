namespace PhpieSdk.Library;

public class Settings
{
    public string targetFramework { get; set; }
    public string sdkPath { get; set; }
    public string sdkIgnore { get; set; }
    public string sdkName { get; set; }
    public string currentPath { get; set; }
    public string targetBuild { get; set; }
    public string rootPath { get; set; } = null;
    public string outputPath { get; set; } = null;
    public string outputScriptPath { get; set; }
}