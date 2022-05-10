using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class MethodParameter : DocComponent
    {
        public string Name { get; private set; }
        public string TypeFullName { get; private set; }
        public string TypeDisplayName => TypeDoc.TryAliasTypeName(TypeFullName);

        public MethodParameter(ModuleDefinition module, ParameterDefinition parameter)
            : base(module)
        {
            AnalyzeParameter(parameter);
        }

        private void AnalyzeParameter(ParameterDefinition parameter)
        {
            Name = parameter.Name;
            TypeFullName = parameter.ParameterType.FullName;
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            sb.Append(TypeDisplayName);
            sb.Append(" ");
            sb.Append(Name);

            return sb.ToString();
        }
    }
}