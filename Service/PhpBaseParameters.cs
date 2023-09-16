using System.Collections.Generic;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public class PhpBaseParameters
{
    protected Settings _settings;
    protected AssemblyType _type;
    
    protected List<string> _model = new();
    protected List<string> _properties = new();
    protected List<string> _methods = new();
    protected List<string> _methodsTraitOverride = new();
    protected List<string> _commentsModel = new();
    protected List<string> _commentsMethods = new();
}