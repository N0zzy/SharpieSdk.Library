using System.Reflection;

namespace PhpieSdk.Library.Service;

public class PhpEnum: PhpFactory
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
        FieldsCompile(PhpSdkStorage.Type.Instance.GetFields(Flags), false);
    }

    protected override void Methods()
    {
        
    }

    protected override void Events()
    {
        
    }
}