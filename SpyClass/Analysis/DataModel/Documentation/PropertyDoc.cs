using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;
using SpyClass.Analysis.DataModel.Documentation.Components;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public class PropertyDoc : DocPart
    {
        public TypeDoc Owner { get; }
        
        public string Name { get; private set; }

        public AttributeList Attributes { get; private set; } = AttributeList.Empty;

        public AttributeList GetAttributes { get; private set; } = AttributeList.Empty;
        public AccessModifier? GetAccess { get; private set; }

        public AttributeList SetAttributes { get; private set; } = AttributeList.Empty;
        public AccessModifier? SetAccess { get; private set; }

        public TypeInfo PropertyTypeInfo { get; private set; }
        public string DefaultValueString { get; private set; }

        public PropertyDoc(TypeDoc owner, PropertyDefinition property)
        {
            Owner = owner;
            AnalyzeProperty(property);
        }

        private void AnalyzeProperty(PropertyDefinition property)
        {
            if (property.HasCustomAttributes)
            {
                Attributes = new AttributeList(property.CustomAttributes);
            }

            if (property.GetMethod != null && property.GetMethod.HasCustomAttributes)
            {
                GetAttributes = new AttributeList(property.GetMethod.CustomAttributes);
            }

            if (property.SetMethod != null && property.SetMethod.HasCustomAttributes)
            {
                SetAttributes = new AttributeList(property.SetMethod.CustomAttributes);
            }

            AnalyzeGetAccessLevel(property);
            AnalyzeSetAccessLevel(property);

            if (GetAccess == null)
            {
                Access = SetAccess!.Value;
            }
            else if (SetAccess == null)
            {
                Access = GetAccess!.Value;
            }
            else if (GetAccess != null && SetAccess != null)
            {
                Access = (int)GetAccess!.Value < (int)SetAccess!.Value
                    ? GetAccess.Value
                    : SetAccess.Value;
            }

            Name = property.Name;
            PropertyTypeInfo = new TypeInfo(property.PropertyType);

            if (property.HasConstant)
            {
                DefaultValueString = StringifyConstant(property.Constant);
            }
        }

        private void AnalyzeGetAccessLevel(PropertyDefinition property)
        {
            if (property.GetMethod == null)
                return;

            if (property.GetMethod.IsPublic)
            {
                GetAccess = AccessModifier.Public;
            }
            else if (property.GetMethod.IsFamily)
            {
                GetAccess = AccessModifier.Protected;
            }
            else if (property.GetMethod.IsAssembly)
            {
                GetAccess = AccessModifier.Internal;
            }
            else if (property.GetMethod.IsFamilyOrAssembly)
            {
                GetAccess = AccessModifier.ProtectedInternal;
            }
            else if (property.GetMethod.IsPrivate)
            {
                GetAccess = AccessModifier.Private;
            }
            else if (property.GetMethod.IsFamilyAndAssembly)
            {
                GetAccess = AccessModifier.PrivateProtected;
            }
        }

        private void AnalyzeSetAccessLevel(PropertyDefinition property)
        {
            if (property.SetMethod == null)
                return;

            if (property.SetMethod.IsPublic)
            {
                SetAccess = AccessModifier.Public;
            }
            else if (property.SetMethod.IsFamily)
            {
                SetAccess = AccessModifier.Protected;
            }
            else if (property.SetMethod.IsAssembly)
            {
                SetAccess = AccessModifier.Internal;
            }
            else if (property.SetMethod.IsFamilyOrAssembly)
            {
                SetAccess = AccessModifier.ProtectedInternal;
            }
            else if (property.SetMethod.IsPrivate)
            {
                SetAccess = AccessModifier.Private;
            }
            else if (property.SetMethod.IsFamilyAndAssembly)
            {
                SetAccess = AccessModifier.PrivateProtected;
            }
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
            sb.Append(AccessModifierString);
            sb.Append(" ");
            sb.Append(PropertyTypeInfo.BuildStringRepresentation());
            sb.Append(" ");
            sb.Append(Name);

            sb.Append(" { ");

            if (GetAccess != null
                && ((GetAccess.Value == AccessModifier.Public || GetAccess.Value == AccessModifier.Protected)
                    || Analyzer.Options.IncludeNonPublicTypes))
            {
                if (GetAttributes.Any)
                {
                    sb.Append(GetAttributes.BuildStringRepresentation(true));
                    sb.Append(" ");
                }

                if ((int)GetAccess > (int)Access)
                {
                    sb.Append(BuildAccessLevelString(GetAccess.Value));
                    sb.Append(" ");
                }

                sb.Append("get; ");
            }

            if (SetAccess != null
                && ((SetAccess.Value == AccessModifier.Public || SetAccess.Value == AccessModifier.Protected)
                    || Analyzer.Options.IncludeNonPublicTypes))
            {
                if (SetAttributes.Any)
                {
                    sb.Append(SetAttributes.BuildStringRepresentation(true));
                    sb.Append(" ");
                }

                if ((int)SetAccess > (int)Access)
                {
                    sb.Append(BuildAccessLevelString(SetAccess.Value));
                    sb.Append(" ");
                }

                sb.Append("set; ");
            }

            if (GetAttributes.Any || SetAttributes.Any)
                sb.AppendLine();

            sb.Append("}");

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