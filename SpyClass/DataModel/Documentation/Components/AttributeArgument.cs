using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class AttributeArgument : DocComponent
    {
        public TypeInfo AttributeArgumentTypeInfo { get; private set; }
        
        public string Name { get; private set; }
        public string ValueString { get; private set; }

        public AttributeArgument(ModuleDefinition module, CustomAttributeArgument argument) 
            : base(module)
        {
            AnalyzeUnnamedArgument(argument);
        }

        public AttributeArgument(ModuleDefinition module, CustomAttributeNamedArgument argument)
            : base(module)
        {
            AnalyzeNamedArgument(argument);
        }

        private void AnalyzeUnnamedArgument(CustomAttributeArgument argument)
        {
            AttributeArgumentTypeInfo = new TypeInfo(Module, argument.Type);
            ValueString = StringifyConstant(argument.Value);
        }

        private void AnalyzeNamedArgument(CustomAttributeNamedArgument argument)
        {
            AttributeArgumentTypeInfo = new TypeInfo(Module, argument.Argument.Type);
            Name = argument.Name;
            ValueString = StringifyConstant(argument.Argument.Value);
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