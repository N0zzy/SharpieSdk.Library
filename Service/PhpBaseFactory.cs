namespace PhpieSdk.Library;

public abstract class PhpFactory: PhpBaseParameters
{
    protected void PhpEnum()
    {
        new PhpEnum(_settings, _type);
    }

    protected void PhpInterface()
    {
        new PhpInterface(_settings, _type);
    }

    protected void PhpClass(bool isFinal = false)
    {
        _type.isFinal = isFinal;
        new PhpClass(_settings, _type);
    }
}