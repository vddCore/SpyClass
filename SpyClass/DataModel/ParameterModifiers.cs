using System;

namespace SpyClass.DataModel
{
    [Flags]
    public enum ParameterModifiers
    {
        Out = 1 << 0,
        Ref = 1 << 1,
        Params = 1 << 2,
        This = 1 << 3
    }
}