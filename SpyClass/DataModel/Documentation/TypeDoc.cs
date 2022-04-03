using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpyClass.DataModel.Documentation
{
    public abstract class TypeDoc : DocPart
    {
        private static Dictionary<string, TypeDoc> _cache = new();
        
        protected Type DocumentedType { get; }

        public TypeKind Kind { get; }
        public TypeDoc DeclaringType { get; private set; }
        
        public string Namespace { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }

        public List<TypeDoc> NestedTypes { get; private set; }
        public virtual string DisplayName => FullName.Replace("+", ".");

        protected TypeDoc(Type documentedType, TypeKind kind)
        {
            DocumentedType = documentedType;
            Kind = kind;

            AnalyzeBasicTypeInformation();
        }

        private void AnalyzeBasicTypeInformation()
        {
            Access = DetermineAccess(DocumentedType);

            Namespace = DocumentedType.Namespace;
            Name = DocumentedType.Name;
            FullName = DocumentedType.FullName;

            var nestedTypes = DocumentedType.GetNestedTypes(
                BindingFlags.Public 
                | BindingFlags.NonPublic
            );

            if (nestedTypes.Any())
            {
                NestedTypes = new List<TypeDoc>();

                foreach (var type in nestedTypes)
                {
                    var typeDoc = FromType(type);
                    typeDoc.DeclaringType = this;

                    NestedTypes.Add(typeDoc);
                }
            }
        }

        public static TypeDoc FromType(Type type)
        {
            if (_cache.ContainsKey(type.FullName!))
                return _cache[type.FullName];

            TypeDoc ret;
            var typeKind = DetermineKind(type);

            switch (typeKind)
            {
                case TypeKind.Enum:
                    ret = new EnumDoc(type);
                    break;

                case TypeKind.Class:
                    ret = new ClassDoc(type);
                    break;
                
                case TypeKind.Struct:
                case TypeKind.Record:
                case TypeKind.Delegate:
                case TypeKind.Interface:
                default:
                    throw new NotSupportedException($"Unsupported type kind {typeKind}.");
            }

            _cache.Add(type.FullName, ret);
            return ret;
        }

        public static AccessModifier DetermineAccess(Type type)
        {
            if (type.IsNested)
            {
                if (type.IsNestedPublic)
                {
                    return AccessModifier.Public;
                }
                else if (type.IsNestedFamily)
                {
                    return AccessModifier.Protected;
                }
                else if (type.IsNestedAssembly)
                {
                    return AccessModifier.Internal;
                }
                else if (type.IsNestedFamORAssem)
                {
                    return AccessModifier.ProtectedInternal;
                }
                else if (type.IsNestedPrivate)
                {
                    return AccessModifier.Private;
                }
                else if (type.IsNestedFamANDAssem)
                {
                    return AccessModifier.PrivateProtected;
                }
            }
            else
            {
                if (type.IsPublic)
                {
                    return AccessModifier.Public;
                }
                else if (!type.IsVisible)
                {
                    return AccessModifier.Internal;
                }
            }

            throw new NotSupportedException($"Cannot determine type access: {type.FullName}");
        }

        public static TypeKind DetermineKind(Type type)
        {
            if (type.IsInterface)
            {
                return TypeKind.Interface;
            }
            else if (type.IsEnum)
            {
                return TypeKind.Enum;
            }
            else if (type.IsValueType)
            {
                return TypeKind.Struct;
            }
            else if (type.IsClass)
            {
                if (type.BaseType == typeof(MulticastDelegate))
                {
                    return TypeKind.Delegate;
                }
                else
                {
                    var methods = type.GetMethods();

                    if (methods.FirstOrDefault(m => m.Name == "<Clone>$") != null)
                        return TypeKind.Record;

                    return TypeKind.Class;
                }
            }

            throw new NotSupportedException($"Cannot determine type kind: {type.FullName}.");
        }

        public static string TryAliasTypeName(string fullTypeName)
        {
            var ret = fullTypeName
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
                .Replace("System.Object", "object");

            return ret;
        }
    }
}