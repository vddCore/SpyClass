using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class AttributeList : DocComponent
    {
        private HashSet<string> IgnoredAttributeTypes { get; } = new()
        {
            "System.ParamArrayAttribute",
            "System.Runtime.CompilerServices.NullableAttribute",
            "System.Runtime.CompilerServices.IsUnmanagedAttribute",
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
            "System.Runtime.CompilerServices.ExtensionAttribute"
        };
            
        public List<AttributeComponent> Attributes { get; private set; } = new();

        public AttributeList(ModuleDefinition module, Mono.Collections.Generic.Collection<CustomAttribute> attributes) 
            : base(module)
        {
            AnalyzeAttributes(attributes);
        }

        private void AnalyzeAttributes(Mono.Collections.Generic.Collection<CustomAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (IgnoredAttributeTypes.Contains(attribute.AttributeType.FullName))
                    continue;
                
                Attributes.Add(new AttributeComponent(Module, attribute));
            }
        }

        public string BuildStringRepresentation(bool singleLine)
        {
            var sb = new StringBuilder();

            if (singleLine)
            {
                sb.Append("[");
                sb.Append(string.Join(", ", Attributes.Select(x => x.BuildStringRepresentation())));
                sb.Append("]");
            }
            else
            {
                foreach (var attribute in Attributes)
                {
                    sb.AppendLine("[" + attribute.BuildStringRepresentation() + "]");
                }
            }

            return sb.ToString();
        }
    }
}