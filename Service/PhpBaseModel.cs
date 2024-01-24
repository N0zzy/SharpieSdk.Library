using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public abstract class PhpBaseModel: PhpBaseParameters
{
    protected List<string> Script = new();

    private readonly string Descriptor = "<?php";
    protected readonly string SymbolOBrace = "{";
    protected readonly string SymbolСBrace = "}";
    protected readonly string WarningDeprecated = "@deprecated this element should not be used by you because it will break your program";

    private readonly BindingFlags BindingFlags
        = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
    
    protected abstract Task Model();
    
    protected abstract Task Methods();
    
    protected abstract Task Properties();

    protected abstract void ScriptBuilder(StreamWriter phpFile);
    protected abstract string ScriptBuilder();
    
    protected PhpBaseModel(Settings settings, AssemblyType type)
    {
        _settings = settings;
        _type = type;
        
        Properties();
        Methods();
        
        AddCommentsModel();
        Model();
    }
    
    public void Execute()
    {
        string filename = $"{_settings.outputScriptPath}/{_type.Name}.php";
        string content = String.Empty;
        content += Descriptor + "\n";
        content += AddNamespace();
        content += ScriptBuilder();
        
        var key =  filename.GetMD5();
        var value = content.GetMD5();
        if (PhpieLibrary.Files.TryGetValue(key, out var file))
        {
            if (file == value)
            {
                return;
            }
        }
        PhpieLibrary.Files[key] = value;
        
        using StreamWriter phpFile = new StreamWriter(filename);
        phpFile.Write(content);
        phpFile.Close();
    }

    private string AddNamespace()
    {
        if(string.IsNullOrEmpty( _type.Namespace )) return "";
        return $"namespace {_type.Namespace.ToReplaceDot("\\")};\n";
    }

    private Task AddCommentsModel()
    {
        _model.Add("/**");
        if(_type.OriginalName.IsPhpNameGeneric())
        {
            _model.Add($" * {WarningDeprecated}");
        }
        
        _model.Add(string.Join("\n", _commentsModel));
        _model.Add(" */");
        return Task.CompletedTask;
    }
    
    private string[] GetInterfaces(Array impls)
    {
        string[] interfaces = new string[impls.Length];
        int i = 0;
        foreach (var impl in impls)
        {
            interfaces.SetValue(impl.ToString()!.Replace('`', '_'), i);
            i++;
        }
        return interfaces.Distinct().ToArray();
    }

    protected string GetImpsInterfaces()
    {
       return _type.Implements.Length > 0 
            ? " implements \n\t" + string.Join(",\n\t", GetInterfaces(_type.Implements)) 
            : "";
    }

    private bool IsParentMethod(TypeMethod method)
    {
        MethodInfo baseMethod;
        MethodInfo derivedMethod;
        try
        {
             baseMethod = _type.BaseType.GetMethod(
                 method.Name, BindingFlags, null, new Type[] { typeof(int) }, null
                 );
             derivedMethod = _type.CurrentType.GetMethod(
                 method.Name, BindingFlags, null, new Type[] { typeof(int) }, null
                 );
        }
        catch
        {
            return false;
        }
        
        if (baseMethod == null || derivedMethod == null)
        {
            return false;
        }

        string modifierBaseMethod = baseMethod.IsPublic 
            ? "public " : (baseMethod.IsPrivate ? "private " : "");
        string modifierDerivedMethod = derivedMethod.IsPublic 
            ? "public " : (derivedMethod.IsPrivate ? "private " : "");

        if (modifierBaseMethod + baseMethod == modifierDerivedMethod + derivedMethod)
        {
            return true;
        }
        return false;
    }
    
    protected string GetArgsMethod(List<PhpArgs> mArgs)
    {
        List<string> args = new List<string>();
        foreach (var arg in mArgs)
        {
            string t = PhpBaseTypes.Extract(arg.Type);
            _commentsMethods.Add($"\t * @param \\{t} ${arg.Name}");
            args.Add($"${arg.Name}");
        }
        return string.Join(", ", args);
    }
    
    protected void PhpImplMethod(KeyValuePair<string, List<TypeMethod>> method)
    {
        PhpBodyMethod(method, method.Key.GetPhpImplName());
    }
    
    protected virtual void PhpBaseMethod(KeyValuePair<string, List<TypeMethod>> method)
    {
        PhpBodyMethod(method, method.Key);
    }

    protected virtual void PhpBodyMethod(KeyValuePair<string, List<TypeMethod>> method, string name)
    {
        int countMethods = method.Value.Count;
        string staticMethod = String.Empty;
        int i = 1;
        List<string> commentMethodsOverride = new List<string>();
        foreach (var m in method.Value)
        {
            if(m.Name.IsPhpNameError()) continue;
            staticMethod = m.IsStatic ? "static" : "";
            _commentsMethods.Clear();
            
            string _args = GetArgsMethod(m.Args);
            if (countMethods == 1)
            {
                if (_type.BaseType != null && IsParentMethod(m)){
                    continue;
                }
                PhpMethodSingleBuilder(m, staticMethod, _args, name);
            }
            else
            {
                _methodsTraitOverride.Add("\t/**");
                if (_commentsMethods.Count > 0)
                    _commentsMethods.Add(string.Join("\n", _commentsMethods));
                commentMethodsOverride.Add($"\t * @uses {_type.Name}MethodsOverride::{name}_{i} ({_args})");
                
                foreach (var _arg in m.Args)
                {
                    _methodsTraitOverride
                        .Add($"\t * @var {PhpBaseTypes.Extract(_arg.Type, true)} ${_arg.Name}");
                }
            
                if(m.Name != "__construct")
                {
                    string t = PhpBaseTypes.Extract(m.ReturnType);
                    _methodsTraitOverride.Add("\t * @return " +  PhpBaseTypes.Extract(t, true));
                }
                _methodsTraitOverride.Add("\t */");
                _methodsTraitOverride.Add($"\t#[MethodOverride] {m.Modifier} {staticMethod} function {name}_{i}({_args})" + "{}");
                i++;
            }
        }
        if (i > 1)
        {
            PhpMethodSingleOverrideBuilder(commentMethodsOverride, staticMethod, name);
        }
    }

    private void PhpMethodSingleBuilder(
        TypeMethod method,
        string staticMethod, 
        string args,
        string name
    )
    {
        _methods.Add("\t/**");
        if(_commentsMethods.Count > 0 ) 
            _methods.Add(string.Join("\n", _commentsMethods));
        if (method.Modifier == "private")
        {
            method.Modifier = "#[MethodPrivate]";
            _methods.Add("\t * " + WarningDeprecated);
            _methods.Add("\t * @return @deprecated" );
        }

        if (method.Name != "__construct")
        {
            _methods.Add("\t * @return " +  PhpBaseTypes.Extract(
                PhpBaseTypes.Extract(method.ReturnType), true
            ));
        }

        _methods.Add("\t */");
        _methods.Add($"\t{method.Modifier} {staticMethod} function {name}({args})" + "{}");
    }

    private void PhpMethodSingleOverrideBuilder(
        List<string> commentMethodsOverride, 
        string staticMethod, 
        string name
    )
    {
        _methods.Add("\t/**");
        if(commentMethodsOverride.Count > 0) 
            _methods.Add(string.Join("\n", commentMethodsOverride)); 
        _methods.Add("\t * @return mixed|@override");
        _methods.Add("\t */");
        _methods.Add($"\t#[MethodOverride] {staticMethod} function {name}(mixed ...$args)" + "{}");
    }

    protected void PhpBaseProperties(KeyValuePair<string, TypeVariables> prop)
    {
        if(prop.Key.Contains("<") || prop.Key.Contains(">") || prop.Value.Modifier.Contains("private")) return;
        
        _properties.Add("\t/**");
        
        string @readonly = String.Empty;
        string @static = prop.Value._isStatic ? "static " : "";
        string typeComment = PhpBaseTypes.Extract(prop.Value.Type.ToString(), true);
        string genericComment = String.Empty;
        
        if (prop.Value.Type.IsGenericType)
        {
            genericComment = "generic-type: " + typeComment.Remove(0,1) + "<br>";
            if (!typeComment.Contains("[]"))
            {
                typeComment = typeComment.Split("[")[0];
            }
        }
        
        if (!String.IsNullOrEmpty(genericComment))
        {
            _properties.Add($"\t * {genericComment}");
        }
        
        _properties.Add($"\t * @var {typeComment}");
        _properties.Add("\t * @" + prop.Value.Element);
        
        if (prop.Value._isReadonly)
        {
            _properties.Add("\t * @since readonly");
        }
        
        _properties.Add("\t */");
        _properties.Add($"\t{prop.Value.Modifier} {@readonly}{@static}${prop.Key};");
    }

    protected string GetScriptToString()
    {
        return string.Join("\n", Script);
    }

    protected void PhpClassScriptCompile()
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
        
        Script.Add(string.Join("\n", _model));
        Script.Add(SymbolOBrace);
        Script.Add(string.Join("\n", _properties));
        Script.Add(string.Join("\n", _methods));
        Script.Add(SymbolСBrace);
    }
    
    protected void PhpEnumScriptCompile()
    {
        Script.Add(string.Join("\n", _model));
        Script.Add(SymbolOBrace);
        Script.Add(string.Join("\n", _properties));
        Script.Add(SymbolСBrace);
    }
    
    protected void PhpInterfaceScriptCompile()
    {
        Script.Add(string.Join("\n", _model));
        Script.Add(SymbolOBrace);
        Script.Add(string.Join("\n", _properties));
        Script.Add(string.Join("\n", _methods));
        Script.Add(SymbolСBrace);
    }
}