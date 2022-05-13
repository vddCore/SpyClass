using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public class ConstructorDoc : DocPart
    {
        public TypeDoc Owner { get; }

        public AttributeList Attributes { get; private set; }
        public MethodModifiers Modifiers { get; private set; }
        public MethodParameterList Parameters { get; private set; }

        public ConstructorDoc(TypeDoc owner, MethodDefinition constructor)
        {
            Owner = owner;
            AnalyzeConstructor(constructor);
        }

        private void AnalyzeConstructor(MethodDefinition constructor)
        {
            Access = MethodDoc.DetermineAccess(constructor);
            Modifiers = MethodDoc.DetermineModifiers(constructor);
            Parameters = new MethodParameterList(constructor.Parameters, false);

            if (constructor.HasCustomAttributes)
            {
                Attributes = new AttributeList(constructor.CustomAttributes);
            }
        }

        private string BuildConstructorModifierString()
        {
            var sb = new StringBuilder();

            if (Modifiers.HasFlag(MethodModifiers.Abstract))
            {
                sb.Append(" abstract");
            }

            if (Modifiers.HasFlag(MethodModifiers.Static))
            {
                sb.Append(" static");
            }

            if (Modifiers.HasFlag(MethodModifiers.Virtual))
            {
                sb.Append(" virtual");
            }

            return sb.ToString();
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            if (Attributes != null)
            {
                sb.Append(indentation);
                sb.Append(Attributes.BuildStringRepresentation(false));
            }

            sb.Append(indentation);
            sb.Append(AccessModifierString);
            sb.Append(BuildConstructorModifierString());
            sb.Append(" ");
            sb.Append(Owner.DisplayName);
            sb.Append(Parameters.BuildStringRepresentation(false));

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}