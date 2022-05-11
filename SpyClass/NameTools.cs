using System.Text.RegularExpressions;

namespace SpyClass
{
    internal static class NameTools
    {
        public static string MakeDocFriendlyName(string fullName, bool removeGenericTypeDescriptor)
        {
            return TryAliasTypeName(RemoveClrTypeCharacters(fullName, removeGenericTypeDescriptor));
        }

        public static string RemoveClrTypeCharacters(string fullName, bool removeGenericTypeDescriptor)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            fullName = fullName.Replace("&", "");

            if (removeGenericTypeDescriptor)
            {
                return fullName.Split("`")[0];
            }
            else
            {
                return Regex.Replace(fullName, @"`\d+", "")
                            .Replace(",", ", ");
            }
        }

        public static string TryAliasTypeName(string fullName)
        {
            var ret = fullName
                .Replace("System.Boolean", "bool")
                .Replace("System.Void", "void")
                .Replace("System.Char", "char")
                .Replace("System.Byte", "byte")
                .Replace("System.SByte", "sbyte")
                .Replace("System.UInt16", "ushort")
                .Replace("System.Int16", "short")
                .Replace("System.UInt32", "uint")
                .Replace("System.Int32", "int")
                .Replace("System.UInt64", "ulong")
                .Replace("System.Int64", "long")
                .Replace("System.Single", "float")
                .Replace("System.Double", "double")
                .Replace("System.Decimal", "decimal")
                .Replace("System.String", "string")
                .Replace("System.IntPtr", "nint")
                .Replace("System.UIntPtr", "nuint")
                .Replace("System.Object", "object");

            return ret;
        }
    }
}