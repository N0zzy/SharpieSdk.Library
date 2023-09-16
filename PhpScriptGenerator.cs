using System;
using System.IO;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public sealed class PhpScriptGenerator
{
    private readonly Settings _settings;
    
    public PhpScriptGenerator(Settings settings)
    {
        _settings = settings;
        SetRootPath();
        SetOutputPath();
        SetListIgrone();
    }
    
    public void Execute()
    {
        new AssemblyLoader().Run(_settings);
        new AssemblyIterator(_settings).Run();
    }

    private void SetRootPath()
    {
        if (_settings.rootPath == null)
        {
            _settings.rootPath = _settings
                .currentPath
                .ToReversSlash()
                .Replace($"/bin/{_settings.targetBuild}/{_settings.targetFramework}", "");
        }
        
        "".WriteLn("root path: " + _settings.rootPath);
    }
    
    private void SetOutputPath()
    {
        var path = _settings.sdkPath == ".sdkpath" 
            ? _settings.rootPath + "/" + _settings.sdkPath
            : _settings.sdkPath;

        FileNotFoundException(path);

        _settings.outputPath = File.ReadAllText(path).Trim().ToReversSlash();

        DirectoryNotFoundException();
        
        "".WriteLn("output path: " + _settings.outputPath);
    }
    
    private void SetListIgrone()
    {
        var path = _settings.sdkIgnore == ".sdkignore" 
            ? _settings.rootPath + "/" + _settings.sdkIgnore
            : _settings.sdkIgnore;
        
        FileNotFoundException(path);
        
        Console.WriteLine(path);
        
        string s;
        StreamReader f = new StreamReader(path);
        while ((s = f.ReadLine()) != null)
        {
            _settings.ListIgnore.Add(s.Trim());
        }
        f.Close();
    }

    private void FileNotFoundException(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }
    }
    
    private void DirectoryNotFoundException()
    {
        if (!Directory.Exists(_settings.outputPath))
        {
            throw new DirectoryNotFoundException(_settings.outputPath);
        }
    }
}
