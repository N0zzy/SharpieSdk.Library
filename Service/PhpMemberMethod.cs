using System.Collections.Generic;

namespace SharpieSdk.Library.Service;

public class PhpMemberMethod
{
    public string Name { set; get; }
    public string Modifier { set; get; }
    public string Static { set; get; }
    public List<PhpReturnType> ReturnType { set; get; }
    public bool IsNameError { get; set; }

    public Dictionary<string, string> Arguments = new();
}