using System;
using System.IO;
using System.Threading.Tasks;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public class PhpEnum: PhpBaseModel
{
    public PhpEnum(Settings settings, AssemblyType type) : base(settings, type) {}

    protected override Task Model()
    {
        _model.Add($"enum {_type.Name}{GetImpsInterfaces()}");
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
        return Task.CompletedTask;
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