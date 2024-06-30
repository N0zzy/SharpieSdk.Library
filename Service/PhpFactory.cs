
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PhpieSdk.Library.Service;

public abstract class PhpFactory: PhpTemplates
{
    protected abstract void Comments();
    protected abstract void Models();
    protected abstract void Properties();
    protected abstract void Methods();
    protected abstract void Events();

    public PhpFactory SetUppercase(bool value)
    {
        IsUppercase = value;
        return this;
    }
    
    
    private void ScriptBuilderForAll()
    {
        SetNamespace();
        PhpScriptCompile();
        Task.Run(() =>
        {
            ImmutableList<string> immutable = Script.ToImmutableList();
            Script.Clear();
            File.WriteAllTextAsync(Path, GetScriptToString(immutable.ToList()));
        });
    }
    
    public void Execute()
    {
        Path = (PhpSdkStorage.Type.Model.Path + PhpSdkStorage.Type.Name + ".php")
            .Replace('`', '_');
        
        
        Task<bool> taskMethods = Task.Run(() => { Methods(); return true; });
        Task<bool> taskProperties = Task.Run(() => { Properties(); return true; });
        Task<bool> taskComments = Task.Run(() => { Comments(); return true; });
        Task<bool> taskModels = Task.Run(() => { Models(); return true; });
        Task<bool> taskEvents = Task.Run(() => { Events(); return true; });
        bool result = taskComments.Result && 
                      taskModels.Result && 
                      taskProperties.Result && 
                      taskMethods.Result && 
                      taskEvents.Result;
        if (result)
        {
            PhpSdkStorage.Type.Model.Clear();
            ScriptBuilderForAll();
            return;
        }
        
        throw new Exception("Task Run in error");
    }
    
    [Obsolete]
    protected string GetXml(Assembly assembly)
    {
        string assemblyPath = new Uri(assembly.Location).LocalPath;
        string xmlPath = System.IO.Path.ChangeExtension(assemblyPath, ".xml");

        if (File.Exists(xmlPath))
        {
            return ExtractSummaryFromXml(assemblyPath, assembly.GetType(PhpSdkStorage.Type.Name));
        }
        return string.Empty;
    }
    [Obsolete]
    private string ExtractSummaryFromXml(string xmlPath, Type type)
    {
        var xml = XDocument.Load(xmlPath);
        string typeName = $"T:{type.FullName}";
        var summaryElement = xml.Descendants("member")
            .FirstOrDefault(m => m.Attribute("name")?.Value == typeName)
            ?.Element("summary");
    
        return summaryElement?.Value.Trim() ?? "Comment not found.";
    }
}