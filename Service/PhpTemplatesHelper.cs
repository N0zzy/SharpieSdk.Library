using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PhpieSdk.Library.Service;

public abstract class PhpTemplatesHelper
{
    public string Path { protected get; set; }
    
    protected List<string> Script = new();
    protected List<string> ContentProperties = new();
    protected List<string> ContentEvents = new();
    protected List<string> ContentMethods = new();
    protected List<string> ContentOverrideMethods = new();
    protected List<string> ContentUsesMethods = new();
    protected List<string> ReturnOverrideMethods = new();
    protected List<string> ContentUses = new();
    protected List<string> FieldTypes = new();
    protected List<string> PropTypes = new();
    protected List<string> CommentModel = new();
    protected List<string> MethodArgs = new();
    protected List<string> CommentMethodArgs = new();
    protected List<string> MethodOverrideArgs = new();
    protected List<string> ParentReturns = new();

    protected readonly BindingFlags Flags = 
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
    
    protected readonly string WarningDeprecated = 
        "@deprecated this element should not be used by you because it will break your program";
    protected readonly string WarningDisclaimer = 
        "@internal Please ensure that this item can interact with your program before using it, as it may not be available and its use is your responsibility.";
    
    protected readonly string Descriptor = "<?php";
    protected readonly string SymbolOBrace = "{";
    protected readonly string SymbolСBrace = "}";

    protected string Namespace = String.Empty;
    protected string Header = String.Empty;
    protected string TraitName = String.Empty;

    protected readonly Type[] TypeInt = new Type[] { typeof(int) };
    
    protected bool IsUppercase = false;
    
    protected void SetNamespace()
    {
        Namespace = PhpSdkStorage.Type.Namespace;
        if (!string.IsNullOrEmpty(Namespace))
        {
            Namespace = $"namespace {Namespace.ToReplaceDot("\\")};";
        }
    }

    protected string GetScriptToString(List<string> script)
    {
        return string.Join("\n", script);
    }

    protected string GetImpsInterfaces()
    {
        return PhpSdkStorage.Type.Model.Implements.Length > 0 
            ? $" implements\n\t{string.Join(",\n\t", PhpSdkStorage.Type.Model.Implements.Distinct())}" 
            : "";
    }
    
    protected void SetContentPropertyType(in Type t)
    {
        ContentProperties.Add("\t * @var " + PhpBaseTypes.Extract(t.ToString(), true));
    }
    
    protected void SetContentPropertyType(String t)
    {
        ContentProperties.Add("\t * @var " + t);
    }

    protected void SetContentPropertyReadonly(in FieldInfo fieldInfo)
    {
        if(fieldInfo.IsInitOnly)
            ContentProperties.Add("\t * @since readonly");
    }
    
    protected void SetContentProperty(in FieldInfo field)
    {
        var @static = field.IsStatic ? "static " : "";
        var @var = String.Empty;
        if (PhpSdkStorage.Type.Instance.IsEnum)
        {
            if (IsFieldConst(field))
            {
                var v = field.GetRawConstantValue();
                @var += $"case {field.Name} = '{v}';";
            }
        }
        else
        {
            if (IsFieldConst(field))
            {
                @var += $"const {field.Name} = '{field.GetValue(null)}';";
            }
            else
            {
                var name = field.Name;
                @var = $"{GetPhpModifier(field.IsPublic, field.IsPrivate)}{@static}${name};";
            }
        }
        ContentProperties.Add("\t" + $"{@var}");
    }

    protected void SetContentEventType(in Type t)
    {
        var customType = PhpSdkStorage.Type.EventType != null ? $"|{PhpSdkStorage.Type.EventType}" : "";
        ContentEvents.Add("\t * @var " + PhpBaseTypes.Extract(t.ToString(), true));
    }
    
    protected void SetContentEventUsesMethods(string name)
    {
        ContentEvents.Add($"\t * @uses {PhpSdkStorage.Type.Name}::add_{name}()");
        ContentEvents.Add($"\t * @uses {PhpSdkStorage.Type.Name}::remove_{name}()");
    }
    
    protected string GetPhpModifier(bool isPublic, bool isPrivate)
    {
        return isPublic ? "public " : (isPrivate ? "private " : "protected ");
    }
    
    protected string GetPhpPropertyModifier(PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            return "public";
        else if (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsFamily)
            return "protected";
        else 
            return "private";
    }
    
    protected bool IsFieldConst(FieldInfo field)
    {
        return field.IsLiteral && field.IsStatic;
    }

    /// <summary>
    /// Простейший фильтр и он не учитывает модификаторы 
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    protected bool IsParentMethod(MethodInfo methodInfo)
    {
        return IsInherited(
            PhpSdkStorage.Type.Instance, 
            methodInfo.Name.Split(".").Last(),
            GetPhpModifier(methodInfo.IsPublic, methodInfo.IsPrivate)
        );
    }

