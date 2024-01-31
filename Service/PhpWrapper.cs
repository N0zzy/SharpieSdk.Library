namespace PhpieSdk.Library.Service;

public class PhpWrapper
{
    public string ModelName { private get; set; }
    public bool IsUppercase { private get; set; } = false;
    
    private PhpClass phpClass = new PhpClass();
    private PhpEnum phpEnum = new PhpEnum();
    private PhpInterface phpInterface = new PhpInterface();
    
    public void Run()
    {
        Route();
    }

    private void Route()
    {
        PhpFactory phpFactory = null;
        
        switch (ModelName)
        {
            case "class": phpFactory = PhpClass(); break;
            case "interface": phpFactory = PhpInterface(); break;
            case "enum": phpFactory = PhpEnum(); break;
            case "struct": phpFactory = PhpClass(); break;
            default: "".WarnLn(ModelName); break;
        }

        if (phpFactory != null)
        {
            phpFactory
                .SetUppercase(IsUppercase)
                .Execute();
        }
    }

    private PhpFactory PhpEnum() => phpEnum;
    private PhpFactory PhpInterface() => phpInterface;
    private PhpFactory PhpClass(bool isStruct = false) => phpClass;
}