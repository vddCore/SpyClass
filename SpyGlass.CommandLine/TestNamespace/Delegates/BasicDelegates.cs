namespace SpyGlass.CommandLine.TestNamespace.Delegates
{
    public delegate int TestVoidDelegate(string arg1, int arg2);

    public delegate T GetThing<T>(T arg1, byte arg2, char arg3) where T: class, new();
}