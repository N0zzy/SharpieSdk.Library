using System.Collections.Generic;
using System.IO;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public class PhpInterface: PhpBaseModel
{
    public PhpInterface(Settings settings, AssemblyType type) : base( settings, type ) { }
    
    protected override void Model()
    {
        _model.Add("interface " + _type.Name);
    }

    protected override void Methods()
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
        _commentsMethods = new();   
        TypeMethod m0 = method.Value[0];
        string _args = GetArgsMethod(m0.Args);
        string _end = m0.IsAbstract ? ";" : "{}";
        _methods.Add("\t/**");
        if(_commentsMethods.Count > 0) 
            _methods.Add(string.Join("\n", _commentsMethods));
        _methods.Add("\t */");
        _methods.Add($"\t{m0.Modifier} function " + m0.Name + $"({_args}){_end}");
    }

    protected override void Properties()
    {

    }

    protected override void ScriptBuilder(StreamWriter phpFile)
    {
        Script.Add(string.Join("\n", _model));
        Script.Add(SymbolOBrace);
        Script.Add(string.Join("\n", _methods));
        Script.Add(SymbolСBrace);
        phpFile.WriteLine(string.Join("\n", Script));
    }
}