using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public class IndexerDoc : DocPart
    {
        public TypeDoc Owner { get; }

        public AttributeList Attributes { get; private set; }

        public AttributeList GetAttributes { get; private set; }
        public AccessModifier? GetAccess { get; private set; }
        
        public AttributeList SetAttributes { get; private set; }
        public AccessModifier? SetAccess { get; private set; }
        
        public MethodParameterList Parameters { get; private set; }

        public TypeInfo IndexerTypeInfo { get; private set; }

        public IndexerDoc(TypeDoc owner, PropertyDefinition property)
        {
            Owner = owner;
            AnalyzeIndexer(property);
        }

        private void AnalyzeIndexer(PropertyDefinition property)
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

            IndexerTypeInfo = new TypeInfo(property.PropertyType);

            if (property.GetMethod != null)
            {
                Parameters = new MethodParameterList(property.GetMethod.Parameters, false);
            }
            else if (property.SetMethod != null)
            {
                Parameters = new MethodParameterList(property.SetMethod.Parameters, true);
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
                    sb.AppendLine(indentation + "[" + attrib.BuildStringRepresentation() + "]");
                }
            }
            
            sb.Append(indentation);
            sb.Append(AccessModifierString);
            sb.Append(" ");
            sb.Append(IndexerTypeInfo.BuildStringRepresentation());
            sb.Append(" this");
            sb.Append("[");
            sb.Append(Parameters.BuildStringRepresentation(true));
            sb.Append("]");

            sb.Append(" { ");

            if (GetAccess != null
                && ((GetAccess.Value == AccessModifier.Public || GetAccess.Value == AccessModifier.Protected)
                    || Analyzer.Options.IncludeNonPublicTypes))
            {
                if (GetAttributes != null)
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
                if (SetAttributes != null)
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

            sb.Append("}");

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}