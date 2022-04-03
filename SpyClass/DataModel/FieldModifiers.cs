using System;

namespace SpyClass.DataModel
{
    [Flags]
    public enum FieldModifiers
    {
        Const = 1 << 0,
        ReadOnly = 1 << 1,
        Static = 1 << 2,
        Ref = 1 << 3,
        Volatile = 1 << 4
    }
}