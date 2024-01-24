using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public class PhpInterface: PhpBaseModel
{
    public PhpInterface(Settings settings, AssemblyType type) : base( settings, type ) { }
    
    protected override Task Model()
    {
        _model.Add("interface " + _type.Name);
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
        foreach (var property in _type.Fields)
        {
            if (property.Value._isStatic && property.Value._isConst)
            {
                _properties.Add("\t/**");
                _properties.Add("\t * @var \\" + PhpBaseTypes.Extract(property.Value.Type.ToString()));
                _properties.Add("\t */");
                _properties.Add("\t" + property.Value.Modifier + " const " + property.Key + " = null;");
            }
            else
            {
                var r = "";
                if (property.Value._isReadonly)
                {
                    r = "-read";
                }
                _commentsModel.Add($" * @property{r} \\" + PhpBaseTypes.Extract(property.Value.Type.ToString()) + " $" + property.Key);
            }
        }
        
        return Task.CompletedTask;
    }

    protected override void PhpBaseMethod(KeyValuePair<string, List<TypeMethod>> method)
    {
        if (method.Value.Count == 1)
        {
            PhpBodyMethod(method, method.Key);
        }
    }

    protected override void PhpBodyMethod(KeyValuePair<string, List<TypeMethod>> method, string methodKey)
    {
        _commentsMethods.Clear();   
        TypeMethod m0 = method.Value[0];
        string args = GetArgsMethod(m0.Args);
        string end = m0.IsAbstract ? ";" : "{}";
        _methods.Add("\t/**");
        if(_commentsMethods.Count > 0) 
            _methods.Add(string.Join("\n", _commentsMethods));
        _methods.Add("\t */");
        _methods.Add($"\t{m0.Modifier} function " + m0.Name + $"({args}){end}");
    }
    
    protected override void ScriptBuilder(StreamWriter phpFile)
    {
        PhpInterfaceScriptCompile();
        phpFile.WriteLine(GetScriptToString());
    }
    
    protected override string ScriptBuilder()
    {
        PhpInterfaceScriptCompile();
        return GetScriptToString();
    }
}