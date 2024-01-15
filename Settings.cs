using System;
using System.Collections.Generic;

namespace PhpieSdk.Library;

public class Settings
{
    public string targetFramework { get; set; }
    public string sdkPath { get; set; }
    public string sdkIgnore { get; set; }
    public string sdkName { get; set; }
    public string currentPath { get; set; }
    public string targetBuild { get; set; } = String.Empty;
    public string rootPath { get; set; } = String.Empty;
    public string libsPath { get; set; } = String.Empty;
    public string outputPath { get; set; } = null;
    public string outputScriptPath { get; set; }
    
    public List<string> ListIgnore = new List<string>();
    /// <summary>
    /// Make sdk list
    /// Default: true - make, false - don't make
    /// </summary>
    public bool isMakeSdkList { get; set; } = false;
    
    public string logLibsLoadedPath { get; set; } = null;
}