using System;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public sealed class FieldDoc : DocPart
    {
        public TypeDoc Owner { get; }

        public FieldModifiers Modifiers { get; private set; }
        public TypeInfo FieldTypeInfo { get; private set; }

        public string Name { get; }

        public FieldDoc(ModuleDefinition module, FieldDefinition field, TypeDoc owner)
            : base(module)
        {
            Owner = owner;
            Access = DetermineAccess(field);

            FieldTypeInfo = new TypeInfo(Module, field.FieldType);

            Name = field.Name;
            Modifiers = DetermineModifiers(field);
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

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}