using System;
using System.IO;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public class PhpEnum: PhpBaseModel
{
    public PhpEnum(Settings settings, AssemblyType type) : base(settings, type) {}

    protected override void Model()
    {
        var implements = GetImpsInterfaces();
        _model.Add($"enum {_type.Name}{implements}");
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
        foreach (var field in _type.Fields)
        {
            try
            {
                Type enumUnderlyingType = Enum.GetUnderlyingType(field.Value.CurrentType);
                var n = field.Value.Number;
                Array enumValues = Enum.GetValues(field.Value.CurrentType);
                object value = enumValues.GetValue(n);
                var v = Convert.ChangeType(value, enumUnderlyingType);
                _properties.Add($"\tcase {field.Key} = {v};");
            }
            catch(Exception)
            {
                _properties.Add($"\tcase {field.Key};");
            }
        }
    }

    protected override void ScriptBuilder(StreamWriter phpFile)
    {
        foreach (var _ in _model) { Script.Add(_); }
        Script.Add(SymbolOBrace);
        foreach (var _ in _properties) { Script.Add(_); }
        Script.Add(SymbolСBrace);
        phpFile.WriteLine(string.Join("\n", Script));
    }
}