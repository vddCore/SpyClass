using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class GenericArgumentList : DocComponent
    {
        private readonly List<TypeInfo> _arguments = new();
        
        public static readonly GenericArgumentList Empty = new();
        
        public bool Any => Arguments.Count > 0;

        public IReadOnlyList<TypeInfo> Arguments => _arguments;
        
        private GenericArgumentList()
        {
        }

        public GenericArgumentList(GenericInstanceType genericInstanceType)
        {
            AnalyzeGenericArguments(genericInstanceType);
        }

        private void AnalyzeGenericArguments(GenericInstanceType genericInstanceType)
        {
            if (genericInstanceType.HasGenericArguments)
            {
                foreach (var genericArgument in genericInstanceType.GenericArguments)
                {
                    _arguments.Add(new TypeInfo(genericArgument));
                }
            }
        }

        public string BuildStringRepresentation(bool skipParentheses)
        {
            var sb = new StringBuilder();

            if (Any)
            {
                if (!skipParentheses)
                    sb.Append("<");

                sb.Append(string.Join(", ", Arguments.Select(x => x.BuildStringRepresentation())));

                if (!skipParentheses)
                    sb.Append(">");
            }

            return sb.ToString();
        }
    }
}