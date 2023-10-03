using System;
using System.Collections.Generic;

namespace PhpieSdk.Library.Service;

public class AssemblyType
{
    public string Model { get; set; }
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public string FullName { get; set; }
    public string Namespace { get; set; }
    public string Extends { get; set; }
    
    public Type CurrentType { get; set; }
    public Type BaseType { get; set; }
    public bool isFinal { get; set; } = false;
    public List<string> Implements { get; set; }
    
    public Dictionary<string, TypeVariables> Properties { get; set; } = new ();
    public Dictionary<string, TypeVariables> Fields { get; set; } = new ();
    public Dictionary<string, List<TypeEvents>> Events { get; set; } = new ();
    public Dictionary<string, List<TypeMethod>> Methods { get; set; } = new ();

}