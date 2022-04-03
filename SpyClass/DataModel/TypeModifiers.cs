using System;

namespace SpyClass.DataModel
{
    [Flags]
    public enum TypeModifiers
    {
        Abstract = 1 << 0,
        Sealed = 1 << 1,
        Static = 1 << 2,
    }
}