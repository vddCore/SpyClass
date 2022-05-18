using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class AttributeArgument : DocComponent
    {
        public TypeInfo AttributeArgumentTypeInfo { get; private set; }
        
        public string Name { get; private set; }
        public string ValueString { get; private set; }

        public AttributeArgument(CustomAttributeArgument argument) 
        {
            AnalyzeUnnamedArgument(argument);
        }

        public AttributeArgument(CustomAttributeNamedArgument argument)
        {
            AnalyzeNamedArgument(argument);
        }

        private void AnalyzeUnnamedArgument(CustomAttributeArgument argument)
        {
            AttributeArgumentTypeInfo = new TypeInfo(argument.Type);
            ValueString = StringifyConstant(argument.Value);
        }

        private void AnalyzeNamedArgument(CustomAttributeNamedArgument namedArgument)
        {
            Name = namedArgument.Name;
            AttributeArgumentTypeInfo = new TypeInfo(namedArgument.Argument.Type);
            ValueString = StringifyConstant(namedArgument.Argument.Value);
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            if (Name != null)
            {
                sb.Append(Name);
                sb.Append(" = ");
            }

            sb.Append(ValueString);

            return sb.ToString();
        }
    }
}