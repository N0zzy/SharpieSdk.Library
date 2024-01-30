
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

    private void ScriptBuilderForAll()
    {
        SetNamespace();
        PhpScriptCompile();
        Task.Run(() =>
        {
            ImmutableList<string> immutable = Script.ToImmutableList();
            Script.Clear();
            // var key =  Path.GetMD5();
            // var content = GetScriptToString(immutable.ToList());
            // var value = content.GetMD5();
            // if (PhpSdkStorage.Files.TryGetValue(key, out var file))
            // {
            //     if (file == value)
            //     {
            //         return;
            //     }
            // }
            // PhpSdkStorage.Files[key] = value;
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