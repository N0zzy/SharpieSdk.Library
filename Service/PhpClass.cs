using System.IO;
using System.Threading.Tasks;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public sealed class PhpClass: PhpBaseModel
{
    public PhpClass(Settings settings, AssemblyType type) : base(settings, type)
    {
    }

    protected override Task Model()
    {
        string extends = string.IsNullOrEmpty(_type.Extends) 
            ? "" 
            : " extends " + _type.Extends.Replace("`", "_");
        string implements = GetImpsInterfaces();
        string final = _type.isFinal 
            ? "final " 
            : "";
        _model.Add($"{final}class {_type.Name}{extends}{implements}");
        return Task.CompletedTask;
    }

    protected override Task Methods()
    {
        foreach (var method in _type.Methods)
        {
            string methodName = method.Key;

            if(methodName.IsPhpNameError()) continue;
            if (methodName.IsPhpNameFoundDot())
            {
                PhpImplMethod(method);
            }
            else
            {
                PhpBaseMethod(method);
            }
        }
        return Task.CompletedTask;
    }

    protected override Task Properties()
    {
        foreach (var v in _type.Fields)
        {
            PhpBaseProperties(v);
        }
        foreach (var v in _type.Properties)
        {
            PhpBaseProperties(v);
        }
        return Task.CompletedTask;
    }

    protected override void ScriptBuilder(StreamWriter phpFile)
    {
        PhpClassScriptCompile();
        phpFile.WriteLine(GetScriptToString());
    }
    
    protected override string ScriptBuilder()
    {
        PhpClassScriptCompile();
        return GetScriptToString();
    }
}