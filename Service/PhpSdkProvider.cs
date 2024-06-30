using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Xml.Linq;

namespace PhpieSdk.Library.Service;

public abstract class PhpSdkProvider: PhpSdkProvider.ISettings
{
    protected List<object> LoaderList = new List<object>()
    {
        typeof(System.Uri),
    };
    
    public abstract PhpSdkSettings Settings { get; init; }
    
    private readonly PhpModel PhpModel = new();
    private string sdkPath = String.Empty;
    
    public interface ISettings
    {
        public PhpSdkSettings Settings { get ; protected init; }
    }
    
    protected void AssemblyLoader()
    {
        var loader = new AssemblyLoader()
        {
            CurrentPath = Settings.CurrentPath,
            IsViewMessageAboutLoaded = Settings.IsViewMessageAboutLoaded,
            LibrariesListLoaded = Settings.LibrariesListLoaded
        };
        loader.Run();
        Settings.LibrariesListLoaded = loader.LibrariesListLoaded;
    }
    
    protected void AssemblyIterator()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (IsIgnoreAssemblyIterator(assembly))
            {
                continue;
            }
            
            AssemblyTypesIterator(assembly);
            
            if (Settings.IsViewMessageAboutLoaded)
            {
                PhpSdkStorage.Assembly.Name.WriteLn("assembly loaded:");
            }
            PhpSdkStorage.Assembly.Clear();
        }
    }

    private void AssemblyTypesIterator(Assembly assembly)
    {
        foreach (var type in ExtractTypes(assembly))
        {
            TypeCreate(type);
        }
    }
    
    private bool IsIgnoreAssemblyIterator(Assembly assembly)
    {
        bool isContinue = false;
        PhpSdkStorage.Assembly.Name = assembly.GetName().Name;
        foreach (FileStream fileStream in assembly.GetFiles())
        {
            string key = fileStream.Name.ToReversSlash().GetMD5();
            if (PhpSdkStorage.Files.ContainsKey(key))
            {
                if (PhpSdkStorage.Files[key][0] == PhpSdkStorage.Files[key][1])
                {
                    isContinue = true;
                }
            }
        }
        return isContinue || Settings.IgnoreList.Contains(PhpSdkStorage.Assembly.Name);
    }
    
    private void TypeCreate(Type type)
    {
        if(PhpModel.IsErrorName(type.Name)) return;
        
        PhpSdkStorage.Type.Instance = type;
        PhpSdkStorage.Type.Name = type.Name;
        PhpSdkStorage.Type.Title = type.Name.Replace("`", "_");
        PhpSdkStorage.Type.Namespace = type.Namespace;
        PhpSdkStorage.Type.FullName = type.FullName;
        PhpSdkStorage.Type.Model.Name = PhpModel.Get(type);
        PhpSdkStorage.Type.Model.Namespace = PhpModel.GetNamespace(type);
        PhpSdkStorage.Type.Model.Extends = PhpModel.GetExtends(type);
        PhpSdkStorage.Type.Model.Implements = PhpModel.GetImplements(type);
        
        PhpXmlCommentsWrapper(type);
        
        MakeDirectory();

        PhpSdkStorage.Type.Model.Path = sdkPath;
        
        PhpWrapper();
        
        PhpSdkStorage.Type.Clear();
    }
    
    private Type[] ExtractTypes(Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch
        {
            types = new Type[]{};
        }
        return types;
    }
    
    private void MakeDirectory()
    {
        string @namespace = string.IsNullOrEmpty(PhpSdkStorage.Type.Namespace)
            ? ".hidden" : PhpSdkStorage.Type.Namespace.ToReplaceDot("/");
        
        sdkPath = Settings.OutputPath + Settings.SdkName+ "/" + @namespace + "/";
        
        if (!Directory.Exists(sdkPath))
        {
            Directory.CreateDirectory(sdkPath);
        }
    }

    private void PhpWrapper()
    {
        PhpModel.Wrapper.ModelName = PhpSdkStorage.Type.Model.Name;
        PhpModel.Wrapper.IsUppercase = Settings.IsUppercaseNames;
        PhpModel.Wrapper.Run();
    }
    
    private void PhpXmlCommentsWrapper(Type type)
    {
        PhpSdkStorage.Assembly.FrameworkVersion = GetVersionFramework( type.Assembly );
        PhpSdkStorage.Assembly.Version = GetAssemblyInformationalVersionAttribute( type.Assembly );
        
        var xmpPath = GetXmlPath();
        if (xmpPath != PhpSdkStorage.Assembly.XmlPath)
        {
            PhpSdkStorage.Assembly.XmlPath = xmpPath;
            PhpSdkStorage.Assembly.Xml = GetXml();
        }
    }
    
    private string GetVersionFramework(Assembly assembly)
    {
        Dictionary<string, string> frameworks = new Dictionary<string, string>()
        {
            {".NETCoreApp,Version=v5.0", "net5.0"},
            {".NETCoreApp,Version=v6.0", "net6.0"},
            {".NETCoreApp,Version=v7.0", "net7.0"},
            {".NETCoreApp,Version=v8.0", "net8.0"},
            {".NETStandard,Version=v1.0", "netstandard1.0"},
            {".NETStandard,Version=v1.3", "netstandard1.3"},
            {".NETStandard,Version=v2.0", "netstandard2.0"},
            {".NETStandard,Version=v2.1", "netstandard2.1"},
            {".NETStandard,Version=v3.1", "netstandard3.1"}
        };
        var name = string.Empty;
        var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
        if( targetFrameworkAttribute == null ) return string.Empty;
        if (frameworks.TryGetValue(targetFrameworkAttribute!.FrameworkName, out string value))
        {
            name = value;
        }
        return name;
    }

    private string GetAssemblyInformationalVersionAttribute(Assembly assembly)
    {
        var version = string.Empty;
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute != null)
        {
            version = attribute.InformationalVersion.Split("+")[0].Trim();
        }
        return version;
    }
    
    private string GetXmlPath()
    {
        var s = PhpSdkStorage.Type.Instance.Assembly.GetName().Name;
        if (s != null)
        {
            var name = s.ToLower();
            return Environment.ExpandEnvironmentVariables(
                "%userprofile%\\.nuget\\packages\\" +
                name.ToLower() + "\\" +
                PhpSdkStorage.Assembly.Version + "\\lib\\" +
                PhpSdkStorage.Assembly.FrameworkVersion + "\\" +
                name + ".xml"
            ).ToReversSlash();
        }
        return null;
    }
    
    private XDocument GetXml()
    {
        return File.Exists(PhpSdkStorage.Assembly.XmlPath)
            ? XDocument.Load(PhpSdkStorage.Assembly.XmlPath)
            : null;
    }
}