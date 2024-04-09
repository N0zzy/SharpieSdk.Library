using System.Collections.Generic;

namespace PhpieSdk.Library;

public class PhpSdkSettings
{
    /// <summary>
    /// Название директории свойства OutputPath
    /// </summary>
    public string SdkName { get; set; } = ".sdk";
    /// <summary>
    /// sdkfiles
    /// </summary>
    public string SdkFilesName { get; set; } = ".sdkfiles";
    /// <summary>
    /// </summary>
    public string CurrentPath { get; set; } = null;
    /// <summary>
    /// </summary>
    public string LibsPath { get; set; } = null;
    /// <summary>
    /// Директория с исходниками
    /// </summary>
    public string RootPath { get; set; } = null;
    /// <summary>
    /// Директория с выгрузкой php-скриптов
    /// </summary>
    public string OutputPath { get; init; } = null;
    /// <summary>
    /// Флаг установки регистра символов в имени методов
    /// </summary>
    public bool IsUppercaseNames { get; init; } = false;
    /// <summary>
    ///
    /// </summary>
    public bool IsViewMessageAboutLoaded { get; set; } = false;
    public HashSet<string> LibrariesListLoaded { get; set; }
    public List<string> IgnoreList{ get; init; }
    public List<object> PreloadList { get; init; } = new ();
}