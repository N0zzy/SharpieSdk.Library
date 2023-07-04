using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharpieSdk.Library.Service;

public abstract class Components
{
    public readonly string Descriptor = "<?php";
    public string _pathRoot = "";
    public string _pathCustom = "";
    public readonly string _pathSdk = "/.sdk";

    public string Dirs { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Element { get; set; }
    public string Extends { get; set; }
    public Type Type { get; set; }
    
    public String Abstract = string.Empty;
    public String Implements = string.Empty;
    
    //string - key => name element
    public Dictionary<string, Dictionary<int, PhpMemberMethod>> _methods = new();
    public Dictionary<string, Dictionary<int, PhpMemberProperty>> _props = new();

    public List<string> scriptBase = new();
    public List<string> scriptUses = new();
    public List<string> scriptComments = new();
    public List<string> scriptElement = new();
    public List<string> scriptMembers = new();
    public List<string> scriptOverrideMethods = new();
    public List<string> scriptDotTraits = new();

    protected ExtractMembers _members = new();
    
    protected void AddPhpDescriptor()
    {
        scriptBase.Add(Descriptor);
    }
    protected void AddPhpNamespace()
    {
        var ns = ToPhpNamespace();
        if(ns != null)
            scriptBase.Add($"namespace {ns};");
    }
    protected void AddPhpUses()
    {
        
    }
    protected void AddPhpElements()
    {
        var __abstract = IsClass() ? Abstract : "";
        var __impls = IsClass() ? Implements : "";
        var __name = Name;
        if (!string.IsNullOrEmpty(__name))
        {
            scriptElement.Add("/**");
            if (Name.IndexOf('`') != -1)
            {
                scriptElement.Add("* @deprecated don`t use this your code will be broken");
                __name = __name.Replace("`", "_");
            }
            scriptElement.Add("*/");
        }
        scriptElement.Add($"{__abstract}{Element} {__name} {__impls}" + "{");
    }    
    
    protected void AddPhpProperties()
    {
        try
        {
            foreach (var props in _props)
            {
                if (IsEnum())
                {
                    ToPhpEnumProperties(props);
                    continue;
                }
                else if (IsInterface())
                {
                    continue;
                }
                ToPhpClassProperties(props);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }

    }
    
    protected void AddPhpMethods()
    {
        try
        {
            if (IsInterface())
            {
                PackPhpInterfaceMethods();
                return;
            }
            if (IsEnum())
            {
                PackPhpInterfaceMethods();
                return;
            }
            PackPhpClassMethods();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    protected void PackPhpInterfaceMethods()
    {
        scriptDotTraits.Add($"{Name}Methods");
        scriptDotTraits.Add($"trait {Name}Methods" + " {");
        
        foreach (var methods in _methods)
        {
            var name = methods.Key;
            foreach (var method in methods.Value.Values)
            {
                ToInterfaceMethods(name, method);
            }
        }
        scriptDotTraits.Add("}");
    }
    protected void PackPhpEnumMethods()
    {
        
    }
    protected void PackPhpClassMethods()
    {
        scriptOverrideMethods.Add("\ntrait OverrideMethods {");
        foreach (var methods in _methods)
        {
            var name = methods.Key;
            if (methods.Value.Count > 1)
            {
                ToPhpOverrideMethod(name, methods);
                continue;
            }
            ToPhpOriginalMethod(name, methods);
        }
        scriptOverrideMethods.Add("}");
    }
    
    protected string ToPhpNamespace()
    {
        return Namespace?.ToString().ToReplaceDot("\\");
    }
    protected void ToPhpEnumProperties(
        KeyValuePair<string, Dictionary<int, PhpMemberProperty>> props
    )
    {
        foreach (var prop in props.Value.Values)
        {
            if (prop.IsLiteral && !prop.IsInitOnly)
            {
                scriptMembers.Add("\tcase " + prop.Name + ";");
            }
        }
    }
    protected void ToPhpClassProperties(
        KeyValuePair<string, Dictionary<int, PhpMemberProperty>> props
    )
    {
        foreach (var prop in props.Value.Values)
        {
            var name = prop.Name;
            var member = "";
            var type = "";
            var isErrorName = false;
            string _public = null;
            string _static = null;
            List<string> typeList = new List<string>();
            List<string> methodsList = new List<string>();
            
            if (string.IsNullOrEmpty(prop.ModifierBase))
            {
                continue;
            }
            
            member = prop.Member;
            _public = prop.ModifierBase + " ";
            _static = string.IsNullOrEmpty(prop.ModifierStatic) ? "" : prop.ModifierStatic + " ";
            PushScriptUses(prop.TypeNamespace + "." + prop.TypeName);
            
            typeList.Add(prop.TypeName.ToOriginalName());
            if (prop.Methods.Count > 0)  methodsList.Add(string.Join("\n", prop.Methods));
            isErrorName = prop.IsNameError;
            
            type = typeList.Count <= 0 ? "" : string.Join("|", typeList).ToReplaceDot("\\"); 
            scriptMembers.Add("\t/**");
            scriptMembers.Add("\t * @" + member);
            if (methodsList.Count > 0)  scriptMembers.Add(string.Join("\n", methodsList));
            scriptMembers.Add("\t * @var " + type);
            scriptMembers.Add("\t */");
            var close = isErrorName ? "//" : "";
            scriptMembers.Add($"\t{close}{_public}{_static}${name};");
        }
    }
    protected void ToInterfaceMethods(string name, PhpMemberMethod method)
    {
        var argsList = GetListArguments(method);
        var typeList = GetListTypes(method);
        var end = method.IsAbstract ? ";" : "{}";
        var type = typeList.Count < 1 ? "Void|void" : string.Join("|", typeList);
        var args = string.Join(", ", argsList.Args);
        var mod = method.IsAbstract ? $"#[Override('{method.Modifier.Trim()}')]" : method.Modifier;
        foreach (var s in new string[] {
             "\t/**",
             string.Join("\n", argsList.Params),
             $"\t * @return {type.ToOriginalName().Replace('`', '_')}",
             "\t */",
             $"\t{mod}{method.Static}function {method.Name}({args}){end}",
        }) {
            if (string.IsNullOrEmpty(s)) continue;
            if (method.IsAbstract)
            {
                scriptMembers.Add(s);
            }
            else
            {
                scriptDotTraits.Add(s);
            }
        }
    }
    protected void ToPhpOverrideMethod(string name, KeyValuePair<string, Dictionary<int, PhpMemberMethod>> methods)
    {
        var k = 0;
        var isError = false;
        var modifier = string.Empty;
        var @static = string.Empty;
        var end = String.Empty;
        scriptMembers.Add("\t/**");
        foreach (var method in methods.Value.Values)
        {
            var _args = String.Empty;
            var _type = String.Empty;
            List<string> argumentList = new();
            List<string> typeList = new();
            
            k++;
            if (!isError)
            {
                isError = method.IsNameError;
            }
            modifier = method.Modifier;
            
            scriptOverrideMethods.Add("\t /**");
            
            foreach (var argument in method.Arguments)
            {
                var json = JsonConvert.DeserializeObject<TypeArgument>(argument.Value);
                scriptOverrideMethods.Add($"\t  * @param {json.Name.ToOriginalTypeName().Replace('`', '_')} ${argument.Key}");
                argumentList.Add("$" + argument.Key);
            }
            foreach (var t in method.ReturnType)
            {
                typeList.Add(t.Name);
                PushScriptUses($"{t.Namespace}\\{t.Name}");
                //scriptUses.Add($"use {t.Namespace}.{t.Name.ToOriginalTypeName()};".ToReplaceDot("\\"));
            }
            _type = typeList.Count < 1 ? "Void|void" : string.Join("|", typeList);
            _args = string.Join(", ", argumentList);
            
            scriptOverrideMethods.Add($"\t  * @return {_type.Replace('`', '_')}");
            scriptOverrideMethods.Add("\t  */");
            scriptOverrideMethods.Add($"\t#[PhpHidden]protected function {name}_{k}({_args})" + "{}");
            scriptMembers.Add("\t * @uses OverrideMethods::" + $"{name}_{k} ({_args}) : {_type}");
        }
        
        scriptMembers.Add("\t * @return mixed|\\override|[...$args]");
        scriptMembers.Add("\t */");
        end = IsClass() ? "{}" : ";";
        var comment = isError ? "//" : "";
        scriptMembers.Add($"\t{comment}#[Override] {modifier}{@static}function {name}(mixed ...$args){end}");
    }
    protected void ToPhpOriginalMethod(string name, KeyValuePair<string, Dictionary<int, PhpMemberMethod>> methods)
    {
        foreach (var method in methods.Value.Values)
        {
            var args = String.Empty;
            var end = String.Empty;
            var type = String.Empty;
            List<string> argumentList = new();
            List<string> typeList = new();
            scriptMembers.Add("\t/**");
            foreach (var argument in method.Arguments)
            {
                var json = JsonConvert.DeserializeObject<TypeArgument>(argument.Value);
                scriptMembers.Add($"\t * @param {json.Name.ToOriginalTypeName().Replace('`', '_')} ${argument.Key}");
                argumentList.Add("$" + argument.Key);
            }
            foreach (var t in method.ReturnType)
            {
                typeList.Add(t.Name);
                if (t.Name.Equals("Void"))
                {
                    typeList.Clear();
                    typeList.Add("Void");
                    typeList.Add("void");
                    break;
                }
                //scriptUses.Add($"use {t.Namespace}.{t.Name.ToOriginalTypeName()};".ToReplaceDot("\\"));
                PushScriptUses($"{t.Namespace}.{t.Name}");
            }
            type = typeList.Count < 1 ? "Void|void" : string.Join("|", typeList);
            args = string.Join(", ", argumentList);
            end = IsClass() ? "{}" : ";";
            scriptMembers.Add("\t * @return " + type.Replace('`', '_'));
            scriptMembers.Add("\t */");
            var comment = method.IsNameError ? "//" : "";
            scriptMembers.Add($"\t{comment}{method.Modifier}{method.Static}function {name}({args}){end}");
        }
    }
    protected string ToRewriteDirs(string ns)
    {
        if (ns?.IndexOf('<') != -1)
        {
            ns = ".hidden/" + Namespace
                .Replace('<', '_')
                .Replace('>', '_');
        }
        else
        {
            ns = ns?.ToReplaceDot("/");
        }

        return ns;
    }
    protected string ToRewriteDirs()
    {
        var ns = "";
        if (
            string.IsNullOrEmpty(Namespace) ||
            Namespace?.Length <= 0
        )
        {
            ns = ".hidden";
        }
        else
        {
            ns = ToRewriteDirs(Namespace);

        }
        
        return _pathRoot + _pathSdk + "/" +  ns;
    }
    protected string ToExtractElement()
    {
        var elem = "";
        if (Type.IsEnum)
        {
            elem = "enum";
        }
        else if (Type.IsInterface)
        {
            elem = "interface";
        }
        else
        {
            elem = "class";
        }
        return elem;
    }
    protected string ToExtractAbstractElement()
    {
        return Type.IsAbstract ? "abstract " : "";
    }
    
    protected string ToExtractImplements()
    {
        var impls = new List<string>();
        var originalImplName = String.Empty;
        foreach (var impl in Type.GetInterfaces())
        {
            originalImplName = impl.Name.Replace('`', '_');
            impls.Add(originalImplName);
            PushScriptUses($"{impl.Namespace}\\{originalImplName}");
            scriptMembers.Add($"\tuse {originalImplName}Methods;");
        }
        if (impls.Count < 1) return "";
        return "implements \n\t" + string.Join("\n\t", impls) + "\n";
    }

    protected Boolean IsClass()
    {
        return Element == "class";
    }
    protected Boolean IsEnum()
    {
        return Element == "enum";
    }
    protected Boolean IsInterface()
    {
        return Element == "interface";
    }
    protected void Clear()
    {
        _methods.Clear();
        _props.Clear();
        scriptBase.Clear();
        scriptUses.Clear();
        scriptComments.Clear();
        scriptElement.Clear();
        scriptMembers.Clear();
        scriptOverrideMethods.Clear();
        scriptDotTraits.Clear();
        Name = null;
        Element = null;
        Namespace = null;
    }

    protected ListArguments GetListArguments(PhpMemberMethod method)
    {
        var argumentList = new List<string>();
        var argumentParams = new List<string>();
        foreach (var argument in method.Arguments)
        {
            var json = JsonConvert.DeserializeObject<TypeArgument>(argument.Value);
            argumentParams.Add($"\t * @param {json.Name.ToOriginalTypeName().Replace('`', '_')} ${argument.Key}");
            argumentList.Add("$" + argument.Key);
        }
        return new ListArguments()
        {
            Args = argumentList,
            Params = argumentParams,
        };
    }
    protected List<string> GetListTypes(PhpMemberMethod method)
    {
        var typeList = new List<string>();
        var end = method.IsAbstract ? ";" : "{}";
        foreach (var t in method.ReturnType)
        {
            typeList.Add(t.Name);
            PushScriptUses($"{t.Namespace}\\{t.Name}");
        }
        return typeList;
    }

    protected void PushScriptUses(string str)
    {
        str = str.Trim().ToOriginalName().ToReplaceDot("\\").TrimStart('\\');
        scriptUses.Add($"use {str}" + ";");
    }
}