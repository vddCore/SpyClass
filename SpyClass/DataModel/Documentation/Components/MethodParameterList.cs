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

        public MethodParameterList(Collection<ParameterDefinition> parameters, bool ignoreValue)
        {
            AnalyzeParameters(parameters, ignoreValue);
        }

        private void AnalyzeParameters(Collection<ParameterDefinition> parameters, bool ignoreValue)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Name == "value" && ignoreValue)
                    continue;
                
                Parameters.Add(new MethodParameter(parameter));
            }
        }

        public string BuildStringRepresentation(bool skipParentheses)
        {
            var sb = new StringBuilder();

            if (!skipParentheses)
                sb.Append("(");

            sb.Append(string.Join(", ", Parameters.Select(x => x.BuildStringRepresentation())));

            if (!skipParentheses)
                sb.Append(")");

            return sb.ToString();
        }
    }
}