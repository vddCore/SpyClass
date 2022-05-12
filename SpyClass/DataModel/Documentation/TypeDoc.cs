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

        protected virtual HashSet<string> IgnoredValidMethodNames { get; set; } = new();

        public TypeKind Kind { get; }
        public TypeModifier Modifier { get; private set; }
        public TypeDoc DeclaringType { get; private set; }

        public string Namespace { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public virtual string DisplayName { get; private set; }

        public AttributeList Attributes { get; private set; }
        public GenericParameterList GenericParameters { get; private set; }

        public TypeInfo BaseTypeInfo { get; private set; }
        public List<TypeInfo> ImplementedInterfaces { get; private set; } = new();

        public List<FieldDoc> Fields { get; } = new();
        public List<PropertyDoc> Properties { get; private set; } = new();
        public List<EventDoc> Events { get; private set; } = new();
        public List<ConstructorDoc> Constructors { get; private set; } = new();
        public List<MethodDoc> Methods { get; private set; } = new();
        public List<TypeDoc> NestedTypes { get; private set; } = new();

        protected TypeDoc(ModuleDefinition module, TypeDefinition documentedType, TypeKind kind)
            : base(module)
        {
            DocumentedType = documentedType;
            Kind = kind;

            AnalyzeBasicTypeInformation();

            if (Kind != TypeKind.Enum)
            {
                AnalyzeFields();
            }

            AnalyzeEvents();
            AnalyzeProperties();
            AnalyzeMethods();
            AnalyzeNestedTypes();
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

            if (DocumentedType.HasInterfaces)
            {
                foreach (var iface in DocumentedType.Interfaces)
                {
                    ImplementedInterfaces.Add(new TypeInfo(Module, iface.InterfaceType));
                }
            }

            if (DocumentedType.HasCustomAttributes)
            {
                Attributes = new AttributeList(Module, DocumentedType.CustomAttributes);
            }

            if (DocumentedType.HasGenericParameters)
            {
                GenericParameters = new GenericParameterList(
                    Module,
                    DocumentedType.GenericParameters
                );
            }
        }

        private void AnalyzeFields()
        {
            foreach (var field in DocumentedType.Fields)
            {
                if (!field.IsPublic && !field.IsFamily && !Analyzer.Options.IncludeNonUserMembers)
                    continue;

                if (field.IsCompilerControlled && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                    continue;

                Fields.Add(new FieldDoc(Module, field, this));
            }
        }

        private void AnalyzeEvents()
        {
            foreach (var ev in DocumentedType.Events)
            {
                Events.Add(new EventDoc(Module, this, ev));
            }
        }

        private void AnalyzeProperties()
        {
            foreach (var prop in DocumentedType.Properties)
            {
                if (prop.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)
                    && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                    continue;
                
                var propDoc = new PropertyDoc(Module, this, prop);

                if (propDoc.Access == AccessModifier.Public
                    || propDoc.Access == AccessModifier.Protected
                    || Analyzer.Options.IncludeNonUserMembers)
                {
                    Properties.Add(propDoc);
                }
            }
        }

        private void AnalyzeMethods()
        {
            foreach (var method in DocumentedType.Methods)
            {
                if (IgnoredValidMethodNames.Contains(method.Name))
                    continue;

                if (!method.IsPublic && !method.IsFamily && !Analyzer.Options.IncludeNonUserMembers)
                    continue;

                if ((method.IsGetter || method.IsSetter) && !Analyzer.Options.IncludeNonUserMembers)
                    continue;

                if ((method.IsAddOn || method.IsRemoveOn) && !Analyzer.Options.IncludeNonUserMembers)
                    continue;

                if (method.IsConstructor)
                {
                    Constructors.Add(new ConstructorDoc(Module, this, method));
                }
                else
                {
                    Methods.Add(new MethodDoc(Module, this, method));
                }
            }
        }

        private void AnalyzeNestedTypes()
        {
            foreach (var type in DocumentedType.NestedTypes)
            {
                var compilerGeneratedAttribute =
                    type.GetCustomAttribute(typeof(CompilerGeneratedAttribute).FullName);

                if (compilerGeneratedAttribute != null && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                    continue;

                if (!type.IsNestedPublic && !type.IsNestedFamily && !Analyzer.Options.IncludeNonPublicTypes)
                    continue;

                var typeDoc = FromType(Module, type);
                typeDoc.DeclaringType = this;

                NestedTypes.Add(typeDoc);
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

            if (Attributes != null)
            {
                foreach (var attrib in Attributes.Attributes)
                {
                    sb.AppendLine(indentation + "[" + attrib.BuildStringRepresentation() + "]");
                }
            }

            sb.Append(indentation);
            sb.Append(BuildBasicIdentityString());
            sb.Append(BuildDisplayNameString());

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterListString());
            }

            if (BaseTypeInfo != null || ImplementedInterfaces.Any())
            {
                sb.Append(" : ");
            }

            if (BaseTypeInfo != null)
            {
                sb.Append(BaseTypeInfo.BuildStringRepresentation());

                if (ImplementedInterfaces.Any())
                    sb.Append(", ");
            }

            if (ImplementedInterfaces.Any())
            {
                sb.Append(string.Join(", ", ImplementedInterfaces.Select(x => x.BuildStringRepresentation())));
            }

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterConstraintString());
            }

            sb.AppendLine();
            sb.AppendLine(indentation + "{");
            sb.Append(BuildInnerContent(indent));

            if (Fields.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Fields */");
                foreach (var field in Fields)
                {
                    sb.AppendLine(indentation + "    " + field);
                }
            }

            if (Properties.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Properties */");
                foreach (var prop in Properties)
                {
                    sb.AppendLine(indentation + "    " + prop);
                }
            }

            if (Events.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Events */");
                foreach (var ev in Events)
                {
                    sb.AppendLine(indentation + "    " + ev);
                }
            }

            if (Methods.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Methods */");
                foreach (var method in Methods)
                {
                    sb.AppendLine(indentation + "    " + method);
                }
            }

            if (Constructors.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Constructors */");
                foreach (var constructor in Constructors)
                {
                    sb.AppendLine(indentation + "    " + constructor);
                }
            }

            if (NestedTypes.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Nested types */");
                foreach (var type in NestedTypes)
                {
                    sb.AppendLine(type.BuildStringRepresentation(4));
                }
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