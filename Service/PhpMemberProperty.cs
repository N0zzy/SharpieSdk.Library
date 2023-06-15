using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace SharpieSdk.Library.Service;

public class PhpMemberProperty
{
    /// master params
    
    public string Name { set; get; }
    public string TypeName { set; get; }
    public string TypeNamespace { set; get; }
    public string Dollar { init; get; }
    public string Member { set; get; }
    public string ModifierBase { set; get; }
    public string ModifierStatic { set; get; }
    public Boolean IsLiteral { set; get; }
    public Boolean IsInitOnly { set; get; }
    
    public Boolean IsNameError = false;
    
    /// slave params
 
    public List<string> Methods = new List<string>();
}