using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpieSdk.Library.Service;

public class Disassembler: Components
{
    public void SetPath(string root, string sdk)
    {
        _pathRoot = root + sdk;
    }

    public void Add(Type type)
    {
        this.Type = type;
        Record();
        MakeDirectory();
    }

    protected void Record()
    {
        var _name = Type.Name;
        var _namespace = Type.Namespace;

        if (_name != Name || _namespace != Namespace)
        {
            AddPhpDescriptor();
            AddPhpNamespace();
            AddPhpUses();
            AddPhpElements();
            AddPhpProperties();
            AddPhpMethods();
            MakePhpScript(Name);
            
            Clear();
        }

        _members.Type = Type;
        
        Name = _name;
        Namespace = _namespace;
        Element = ToExtractElement();
        Dirs = ToRewriteDirs();
        Abstract = ToExtractAbstractElement();
        Implements = ToExtractImplements();
        
        
        AddCsFields();
        AddCsProps();
        AddCsMethods();
    }

    private void AddCsFields()
    {
        var k = 0;
        foreach (PhpMemberProperty property in _members.ExtractFields())
        {
            if (!_props.ContainsKey(property.Name))
            {
                _props[property.Name] = new Dictionary<int, PhpMemberProperty>();
            }
            _props[property.Name][k] = property;
            k++;
        }
    }
    
    private void AddCsProps()
    {
        var k = 0;
        foreach (var property in _members.ExtractProperties())
        {
            if (!_props.ContainsKey(property.Name))
            {
                _props[property.Name] = new Dictionary<int, PhpMemberProperty>();
            }
            _props[property.Name][k] = property;
            k++;
        }
    }

    private void AddCsMethods()
    {
        var k = 0;
        foreach (var method in _members.ExtractMethods())
        {
            if (!_methods.ContainsKey(method.Name))
            {
                _methods[method.Name] = new Dictionary<int, PhpMemberMethod>();
            }
            _methods[method.Name][k] = method;
            k++;
        }
    }
    
    private void MakeDirectory()
    {
        try
        {
            if (!Directory.Exists(Dirs))
            {
                Directory.CreateDirectory(Dirs);
            }
        }
        catch (Exception)
        {
            // using (StreamWriter writer = new StreamWriter(Dirs + "/__sdk_error_log__.txt", true))
            // {
            //     writer.WriteLine(Dirs);
            // }
        }

    }
    private void MakePhpScript(string name)
    {
        try
        {
            string filename = Dirs + $"/{name.Replace('`', '_')}.php";
            using StreamWriter php = new StreamWriter(filename);
            AddUseOverrideMethods();
            php.WriteLine(string.Join("\n", scriptBase));
            php.WriteLine(string.Join("\n", scriptUses.Distinct()));
            php.WriteLine(string.Join("\n", scriptOverrideMethods));
            AddTraitsMethods(php);
            php.WriteLine(string.Join("\n", scriptComments.Distinct()));
            php.WriteLine(string.Join("\n", scriptElement));
            php.WriteLine(string.Join("\n", scriptMembers));
            php.WriteLine("}");
            php.Close();
        }
        catch (Exception)
        {
            // using StreamWriter writer = new StreamWriter(Dirs + "/__sdk_error_log__.txt", true);
            // writer.WriteLine(name);
        }
    }

    private void AddUseOverrideMethods()
    {
        if (scriptOverrideMethods.Count > 0)
        {
            scriptElement.Add("\tuse OverrideMethods;");
        }
    }
    
    private void AddTraitsMethods(StreamWriter php)
    {
        if (scriptDotTraits.Count > 4)
        {
            var name = scriptDotTraits[0];
            scriptDotTraits.RemoveAt(0);
            php.WriteLine(string.Join("\n", scriptDotTraits));
        }
    }
}