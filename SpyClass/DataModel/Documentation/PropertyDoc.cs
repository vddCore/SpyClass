using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation
{
    public class PropertyDoc : DocPart
    {
        public string Name { get; private set; }
        public TypeDoc Owner { get; }

        public AccessModifier? GetAccess { get; private set; }
        public AccessModifier? SetAccess { get; private set; }

        public string PropertyTypeFullName { get; private set; }
        public string PropertyTypeDisplayName { get; private set; }

        public PropertyDoc(ModuleDefinition module, TypeDoc owner, PropertyDefinition property)
            : base(module)
        {
            Owner = owner;
            AnalyzeProperty(property);
        }

        private void AnalyzeProperty(PropertyDefinition property)
        {
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

            PropertyTypeFullName = property.PropertyType.FullName;
            PropertyTypeDisplayName = NameTools.MakeDocFriendlyName(PropertyTypeFullName, true);
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

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            sb.Append(AccessModifierString);
            sb.Append(" ");
            sb.Append(PropertyTypeDisplayName);
            sb.Append(" ");
            sb.Append(Name);

            sb.Append(" { ");

            if (GetAccess != null
                && ((GetAccess.Value == AccessModifier.Public || GetAccess.Value == AccessModifier.Protected)
                    || Analyzer.Options.IncludeNonPublicTypes))
            {
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
    }
}