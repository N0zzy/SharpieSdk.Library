using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhpieSdk.Library.Service;

public abstract class PhpTemplates: PhpTemplatesHelper
{
    protected void FieldsCompile(FieldInfo[] fieldInfos, bool isComment = true)
    {
        foreach (var fieldInfo in fieldInfos)
        {
            if(fieldInfo.IsPrivate) return;

            if (isComment)
            {
                ContentProperties.Add("\t/**");
                ContentProperties.Add("\t * @field");
                SetContentPropertyReadonly(fieldInfo);
                SetContentPropertyType(fieldInfo.FieldType);
                ContentProperties.Add("\t */");
            }
           
            SetContentProperty(fieldInfo);
        }
    }
    
    protected void FieldsCompileGroup(FieldInfo[] fieldInfos)
    {
        FieldInfo field = null;
        foreach (var fieldInfo in fieldInfos)
        {
            field = fieldInfo; //может возникнуть некорректное представление из за перезагрузки поля
            FieldTypes.Add(PhpBaseTypes.Extract(fieldInfo.FieldType.ToString(), true));
        }
        
        if (field == null) return;
        
        ContentProperties.Add("\t/**");
        ContentProperties.Add("\t * @field dublicate");
        var t = string.Join('|', FieldTypes.Distinct());
        SetContentPropertyType(t);
        FieldTypes.Clear();
        ContentProperties.Add("\t */");
        SetContentProperty(field);
    }
    
    protected void PropertiesCompile(PropertyInfo[] propertyInfos)
    {
        foreach (var propertyInfo in propertyInfos)
        {
            var modifier = GetPhpPropertyModifier(propertyInfo);
            if (modifier == "private") return;
            
            ContentProperties.Add("\t/**");
            ContentProperties.Add("\t * @property");
            SetContentPropertyType(propertyInfo.PropertyType);
            if (!propertyInfo.CanWrite)
            {
                ContentProperties.Add("\t * @since readonly");
            }
            ContentProperties.Add("\t */");
            ContentProperties.Add($"\t{modifier} $" + propertyInfo.Name + ";");
        }
    }
    
    protected void PropertiesCompileGroup(PropertyInfo[] propertyInfos)
    {
        PropertyInfo prop = null;
        foreach (var propertyInfo in propertyInfos)
        {
            prop = propertyInfo; //может возникнуть некорректное представление из за перезагрузки поля
            PropTypes.Add(PhpBaseTypes.Extract(propertyInfo.PropertyType.ToString(), true));
        }

        if (prop == null)
        {
            return;
        }
        var modifier = GetPhpPropertyModifier(prop);
        if (modifier == "private")
        {
            PropTypes.Clear();
            return;
        }
        
        ContentProperties.Add("\t/**");
        ContentProperties.Add("\t * @property dublicate");
        var t = string.Join('|', PropTypes.Distinct());
        SetContentPropertyType(t);
        PropTypes.Clear();
        ContentProperties.Add("\t */");
        var name = prop.Name.Split(".").Last();
        ContentProperties.Add($"\t{modifier} ${name};");
    }
    
    protected void MethodsCompile(MethodInfo[] methods, bool isCtor = false)
    {
        foreach (var methodInfo in methods)
        {
            if (methodInfo.Name.IsPhpNameError() || IsParentMethod(methodInfo) || IsPropertyMethod(methodInfo))
            {
                continue;
            }
            
            SetContentMethod(methodInfo, isCtor);
        }
    }

    protected void MethodsCompileGroup(string name, MethodInfo[] methods)
    {
        var i = 0;
        var n = 0;
        var countPrivate = 0;
        var countPublic = 0;
        var globalAttribute = "";

        name = name.Split(".").Last();
        var parentMethods = DetectParentMethods(name);
        foreach (var pm in parentMethods)
        {
            ParentReturns.Add(PhpBaseTypes.Extract(pm.ReturnType.ToString(), true).Split("[").First());
        }
        
        foreach (var methodInfo in methods)
        {
            //завершить поиск родительских методов
            var x = parentMethods.Where( x => 
                x.Name == methodInfo.Name && 
                x.IsPublic == methodInfo.IsPublic &&
                x.IsStatic == methodInfo.IsStatic
            ).ToArray().Length;

            if(x > 0)
            {
                continue;
            }
            
            n++;
            
            MethodArgsCompile(methodInfo.GetParameters());
            var @modifier = GetPhpModifier(methodInfo.IsPublic, methodInfo.IsPrivate);
            var @static = methodInfo.IsStatic ? "static " : "";
            var @args = GetArgsToString();
            var @type = PhpBaseTypes.Extract(methodInfo.ReturnType.ToString(), true)
                .Split("[").First().Split("+").First();

            switch (@modifier)
            {
                case "public ": countPublic++;
                    break;
                case "private ": countPrivate++;
                    break;
            }
            
            ReturnOverrideMethods.Add(@type);
            ContentUsesMethods.Add($"\t * @uses {PhpSdkStorage.Type.Title}Override::{name}_{n} <br>{@modifier}, args: ({@args})<br>");
            
            ContentOverrideMethods.Add("\t/**");
            ContentOverrideMethods.Add("\t * @deprecated");
            if (CommentMethodArgs.Count > 0)
            {
                ContentOverrideMethods.Add( string.Join("\n", CommentMethodArgs));
            }
            ContentOverrideMethods.Add("\t * @return " + @type);
            ContentOverrideMethods.Add("\t */");
            name = name.ToString().ToUpperFirstSymbol(IsUppercase);
            ContentOverrideMethods.Add($"\t#[MethodOverride]{@modifier}{@static}function {name}_{n} ({@args})" + "{}");
            
            i += MethodArgs.Count;
            MethodArgs.Clear();
            CommentMethodArgs.Clear();
        }

        if (ContentUsesMethods.Count > 1)
        {
            ContentMethods.Add("\t/**");
            
            if (countPrivate >= n)
            {
                globalAttribute = "Private";
                ContentMethods.Add($"\t * @since @override => public | private | protected"); 
                ContentMethods.Add($"\t * {WarningDisclaimer}"); 
            } 
            else if (countPublic > 0)
            {
                globalAttribute = "Public";
            }
            else
            {
                globalAttribute = "Protected";
            }
            
            ContentMethods.Add(string.Join("\n", ContentUsesMethods)); 
            ContentMethods.Add("\t * @var mixed|\\override ...$args");
            if (ReturnOverrideMethods.Count > 0)
            {
                if (ParentReturns.Count > ReturnOverrideMethods.Count)
                {
                    ContentMethods.Add("\t * @return " + string.Join("|", ParentReturns.Distinct()) + "|mixed|\\override");
                }
                else
                {
                    ContentMethods.Add("\t * @return " + string.Join("|", ReturnOverrideMethods.Distinct()) + "|mixed|\\override");
                }
            }
            else
            {
                ContentMethods.Add($"\t * @return mixed|\\override");
            }
           
            ContentMethods.Add("\t */");
            
            name = name.Split(".").Last().ToString().ToUpperFirstSymbol(IsUppercase);
            var args = i > 0 ? "\\override ...$args" : "";
            ContentMethods.Add($"\t#[MethodOverride{globalAttribute}]function {name} ({args})" + "{}");
        }
        ParentReturns.Clear();
        ReturnOverrideMethods.Clear();
        ContentUsesMethods.Clear();
    }

