using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class GenericArgumentList : DocComponent
    {
        public bool Any => Arguments.Count > 0;

        public List<TypeInfo> Arguments { get; } = new();

        public GenericArgumentList(ModuleDefinition module, GenericInstanceType genericInstanceType) 
            : base(module)
        {
            AnalyzeGenericArguments(genericInstanceType);
        }

        private void AnalyzeGenericArguments(GenericInstanceType genericInstanceType)
        {
            if (genericInstanceType.HasGenericArguments)
            {
                foreach (var genericArgument in genericInstanceType.GenericArguments)
                {
                    Arguments.Add(new TypeInfo(Module, genericArgument));
                }
            }
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            if (Any)
            {
                sb.Append("<");
                sb.Append(string.Join(", ", Arguments.Select(x => x.BuildStringRepresentation())));
                sb.Append(">");
            }
            
            return sb.ToString();
        }
    }
}