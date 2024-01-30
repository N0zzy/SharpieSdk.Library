using System;
using System.IO;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public abstract class PhpSdkProvider: PhpSdkProvider.ISettings
{
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
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
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
    
    private bool IsIgnoreAssemblyIterator(in Assembly assembly)
    {
        var isContinue = false;
        PhpSdkStorage.Assembly.Name = assembly.GetName().Name;
        foreach (var fileStream in assembly.GetFiles())
        {
            var key = fileStream.Name.ToReversSlash().GetMD5();
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
        PhpModel.Wrapper.Run();
    }
}