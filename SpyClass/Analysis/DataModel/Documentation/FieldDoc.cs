using System;
using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;
using SpyClass.Analysis.DataModel.Documentation.Components;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public sealed class FieldDoc : DocPart
    {
        public TypeDoc Owner { get; }
        public TypeInfo FieldTypeInfo { get; private set; }

        public AttributeList Attributes { get; private set; } = AttributeList.Empty;

        public FieldModifiers Modifiers { get; private set; }
        public string Name { get; private set; }
        public string DefaultValueString { get; private set; }

        public FieldDoc(FieldDefinition field, TypeDoc owner)
        {
            Owner = owner;

            AnalyzeField(field);
        }

        private void AnalyzeField(FieldDefinition field)
        {
            Access = DetermineAccess(field);
            Modifiers = DetermineModifiers(field);
            
            Name = field.Name;
            FieldTypeInfo = new TypeInfo(field.FieldType);

            if (field.HasConstant)
            {
                DefaultValueString = StringifyConstant(field.Constant);
            }

            if (field.HasCustomAttributes)
            {
                Attributes = new AttributeList(field.CustomAttributes);
            }
        }

        public static FieldModifiers DetermineModifiers(FieldDefinition field)
        {
            FieldModifiers ret = 0;

            if (field.IsLiteral)
            {
                return FieldModifiers.Const;
            }

            if (field.IsInitOnly)
            {
                ret |= FieldModifiers.ReadOnly;
            }

            if (field.FieldType is RequiredModifierType)
            {
                ret |= FieldModifiers.Volatile;
            }

            if (field.IsStatic)
            {
                ret |= FieldModifiers.Static;
            }
            
            return ret;
        }

        public static AccessModifier DetermineAccess(FieldDefinition field)
        {
            if (field.IsPublic)
            {
                return AccessModifier.Public;
            }
            else if (field.IsFamily)
            {
                return AccessModifier.Protected;
            }
            else if (field.IsAssembly)
            {
                return AccessModifier.Internal;
            }
            else if (field.IsFamilyOrAssembly)
            {
                return AccessModifier.ProtectedInternal;
            }
            else if (field.IsPrivate)
            {
                return AccessModifier.Private;
            }
            else if (field.IsFamilyAndAssembly)
            {
                return AccessModifier.PrivateProtected;
            }

            throw new NotSupportedException($"Cannot determine type access: {field.Name}");
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            sb.Append(indentation + AccessModifierString);

            if (Modifiers.HasFlag(FieldModifiers.Const))
            {
                sb.Append(" const");
            }
            else
            {
                if (Modifiers.HasFlag(FieldModifiers.Static))
                {
                    sb.Append(" static");
                }

                if (Modifiers.HasFlag(FieldModifiers.ReadOnly))
                {
                    sb.Append(" readonly");
                }

                if (Modifiers.HasFlag(FieldModifiers.Volatile))
                {
                    sb.Append(" volatile");
                }
            }

            sb.Append(" ");
            sb.Append(FieldTypeInfo.BuildStringRepresentation());
            sb.Append(" ");
            sb.Append(Name);

            if (DefaultValueString != null)
            {
                sb.Append(" = ");
                sb.Append(DefaultValueString);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}