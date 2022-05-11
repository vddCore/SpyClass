using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;
using SpyClass.Extensions;

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

        public List<TypeDoc> NestedTypes { get; private set; } = new();
        public List<MethodDoc> Methods { get; private set; } = new();
        public List<EventDoc> Events { get; private set; } = new();
        public List<PropertyDoc> Properties { get; private set; } = new();

        public virtual string DisplayName { get; private set; }

        public TypeInfo BaseTypeInfo { get; private set; }

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
            DisplayName = NameTools.RemoveClrTypeCharacters(FullName, true);

            if (DocumentedType.BaseType != null && !IsBuiltInBaseType(DocumentedType.BaseType.FullName))
            {
                BaseTypeInfo = new TypeInfo(Module, DocumentedType.BaseType);
            }

            if (DocumentedType.NestedTypes.Any())
            {
                NestedTypes = new List<TypeDoc>();

                foreach (var type in DocumentedType.NestedTypes)
                {
                    var compilerGeneratedAttribute =
                        type.GetCustomAttribute(typeof(CompilerGeneratedAttribute).FullName);

                    if (compilerGeneratedAttribute != null && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                        continue;

                    if (!type.IsPublic && !Analyzer.Options.IncludeNonPublicTypes)
                        continue;

                    var typeDoc = FromType(Module, type);
                    typeDoc.DeclaringType = this;

                    NestedTypes.Add(typeDoc);
                }
            }

            if (DocumentedType.GenericParameters.Any())
            {
                GenericParameters = new GenericParameterList(
                    Module,
                    DocumentedType.GenericParameters
                );
            }

            if (DocumentedType.Methods.Any())
            {
                Methods = new List<MethodDoc>();

                foreach (var method in DocumentedType.Methods)
                {
                    if (!method.IsPublic && !method.IsFamily && !Analyzer.Options.IncludeNonUserMembers)
                        continue;

                    if ((method.IsGetter || method.IsSetter) && !Analyzer.Options.IncludeNonUserMembers)
                        continue;

                    if ((method.IsAddOn || method.IsRemoveOn) && !Analyzer.Options.IncludeNonUserMembers)
                        continue;

                    if (method.IsConstructor)
                    {
                    }
                    else
                    {
                        Methods.Add(new MethodDoc(Module, this, method));
                    }
                }
            }

            if (DocumentedType.Events.Any())
            {
                Events = new List<EventDoc>();

                foreach (var ev in DocumentedType.Events)
                {
                    Events.Add(new EventDoc(Module, this, ev));
                }
            }

            if (DocumentedType.Properties.Any())
            {
                Properties = new List<PropertyDoc>();

                foreach (var prop in DocumentedType.Properties)
                {
                    var propDoc = new PropertyDoc(Module, this, prop);

                    if (propDoc.Access == AccessModifier.Public
                        || propDoc.Access == AccessModifier.Protected
                        || Analyzer.Options.IncludeNonUserMembers)
                    {
                        Properties.Add(propDoc);
                    }
                }
            }
        }

        private bool IsBuiltInBaseType(string fullName)
        {
            return fullName == typeof(object).FullName
                   || fullName == typeof(ValueType).FullName
                   || fullName == typeof(Enum).FullName;
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
                    ret = new StructDoc(module, type);
                    break;

                case TypeKind.Record:
                    ret = new RecordDoc(module, type);
                    break;

                case TypeKind.Interface:
                    ret = new InterfaceDoc(module, type);
                    break;

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

        protected string BuildDisplayNameString()
        {
            var sb = new StringBuilder();

            sb.Append(" ");
            sb.Append(DisplayName);

            return sb.ToString();
        }

        protected virtual string BuildInnerContent(int indent)
        {
            return string.Empty;
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            sb.Append(indentation);
            sb.Append(BuildBasicIdentityString());
            sb.Append(BuildDisplayNameString());

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterListString());
            }

            if (BaseTypeInfo != null)
            {
                sb.Append(" : ");
                sb.Append(BaseTypeInfo.BuildStringRepresentation());
            }

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterConstraintString());
            }

            sb.AppendLine();
            sb.AppendLine(indentation + "{");
            sb.Append(BuildInnerContent(indent));

            foreach (var prop in Properties)
            {
                sb.AppendLine(indentation + "    " + prop.BuildStringRepresentation());
            }

            foreach (var ev in Events)
            {
                sb.AppendLine(indentation + "    " + ev.BuildStringRepresentation());
            }

            foreach (var method in Methods)
            {
                sb.AppendLine(indentation + "    " + method.BuildStringRepresentation());
            }

            foreach (var type in NestedTypes)
            {
                sb.AppendLine(type.BuildStringRepresentation(4));
            }

            sb.Append(indentation + "}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}