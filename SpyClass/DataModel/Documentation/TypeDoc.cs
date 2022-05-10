using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public abstract class TypeDoc : DocPart
    {
        private static Dictionary<string, TypeDoc> _cache = new();
        
        protected TypeDefinition DocumentedType { get; }

        public TypeKind Kind { get; }
        public TypeModifier Modifier { get; private set; }
        public TypeDoc DeclaringType { get; private set; }
        
        public string Namespace { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }

        public GenericParameterList GenericParameters { get; private set; }
        
        public List<TypeDoc> NestedTypes { get; private set; }
        public virtual string DisplayName => FullName.Replace("+", ".").Split("`")[0];

        protected TypeDoc(ModuleDefinition module, TypeDefinition documentedType, TypeKind kind)
            : base(module)
        {
            DocumentedType = documentedType;
            Kind = kind;

            AnalyzeBasicTypeInformation();
        }

        private void AnalyzeBasicTypeInformation()
        {
            Access = DetermineAccess(DocumentedType);
            Modifier = DetermineModifiers(DocumentedType);
            
            Namespace = DocumentedType.Namespace;
            Name = DocumentedType.Name;
            FullName = DocumentedType.FullName;

            if (DocumentedType.NestedTypes.Any())
            {
                NestedTypes = new List<TypeDoc>();

                foreach (var type in DocumentedType.NestedTypes)
                {
                    var typeDoc = FromType(Module, type);
                    typeDoc.DeclaringType = this;

                    NestedTypes.Add(typeDoc);
                }
            }

            if (DocumentedType.GenericParameters.Any())
            {
                GenericParameters = new GenericParameterList(
                    Module, 
                    this, 
                    DocumentedType.GenericParameters
                );
            }
        }

        public static TypeDoc FromType(ModuleDefinition module, TypeDefinition type)
        {
            if (_cache.ContainsKey(type.FullName!))
                return _cache[type.FullName];

            TypeDoc ret;
            var typeKind = DetermineKind(type);

            switch (typeKind)
            {
                case TypeKind.Enum:
                    ret = new EnumDoc(module, type);
                    break;

                case TypeKind.Class:
                    ret = new ClassDoc(module, type);
                    break;
                
                case TypeKind.Delegate:
                    ret = new DelegateDoc(module, type);
                    break;
                
                case TypeKind.Struct:
                case TypeKind.Record:
                case TypeKind.Interface:
                default:
                    throw new NotSupportedException($"Unsupported type kind {typeKind}.");
            }

            _cache.Add(type.FullName, ret);
            return ret;
        }

        public static AccessModifier DetermineAccess(TypeDefinition type)
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
                else if (type.IsNestedFamilyOrAssembly)
                {
                    return AccessModifier.ProtectedInternal;
                }
                else if (type.IsNestedPrivate)
                {
                    return AccessModifier.Private;
                }
                else if (type.IsNestedFamilyAndAssembly)
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
                else
                {
                    return AccessModifier.Internal;
                }
            }

            throw new NotSupportedException($"Cannot determine type access: {type.FullName}");
        }

        public static TypeKind DetermineKind(TypeDefinition type)
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
                if (type.BaseType.FullName == typeof(MulticastDelegate).FullName)
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
                .Replace("System.String", "string")
                .Replace("System.IntPtr", "nint")
                .Replace("System.UIntPtr", "nuint")
                .Replace("System.Object", "object");

            return ret;
        }
        
        public static TypeModifier DetermineModifiers(TypeDefinition type)
        {
            var modifier = TypeModifier.Empty;

            if (type.IsSealed && type.IsAbstract)
            {
                modifier = TypeModifier.Static;
            }
            else if (type.IsSealed
                     && type.BaseType.FullName != typeof(MulticastDelegate).FullName
                     && type.BaseType.FullName != typeof(ValueType).FullName
                     && type.BaseType.FullName != typeof(Enum).FullName)
            {
                modifier = TypeModifier.Sealed;
            }
            else if (type.IsAbstract && !type.IsInterface)
            {
                modifier = TypeModifier.Abstract;
            }
            else if (type.IsByReference
                     && type.BaseType.FullName == typeof(ValueType).FullName)
            {
                modifier = TypeModifier.Ref;
            }

            return modifier;
        }

        protected string BuildBasicIdentityString()
        {
            var sb = new StringBuilder();

            sb.Append(AccessModifierString);
            
            switch (Modifier)
            {
                case TypeModifier.Abstract:
                    sb.Append(" abstract");
                    break;
                
                case TypeModifier.Sealed:
                    sb.Append(" sealed");
                    break;
                
                case TypeModifier.Static:
                    sb.Append(" static");
                    break;
                
                case TypeModifier.Ref:
                    sb.Append(" ref");
                    break;
            }

            switch (Kind)
            {
                case TypeKind.Enum:
                    sb.Append(" enum");
                    break;
                
                case TypeKind.Class:
                    sb.Append(" class");
                    break;
                
                case TypeKind.Struct:
                    sb.Append(" struct");
                    break;
                
                case TypeKind.Record:
                    sb.Append(" record");
                    break;
                
                case TypeKind.Delegate:
                    sb.Append(" delegate");
                    break;
                
                case TypeKind.Interface:
                    sb.Append(" interface");
                    break;
            }

            return sb.ToString();
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            sb.Append(indentation);
            sb.Append(BuildBasicIdentityString());

            sb.Append(" ");
            sb.Append(DisplayName);

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterListString());
                sb.Append(GenericParameters.BuildGenericParameterConstraintString());
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}