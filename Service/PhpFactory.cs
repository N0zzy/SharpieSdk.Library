
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhpieSdk.Library.Service;

public abstract class PhpFactory: PhpTemplates
{
    protected abstract void Comments();
    protected abstract void Models();
    protected abstract void Properties();
    protected abstract void Methods();

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
        bool result = taskComments.Result && 
                      taskModels.Result && 
                      taskProperties.Result && 
                      taskMethods.Result;
        if (result)
        {
            PhpSdkStorage.Type.Model.Clear();
            ScriptBuilderForAll();
            return;
        }
        
        throw new Exception("Task Run in error");
    }
}