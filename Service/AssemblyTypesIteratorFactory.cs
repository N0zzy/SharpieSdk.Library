using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PhpieSdk.Library.Service;

public abstract class AssemblyTypesIteratorFactory: MembersFactory
{
    protected void TypeCreate()
    {
        if(IsErrorName()) return;
        if(Cache?.Name != Name)
        {
            MakeAndInit();
        }
        AddMembers();
        ModelCount++;
    }
    
    protected void TypeClear()
    {
        if (ModelCount > 0) return;
        Cache = null;
    }
    
    private void MakeAndInit()
    {
        if (ModelCount > 0)
        {
            PhpCreator();
            ModelCount = 0;
        }
        
        Cache = new AssemblyType()
        {
            CurrentType = Type,
            BaseType = Type.BaseType,
            Model = GetModel(),
            Name = Name,
            OriginalName = Type.Name,
            Namespace = Type.Namespace,
            Extends = Type.BaseType == null 
                ? "" 
                : "\\" + (Type.BaseType.Namespace + "." + Type.BaseType.Name).ToReplaceDot("\\"),
            Implements = Type.GetInterfaces().Select(i => ("\\" + i.Namespace + "." + i.Name).ToReplaceDot("\\")).ToArray(),
            FullName = Type.FullName
        };

        MakeDirectory();
    }

    private void AddMembers()
    {
        AddFields();
        AddProperties();
        AddConstructs();
        AddMethods();
    }

    private string GetModel()
    {
        if (Type.IsClass || Type is { IsValueType: true, IsEnum: false })
        {
            return "class";
        }
        else if (Type.IsInterface)
        {
            return "interface";
        }
        else if (Type.IsEnum)
        {
            return "enum";
        }
        else
        {
            return null;
        }
    }

    private bool IsErrorName()
    {
        return !Regex.IsMatch(Name, "^[a-z0-9\\.`_]+$", RegexOptions.IgnoreCase);    
    }
    
    private void MakeDirectory()
    {
        string @namespace = string.IsNullOrEmpty(Type.Namespace)
            ? ".hidden" : Type.Namespace.ToReplaceDot("/");
        
        Settings.outputScriptPath = Settings.outputPath + "/" + Settings.sdkName + "/" + @namespace;
        
        if (!Directory.Exists(Settings.outputScriptPath))
        {
            Directory.CreateDirectory(Settings.outputScriptPath);
        }
    }
    
    private void PhpCreator()
    {
        new PhpRoute(Settings, Cache).Run();
        Cache = new AssemblyType();
    }
}