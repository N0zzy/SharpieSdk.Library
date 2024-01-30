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
        ContentProperties.Add($"\t{modifier} $" + prop.Name.Split(".").Last() + ";");
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

        foreach (var methodInfo in methods)
        {
            // if (IsParentMethod(methodInfo))
            // {
            //     continue;
            // }
            
            n++;
            
            MethodArgsCompile(methodInfo.GetParameters());
            var @modifier = GetPhpModifier(methodInfo.IsPublic, methodInfo.IsPrivate);
            var @static = methodInfo.IsStatic ? "static " : "";
            var @args = GetArgsToString();
            var @type = PhpBaseTypes.Extract(methodInfo.ReturnType.ToString(), true).Split("[").First();

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
            ContentOverrideMethods.Add($"\t#[MethodOverride]{@modifier}{@static}function {name.Split(".").Last()}_{n} ({@args})" + "{}");
            
            i += MethodArgs.Count;
            MethodArgs.Clear();
            CommentMethodArgs.Clear();
        }

        if (ContentUsesMethods.Count > 1)
        {
            ContentMethods.Add("\t/**");
            ContentMethods.Add(string.Join("\n", ContentUsesMethods)); 
            ContentMethods.Add("\t * @var mixed|\\override ...$args");
            if (ReturnOverrideMethods.Count > 0)
            {
                ContentMethods.Add("\t * @return " + string.Join("|", ReturnOverrideMethods.Distinct()) + "|mixed|\\override");
            }
            else
            {
                ContentMethods.Add($"\t * @return mixed|\\override");
            }
           
            ContentMethods.Add("\t */");
            
            if (countPrivate >= n)
            {
                globalAttribute = "Private";
            } 
            else if (countPublic > 0)
            {
                globalAttribute = "Public";
            }
            else
            {
                globalAttribute = "Protected";
            }

            name = name.Split(".").Last();
            var args = i > 0 ? "\\override ...$args" : "";
            ContentMethods.Add($"\t#[MethodOverride{globalAttribute}]function {name} ({args})" + "{}");
        }
        
        ReturnOverrideMethods.Clear();
        ContentUsesMethods.Clear();
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
            MethodArgsCompile(ctors[0].GetParameters());
            var @args = GetArgsToString();
            ContentMethods.Add("\t */");
            ContentMethods.Add($"\t{@modifier}function __construct({@args})" + "{}");
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