    private bool IsInherited(Type type, string name, string modifier)
    {
        var methods = type.BaseType?.GetMethods(Flags);
        if (methods == null)
        {
            return false;
        }
        
        foreach (var methodInfo in methods)
        {
            if (
                name == methodInfo.Name && 
                modifier == GetPhpModifier(methodInfo.IsPublic, methodInfo.IsPrivate)
            )
            {
                return true;
            }
        }
        return IsInherited(type.BaseType, name, modifier);
    }

    protected bool IsPropertyMethod(MethodInfo methodInfo)
    {
        return methodInfo.IsSpecialName && methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_");
    }

    protected void SetContentMethodReturnType(Type t)
    {
        ContentMethods.Add("\t * @return " + PhpBaseTypes.Extract(t.ToString(), true));
    }

    protected string GetArgsToString()
    {
        return string.Join(", ", MethodArgs);
    }
    protected void SetContentMethod(MethodInfo method, bool isCtor)
    {
        var @name = isCtor ? "__construct" : method.Name.Split(".").Last();
        if (name.IsPhpLastNameError()) return;
        
        var @static = method.IsStatic ? "static " : "";
        var @abstract = method.IsAbstract && !PhpSdkStorage.Type.Instance.IsInterface 
            ? "abstract " : "";
        var @back = method.IsAbstract ? ";" : "{}";
        var @modifier = GetPhpModifier(method.IsPublic, method.IsPrivate);
        string @args;
   
        if (PhpSdkStorage.Type.Instance.IsInterface)
        {
            if (method.Attributes.HasFlag(MethodAttributes.Virtual) && method.Attributes.HasFlag(MethodAttributes.SpecialName))
            {
                MethodArgsCompile(method.GetParameters(), false);
                args = GetArgsToString();
                @name = @name.ToString().ToUpperFirstSymbol(IsUppercase);
                CommentModel.Add($" * @method {PhpBaseTypes.Extract(method.ReturnType.ToString(), true)} {@name}({@args}) [modifier: {@modifier.TrimEnd()}]");
                MethodArgs.Clear();
                CommentMethodArgs.Clear();
                return;
            }
        }
        
        if (!method.IsPrivate)
        {
            ContentMethods.Add("\t/**");
            MethodArgsCompile(method.GetParameters(), true);
            SetContentMethodReturnType(method.ReturnType);
            ContentMethods.Add("\t */");
        }
        else
        {
            MethodArgsCompile(method.GetParameters(), false);
        }
        
        args = GetArgsToString();
        @name = @name.ToString().ToUpperFirstSymbol(IsUppercase);
        ContentMethods.Add($"\t{@abstract}" + GetPhpModifier(
            method.IsPublic, method.IsPrivate
        ) + $"{@static}function {@name}({@args}){@back}");
        
        MethodArgs.Clear();
        CommentMethodArgs.Clear();
    }
    
    protected void MethodArgsCompile(ParameterInfo[] parameterInfos, bool isParam, bool isOverride = false)
    {
        foreach (var parameter in parameterInfos)
        {
            var @params = parameter.IsDefined(typeof(ParamArrayAttribute), false) ? "..." : "";
            var @out = parameter.IsOut || parameter.IsIn || parameter.ParameterType == typeof(TypedReference) ? "&" : @params;
            var @type = PhpBaseTypes.Extract(parameter.ParameterType.ToString(), true);
            Match match = Regex.Match(@type, @"\[(.+?)\]");
            var @generic = string.Empty;
            if (match.Success)
            {
                @generic = " [generic: " + match.Groups[1].Value + "]";
            }
            var p = "\t * @param " + @type.Split("[")[0] +
                    $" {@out}$" + parameter.Name + @generic;
            if (isParam)
            {
                ContentMethods.Add(p);
            }
            if (isOverride)
            {
                MethodOverrideArgs.Add(p);
                ContentOverrideMethods.Add(p);
            }
            MethodArgs.Add($"{@out}$" + parameter.Name);
        }
    }
    
    protected void MethodArgsCompile(ParameterInfo[] parameterInfos)
    {
        foreach (var parameter in parameterInfos)
        {
            var @params = parameter.IsDefined(typeof(ParamArrayAttribute), false) ? "..." : "";
            var @out = parameter.IsOut || parameter.IsIn || parameter.ParameterType == typeof(TypedReference) ? "&" : @params;
            var @type = PhpBaseTypes.Extract(parameter.ParameterType.ToString(), true);
            Match match = Regex.Match(@type, @"\[(.+?)\]");
            var @generic = string.Empty;
            if (match.Success)
            {
                @generic = " [generic: " + match.Groups[1].Value + "]";
            }
            var comment = "\t * @param " + @type.Split("[")[0] +
                    $" {@out}$" + parameter.Name + @generic;
           
            MethodArgs.Add($"{@out}$" + parameter.Name);
            CommentMethodArgs.Add(comment);
        }
    }
}