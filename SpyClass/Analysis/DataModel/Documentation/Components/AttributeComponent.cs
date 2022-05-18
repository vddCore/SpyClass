using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;
using SpyClass.Analysis.DataModel.Documentation.Components;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public class AttributeComponent : DocComponent
    {
        public TypeInfo AttributeTypeInfo { get; private set; }

        public List<AttributeArgument> ConstructorArguments { get; private set; } = new();
        public List<AttributeArgument> NamedArguments { get; private set; } = new();

        public AttributeComponent(CustomAttribute customAttribute)
        {
            AnalyzeCustomAttribute(customAttribute);
        }

        private void AnalyzeCustomAttribute(CustomAttribute customAttribute)
        {
            AttributeTypeInfo = new TypeInfo(customAttribute.AttributeType);
            
            foreach (var constructorArgument in customAttribute.ConstructorArguments)
            {
                ConstructorArguments.Add(new AttributeArgument(constructorArgument));
            }

            foreach (var fieldArgument in customAttribute.Fields)
            {
                NamedArguments.Add(new AttributeArgument(fieldArgument));
            }

            foreach (var propertyArgument in customAttribute.Properties)
            {
                NamedArguments.Add(new AttributeArgument(propertyArgument));
            }
        }

        public string BuildStringRepresentation(bool skipParentheses)
        {
            var sb = new StringBuilder();

            if (!skipParentheses)
            {
                sb.Append("[");
            }

            sb.Append(Regex.Replace(AttributeTypeInfo.DisplayName, "(.+)Attribute", "$1"));

            if (ConstructorArguments.Any()
                || NamedArguments.Any())
            {
                sb.Append("(");                
            }

            if (ConstructorArguments.Any())
            {
                sb.Append(string.Join(", ", ConstructorArguments.Select(x => x.BuildStringRepresentation())));

                if (NamedArguments.Any())
                {
                    sb.Append(", ");
                }
            }

            if (NamedArguments.Any())
            {
                sb.Append(string.Join(", ", NamedArguments.Select(x => x.BuildStringRepresentation())));
            }
            
            if (ConstructorArguments.Any()
                || NamedArguments.Any())
            {
                sb.Append(")");                
            }

            if (!skipParentheses)
                sb.Append("]");

            return sb.ToString();
        }
    }
}