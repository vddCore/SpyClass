using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public class AttributeComponent : DocComponent
    {
        public TypeInfo AttributeTypeInfo { get; private set; }

        public List<AttributeArgument> ConstructorArguments { get; private set; } = new();
        public List<AttributeArgument> NamedArguments { get; private set; } = new();

        public AttributeComponent(ModuleDefinition module, CustomAttribute customAttribute)
            : base(module)
        {
            AnalyzeCustomAttribute(customAttribute);
        }

        private void AnalyzeCustomAttribute(CustomAttribute customAttribute)
        {
            AttributeTypeInfo = new TypeInfo(Module, customAttribute.AttributeType);
            
            foreach (var constructorArgument in customAttribute.ConstructorArguments)
            {
                ConstructorArguments.Add(new AttributeArgument(Module, constructorArgument));
            }

            foreach (var fieldArgument in customAttribute.Fields)
            {
                NamedArguments.Add(new AttributeArgument(Module, fieldArgument));
            }

            foreach (var propertyArgument in customAttribute.Properties)
            {
                NamedArguments.Add(new AttributeArgument(Module, propertyArgument));
            }
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();
            sb.Append(AttributeTypeInfo.DisplayName);

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

            return sb.ToString();
        }
    }
}