using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharpieSdk.Library.Service;

public abstract class Components
{
    public readonly string Descriptor = "<?php";
    public string _pathRoot = String.Empty;
    public string _pathCustom = String.Empty;
    public readonly string _pathSdk = "/.sdk";

    public string Dirs { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Element { get; set; }
    public string Extends { get; set; }
    public Type Type { get; set; }
    
    public String Abstract = null;
    
    //string - key => name element
    public Dictionary<string, Dictionary<int, PhpMemberMethod>> _methods = new();
    public Dictionary<string, Dictionary<int, PhpMemberProperty>> _props = new();

    public List<string> scriptBase = new();
    public List<string> scriptUses = new();
    public List<string> scriptComments = new();
    public List<string> scriptElement = new();
    public List<string> scriptMembers = new();
    public List<string> scriptOverrideMethods = new();

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
        scriptElement.Add($"{__abstract}{Element} {Name} " + "{");
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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }

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
            scriptUses.Add(
                "use " + (prop.TypeNamespace + "." + prop.TypeName).ToOriginalName().ToReplaceDot("\\") + ";"
            );
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
                scriptOverrideMethods.Add($"\t  * @param {json.Name.ToOriginalTypeName()} ${argument.Key}");
                argumentList.Add("$" + argument.Key);
            }
            foreach (var t in method.ReturnType)
            {
                typeList.Add(t.Name);
                scriptUses.Add($"use {t.Namespace}.{t.Name.ToOriginalTypeName()};".ToReplaceDot("\\"));
            }
            _type = typeList.Count < 1 ? "Void|void" : string.Join("|", typeList);
            _args = string.Join(", ", argumentList);
            
            scriptOverrideMethods.Add($"\t  * @return {_type}");
            scriptOverrideMethods.Add("\t  */");
            scriptOverrideMethods.Add($"\tprotected function {name}_{k}({_args})" + "{}");
            scriptMembers.Add("\t * @uses OverrideMethods::" + $"{name}_{k} ({_args}) : {_type}");
        }
        
        scriptMembers.Add("\t * @return mixed");
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
            //Console.WriteLine($"method: {method.Name}");
            //Console.WriteLine(string.Join(", ", method.Arguments.Values.ToArray()));

            foreach (var argument in method.Arguments)
            {
                var json = JsonConvert.DeserializeObject<TypeArgument>(argument.Value);
                scriptMembers.Add($"\t * @param {json.Name.ToOriginalTypeName()} ${argument.Key}");
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
                scriptUses.Add($"use {t.Namespace}.{t.Name.ToOriginalTypeName()};".ToReplaceDot("\\"));
            }
            
            type = typeList.Count < 1 ? "Void|void" : string.Join("|", typeList);
            
            args = string.Join(", ", argumentList);
            end = IsClass() ? "{}" : ";";
            scriptMembers.Add("\t * @return " + type);
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
        Name = null;
        Element = null;
        Namespace = null;
    }
}