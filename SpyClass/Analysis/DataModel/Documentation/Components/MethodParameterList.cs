using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Collections.Generic;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class MethodParameterList : DocComponent
    {
        private List<MethodParameter> _parameters = new();

        public static readonly MethodParameterList Empty = new();
        
        public bool Any => Parameters.Count > 0;

        public IReadOnlyList<MethodParameter> Parameters => _parameters;

        private MethodParameterList()
        {
        }
        
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
                
                _parameters.Add(new MethodParameter(parameter));
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