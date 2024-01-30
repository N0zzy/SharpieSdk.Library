namespace PhpieSdk.Library.Service;

public class PhpWrapper
{
    public string ModelName { private get; set; }
    private PhpClass phpClass = new PhpClass();
    private PhpEnum phpEnum = new PhpEnum();
    private PhpInterface phpInterface = new PhpInterface();
    
    public void Run()
    {
        Route();
    }

    private void Route()
    {
        switch (ModelName)
        {
            case "class": PhpClass(); break;
            case "interface": PhpInterface(); break;
            case "enum": PhpEnum(); break;
            case "struct": PhpClass(true); break;
            default: "".WarnLn(ModelName); break;
        }
    }

    private void PhpEnum() => phpEnum.Execute();
    private void PhpInterface() => phpInterface.Execute();
    private void PhpClass(bool isStruct = false) => phpClass.SetStruct(isStruct).Execute();
}