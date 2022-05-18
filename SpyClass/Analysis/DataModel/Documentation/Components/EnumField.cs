using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public sealed class EnumField : DocComponent
    {
        public EnumDoc Owner { get; }
        
        public AttributeList Attributes { get; private set; } = AttributeList.Empty;
        
        public string Name { get; private set; }
        public string Value { get; private set; }

        public EnumField(EnumDoc owner, FieldDefinition field)
        {
            Owner = owner;
            AnalyzeField(field);
        }

        private void AnalyzeField(FieldDefinition field)
        {
            if (field.HasCustomAttributes)
            {
                Attributes = new AttributeList(field.CustomAttributes);
            }

            Name = field.Name;
            Value = field.Constant.ToString();
        }

        public string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            if (Attributes.Any)
            {
                foreach (var attr in Attributes.Attributes)
                {
                    sb.AppendLine(indentation + "    " + attr.BuildStringRepresentation(false));
                }
            }
            
            sb.Append(indentation + $"    {Name} = {Value}");
            return sb.ToString();
        }
    }
}