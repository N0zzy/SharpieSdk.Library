using System;
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
        var extends = string.IsNullOrEmpty(_type.Extends) 
            ? "" 
            : " extends " + _type.Extends.Replace("`", "_");
        var implements = GetImpsInterfaces();
        var final = _type.isFinal 
            ? "final " 
            : "";
        _model.Add($"{final}class {_type.Name}{extends}{implements}");
    }

    protected override void Methods()
    {
        foreach (var method in _type.Methods)
        {
            var methodName = method.Key;
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
        if (_methodsTraitOverride.Count > 0)
        {
            Script.Add("/**");
            Script.Add(" * " + WarningDeprecated);
            Script.Add(" */");
            Script.Add($"trait {_type.Name}MethodsOverride");
            Script.Add(SymbolOBrace);
            Script.Add(string.Join("\n", _methodsTraitOverride));
            Script.Add(SymbolСBrace);
        }
        foreach (var _ in _model) { Script.Add(_); }
        Script.Add(SymbolOBrace);
        foreach (var _ in _properties) { Script.Add(_); }
        foreach (var _ in _methods) { Script.Add(_); }
        Script.Add(SymbolСBrace);
        phpFile.WriteLine(string.Join("\n", Script));
    }
}