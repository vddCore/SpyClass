using System;

namespace SpyClass.Analysis.DataModel
{
    [Flags]
    public enum FieldModifiers
    {
        Const = 1 << 0,
        ReadOnly = 1 << 1,
        Static = 1 << 2,
        Volatile = 1 << 3
    }
}