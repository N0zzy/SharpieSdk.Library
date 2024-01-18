using System;
using System.Collections.Generic;

namespace PhpieSdk.Library;

public class Settings
{
    public string targetFramework { get; set; } = String.Empty;
    public string sdkPath { get; set; } = String.Empty;
    public string sdkIgnore { get; set; } = String.Empty;
    public string sdkName { get; set; } = String.Empty;
    public string currentPath { get; set; } = String.Empty;
    public string targetBuild { get; set; } = String.Empty;
    public string rootPath { get; set; } = String.Empty;
    public string libsPath { get; set; } = String.Empty;
    public string outputPath { get; set; } = null;
    public string outputScriptPath { get; set; } = String.Empty;
    
    public List<string> ListIgnore = new List<string>();
    /// <summary>
    /// Make sdk list
    /// Default: true - make, false - don't make
    /// </summary>
    public bool isMakeSdkList { get; set; } = false;
    /// <summary>
    /// Make a cache of SDK files to speed up when restarting the generator.
    /// Reduces files generation time by 2-3 times.
    /// Warning: It is recommended to enable this option only if you will be in control of cleaning the [.sdkfiles]
    /// Default: true - make, false - don't make
    /// </summary>
    public bool isMakeSdkFiles { get; set; } = false;
    public bool isViewLibsIgnore { get; set; } = false;
    public bool isViewOutputPath { get; set; } = false;
    public bool isViewLibsLoaded { get; set; } = false;
    public string logLibsLoadedPath { get; set; } = null;
    public string filesLoadedPath { get; set; } = null;
    
}