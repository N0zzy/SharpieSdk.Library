namespace PhpieSdk.Library.Service;

public class PhpInterface: PhpFactory
{
    protected override void Comments()
    {
   
    }
    
    protected override void Models()
    {
        Header = PhpSdkStorage.Type.Model.Name + " " + PhpSdkStorage.Type.Name;
    }

    protected override void Properties()
    {
        
    }

    protected override void Methods()
    {
        MethodsCompile(PhpSdkStorage.Type.Instance.GetMethods(Flags));
    }
}