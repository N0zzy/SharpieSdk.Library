using System.Collections.Generic;

namespace PchpSdkLibrary.Service;

public class PhpMemberMethod
{
    public string Name { set; get; }
    public List<PhpReturnType> ReturnType { set; get; }

    public Dictionary<string, string> Arguments = new();
}