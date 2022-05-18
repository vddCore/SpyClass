using System;

namespace SpyClass.Analysis.DataModel
{
    [Flags]
    public enum MethodModifiers
    {
        Static = 1 << 0,
        Abstract = 1 << 1,
        Virtual = 1 << 2,
        Extern = 1 << 3
    }
}