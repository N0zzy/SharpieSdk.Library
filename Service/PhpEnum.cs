using System;
using System.IO;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public class PhpEnum: PhpBaseModel
{
    public PhpEnum(Settings settings, AssemblyType type) : base(settings, type) {}

    protected override void Model()
    {
        _model.Add($"enum {_type.Name}{GetImpsInterfaces()}");
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
            catch
            {
                _properties.Add($"\tcase {field.Key};");
            } 
        }
    }

    protected override void ScriptBuilder(StreamWriter phpFile)
    {
        PhpEnumScriptCompile();
        phpFile.WriteLine(GetScriptToString());
    }
    
    protected override string ScriptBuilder()
    {
        PhpEnumScriptCompile();
        return GetScriptToString();
    }
}