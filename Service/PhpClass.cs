using System.IO;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public sealed class PhpClass: PhpBaseModel
{
    public PhpClass(Settings settings, AssemblyType type) : base(settings, type)
    {
    }

    protected override void Model()
    {
        string extends = string.IsNullOrEmpty(_type.Extends) 
            ? "" 
            : " extends " + _type.Extends.Replace("`", "_");
        string implements = GetImpsInterfaces();
        string final = _type.isFinal 
            ? "final " 
            : "";
        _model.Add($"{final}class {_type.Name}{extends}{implements}");
    }

    protected override void Methods()
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
    }

    protected override void Properties()
    {
        foreach (var v in _type.Fields)
        {
            PhpBaseProperties(v);
        }
        foreach (var v in _type.Properties)
        {
            PhpBaseProperties(v);
        }
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