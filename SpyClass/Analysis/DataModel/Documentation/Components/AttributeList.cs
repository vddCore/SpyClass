using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class AttributeList : DocComponent
    {
        private readonly HashSet<string> _ignoredAttributeTypes = new()
        {
            "System.ParamArrayAttribute",
            "System.Runtime.CompilerServices.NullableAttribute",
            "System.Runtime.CompilerServices.IsUnmanagedAttribute",
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
            "System.Runtime.CompilerServices.ExtensionAttribute",
            "System.Runtime.CompilerServices.IsReadOnlyAttribute"
        };
        
        private readonly List<AttributeComponent> _attributes = new();

        public static readonly AttributeList Empty = new(); 

        public IReadOnlyList<AttributeComponent> Attributes => _attributes;
        
        public bool Any => Attributes.Count > 0;

        private AttributeList()
        {
        }
        
        public AttributeList(Mono.Collections.Generic.Collection<CustomAttribute> attributes) 
        {
            AnalyzeAttributes(attributes);
        }

        public void RemoveAll(Predicate<AttributeComponent> match)
        {
            _attributes.RemoveAll(match);
        }

        private void AnalyzeAttributes(Mono.Collections.Generic.Collection<CustomAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (_ignoredAttributeTypes.Contains(attribute.AttributeType.FullName))
                    continue;
                
                _attributes.Add(new AttributeComponent(attribute));
            }
        }

        public string BuildStringRepresentation(bool singleLine)
        {
            var sb = new StringBuilder();

            if (singleLine)
            {
                sb.Append("[");
                sb.Append(string.Join(", ", Attributes.Select(x => x.BuildStringRepresentation(true))));
                sb.Append("]");
            }
            else
            {
                foreach (var attribute in Attributes)
                {
                    sb.AppendLine(attribute.BuildStringRepresentation(false));
                }
            }

            return sb.ToString();
        }
    }
}