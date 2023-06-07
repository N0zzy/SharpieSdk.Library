using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace PchpSdkLibrary.Service;

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
        
        try
        {
            AddCsFields();
            AddCsProps();
            AddCsMethods();
        }
        catch (Exception e)
        {
            GD.Print(e.Message);
            GD.Print(e.StackTrace);
        }
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
            using (StreamWriter writer = new StreamWriter(Dirs + "/__sdk_error_log__.txt", true))
            {
                writer.WriteLine(Dirs);
            }
        }

    }
    private void MakePhpScript(string name)
    {
        try
        {
            using StreamWriter php = new StreamWriter(Dirs + $"/{name}.php");
            php.WriteLine(string.Join("\n", scriptBase));
            php.WriteLine(string.Join("\n", scriptUses.Distinct()));
            php.WriteLine(string.Join("\n", scriptComments.Distinct()));
            php.WriteLine(string.Join("\n", scriptElement));
            php.WriteLine(string.Join("\n", scriptMembers));
            php.WriteLine("}");
            php.Close();
        }
        catch (Exception)
        {
            using StreamWriter writer = new StreamWriter(Dirs + "/__sdk_error_log__.txt", true);
            writer.WriteLine(name);
        }
    }
}

