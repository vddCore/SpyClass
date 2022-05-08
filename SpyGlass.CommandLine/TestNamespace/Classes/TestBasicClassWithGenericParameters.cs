namespace SpyGlass.CommandLine.TestNamespace.Classes
{
    public class TestBasicClassWithGenericParameters<T, U, V> 
        where T: TestBasicClassWithFields, new()
        where U: struct
        where V: unmanaged
    {
    }
}