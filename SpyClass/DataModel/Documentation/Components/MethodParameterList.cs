using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Collections.Generic;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class MethodParameterList : DocComponent
    {
        public bool Any => Parameters.Count > 0;
        
        public List<MethodParameter> Parameters { get; } = new();

        public MethodParameterList(ModuleDefinition module, Collection<ParameterDefinition> parameters)
            : base(module)
        {
            AnalyzeParameters(parameters);
        }

        private void AnalyzeParameters(Collection<ParameterDefinition> parameters)
        {
            foreach (var parameter in parameters)
            {
                Parameters.Add(new MethodParameter(Module, parameter));
            }
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            sb.Append("(");
            sb.Append(string.Join(", ", Parameters.Select(x => x.BuildStringRepresentation())));
            sb.Append(")");
            
            return sb.ToString();
        }
    }
}