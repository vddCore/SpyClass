using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using SpyClass.Analysis.DataModel.Documentation.Base;
using SpyClass.Analysis.DataModel.Documentation.Components;
using SpyClass.Analysis.Extensions;
using TypeInfo = SpyClass.Analysis.DataModel.Documentation.Components.TypeInfo;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public abstract class TypeDoc : DocPart
    {
        private static Dictionary<string, TypeDoc> _cache = new();

        private CustomAttribute _defaultMemberAttribute;
        private string _defaultMemberName;

        protected TypeDefinition DocumentedType { get; }

        protected virtual HashSet<string> IgnoredValidMethodNames { get; set; } = new();

        public TypeKind Kind { get; }
        public TypeModifier Modifier { get; private set; }
        public TypeDoc DeclaringType { get; private set; }

        public string Namespace { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public virtual string DisplayName { get; private set; }

        public AttributeList Attributes { get; private set; } = AttributeList.Empty;
        public GenericParameterList GenericParameters { get; private set; } = GenericParameterList.Empty;

        public TypeInfo BaseTypeInfo { get; private set; }
        public List<TypeInfo> ImplementedInterfaces { get; private set; } = new();

        public List<FieldDoc> Fields { get; } = new();
        public List<EventDoc> Events { get; } = new();
        public List<IndexerDoc> Indexers { get; } = new();
        public List<PropertyDoc> Properties { get; } = new();
        public List<ConstructorDoc> Constructors { get; } = new();
        public List<MethodDoc> Methods { get; } = new();
        public List<TypeDoc> NestedTypes { get; } = new();

        protected TypeDoc(TypeDefinition documentedType, TypeKind kind)
        {
            DocumentedType = documentedType;
            Kind = kind;

            AnalyzeBasicTypeInformation();

            if (Kind != TypeKind.Enum)
            {
                AnalyzeFields();
            }

            AnalyzeEvents();

            if (_defaultMemberName == "Item")
            {
                AnalyzeIndexers();
            }

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

            if (DocumentedType.HasCustomAttributes)
            {
                Attributes = new AttributeList(DocumentedType.CustomAttributes);
            }
            
            var indexerCriterion = DocumentedType.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == typeof(DefaultMemberAttribute).FullName);

            if (indexerCriterion != null)
            {
                var itemPropertyName = indexerCriterion.ConstructorArguments[0].Value.ToString();

                if (itemPropertyName == "Item")
                {
                    Attributes.RemoveAll(x =>
                        x.AttributeTypeInfo.FullName == typeof(DefaultMemberAttribute).FullName);
                    
                    _defaultMemberName = itemPropertyName;
                    _defaultMemberAttribute = indexerCriterion;
                }
            }

            if (DocumentedType.BaseType != null && !IsBuiltInBaseType(DocumentedType.BaseType.FullName))
            {
                BaseTypeInfo = new TypeInfo(DocumentedType.BaseType);
            }

            if (DocumentedType.HasInterfaces)
            {
                foreach (var iface in DocumentedType.Interfaces)
                {
                    ImplementedInterfaces.Add(new TypeInfo(iface.InterfaceType));
                }
            }

            if (DocumentedType.HasGenericParameters)
            {
                GenericParameters = new GenericParameterList(
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

                Fields.Add(new FieldDoc(field, this));
            }
        }

        private void AnalyzeEvents()
        {
            foreach (var ev in DocumentedType.Events)
            {
                Events.Add(new EventDoc(this, ev));
            }
        }

        private void AnalyzeIndexers()
        {
            if (_defaultMemberName == null)
                return;

            foreach (var prop in DocumentedType.Properties)
            {
                if (prop.CustomAttributes.Any(x =>
                        x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)
                    && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                    continue;

                if (_defaultMemberName == null)
                    continue;

                if (prop.Name != _defaultMemberName)
                    continue;

                var propDoc = new IndexerDoc(this, prop);

                if (propDoc.Access == AccessModifier.Public
                    || propDoc.Access == AccessModifier.Protected
                    || Analyzer.Options.IncludeNonUserMembers)
                {
                    Indexers.Add(propDoc);
                }
            }
        }

        private void AnalyzeProperties()
        {
            foreach (var prop in DocumentedType.Properties)
            {
                if (prop.CustomAttributes.Any(x =>
                        x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)
                    && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                    continue;

                if (_defaultMemberName != null && prop.Name == _defaultMemberName)
                    continue;

                var propDoc = new PropertyDoc(this, prop);

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
                    Constructors.Add(new ConstructorDoc(this, method));
                }
                else
                {
                    Methods.Add(new MethodDoc(this, method));
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

                var typeDoc = FromType(type);
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

        public static TypeDoc FromType(TypeDefinition type)
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

                case TypeKind.Delegate:
                    ret = new DelegateDoc(type);
                    break;

                case TypeKind.Struct:
                    ret = new StructDoc(type);
                    break;

                case TypeKind.Record:
                    ret = new RecordDoc(type);
                    break;

                case TypeKind.Interface:
                    ret = new InterfaceDoc(type);
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

        public static string BuildBasicIdentityString(TypeDoc typeDoc)
        {
            var sb = new StringBuilder();

            sb.Append(typeDoc.AccessModifierString);

            switch (typeDoc.Modifier)
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

            if (typeDoc.Kind == TypeKind.Struct && ((StructDoc)typeDoc).IsReadOnly)
            {
                sb.Append(" readonly");
            }

            switch (typeDoc.Kind)
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
                    sb.AppendLine(indentation + attrib.BuildStringRepresentation(false));
                }
            }

            sb.Append(indentation);
            sb.Append(BuildBasicIdentityString(this));
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
            
            if (Events.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Events */");
                foreach (var ev in Events)
                {
                    sb.AppendLine(indentation + "    " + ev);
                }
            }

            if (Indexers.Any())
            {
                sb.AppendLine(indentation + "    " + "/* Indexers */");
                foreach (var indexer in Indexers)
                {
                    sb.AppendLine(indentation + "    " + indexer);
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