    private List<MethodInfo> DetectParentMethods(string name)
    {
        return ReverseParentMethods(PhpSdkStorage.Type.Instance, name, new List<MethodInfo>());
    }
    
    private List<MethodInfo> ReverseParentMethods(Type type, string name, List<MethodInfo> parentMethods)
    {
        var methods = type.BaseType?.GetMethods(Flags);
        if(methods != null)
        {
            var ms = methods
                .Where(m => m.Name.Split(".").Last() == name)
                .ToList();
            foreach (var m in ms)
            {
                parentMethods.Add(m);
            }
        }
        if (type.BaseType != null)
        {
            return ReverseParentMethods(type.BaseType, name, parentMethods);
        }
        else
        {
            return parentMethods;
        }
    }

    protected void CtorCompile(ConstructorInfo[] ctors)
    {
        string @modifier;
        var n = 0;
        var i = 0;
        if (ctors.Length > 1)
        {
            ContentMethods.Add("\t/**");
            string @args;
            foreach (var ctor in ctors)
            {
                n++;
                ContentOverrideMethods.Add($"\t/**");
                MethodArgsCompile(ctor.GetParameters(), false, true);
                i += MethodArgs.Count;
                @args = GetArgsToString();
                @modifier = GetPhpModifier(ctor.IsPublic, ctor.IsPrivate);
                ContentMethods.Add($"\t * @uses {PhpSdkStorage.Type.Title}Override::__construct_{n} <br>{@modifier}, args: ({@args})<br>");
                ContentOverrideMethods.Add($"\t */");
                ContentOverrideMethods.Add($"\t#[MethodOverride]{@modifier}function __construct_{n} ({@args})" + "{}");
                MethodArgs.Clear();
            }

            @args = string.Empty;
            if (i > 0)
            {
                @args = "\\override ...$args";
                ContentMethods.Add("\t * @var mixed|\\override ...$args");
            }
            ContentMethods.Add("\t*/");
            ContentMethods.Add($"\t#[MethodOverride]function __construct({@args})"+"{}");
        }
        else if (ctors.Length == 1)
        {
            ContentMethods.Add("\t/**");
            @modifier = GetPhpModifier(ctors[0].IsPublic, ctors[0].IsPrivate);
            MethodArgsCompile(ctors[0].GetParameters(), true);
            var @args = GetArgsToString();
            ContentMethods.Add("\t */");
            ContentMethods.Add($"\t{@modifier}function __construct({@args})" + "{}");
            MethodArgs.Clear();
        }
    }
    
    protected void SetNameOverride()
    {
        TraitName = PhpSdkStorage.Type.Title + "Override";
    }
    
    protected void PhpScriptCompile()
    {
        Script.Add(Descriptor);
        Script.Add(Namespace);
        
        if (ContentOverrideMethods.Count > 0)
        {
            Script.Add("/**");
            Script.Add($" * {WarningDeprecated}");
            Script.Add("*/");
            Script.Add($"trait {TraitName} " + "{");
            Script.Add(string.Join("\n", ContentOverrideMethods));
            Script.Add("}");
            ContentOverrideMethods.Clear();
            ContentUses.Add("\tuse " + TraitName +";");
        }
        
        if (CommentModel.Count > 0)
        {
            Script.Add("/**");
            Script.Add(string.Join("\n", CommentModel));
            Script.Add("*/");
            CommentModel.Clear();
        }
        
        Script.Add(Header.Replace('`', '_'));
        
        Script.Add(SymbolOBrace);
        
        if (ContentUses.Count > 0)
        {
            Script.Add(string.Join("\n", ContentUses));
            ContentUses.Clear();
        }
        
        Script.Add(string.Join("\n", ContentProperties));
        ContentProperties.Clear();
        
        Script.Add(string.Join("\n", ContentMethods));
        ContentMethods.Clear();
        
        Script.Add(SymbolСBrace);
    }
}