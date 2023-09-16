using System;
using PhpieSdk.Library.Service;

namespace PhpieSdk.Library;

public sealed class PhpRoute: PhpFactory
{
    public PhpRoute(Settings settings, AssemblyType type)
    {
        _settings = settings;
        _type = type;
    }

    public void Run()
    {
        switch (_type.Model)
        {
            case "class": PhpClass(); break;
            case "interface": PhpInterface(); break;
            case "enum": PhpEnum(); break;
            case "struct": PhpClass(true); break;
            default: "".WarnLn(_type.Model); break;
        }
    }
}