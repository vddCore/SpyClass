using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class MethodParameter : DocComponent
    {
        public string Name { get; private set; }
        
        public string TypeFullName { get; private set; }
        public string TypeDisplayName { get; private set; }

        public string DefaultValueString { get; private set; }
        public ParameterModifiers Modifiers { get; private set; }

        public MethodParameter(ModuleDefinition module, ParameterDefinition parameter)
            : base(module)
        {
            AnalyzeParameter(parameter);
        }

        private void AnalyzeParameter(ParameterDefinition parameter)
        {
            Name = parameter.Name;
            
            TypeFullName = parameter.ParameterType.FullName;
            TypeDisplayName = NameTools.MakeDocFriendlyName(TypeFullName, false);
            
            if (parameter.IsOut)
            {
                Modifiers |= ParameterModifiers.Out;
            }
            else if (parameter.ParameterType.IsByReference)
            {
                Modifiers |= ParameterModifiers.Ref;
            }
            else if (parameter.CustomAttributes.Any(a => a.AttributeType.FullName == "System.ParamArrayAttribute"))
            {
                Modifiers |= ParameterModifiers.Params;
            }

            if (parameter.HasConstant)
            {
                if (parameter.Constant == null)
                {
                    DefaultValueString = "null";
                }
                else if (parameter.Constant.GetType().FullName == typeof(string).FullName)
                {
                    DefaultValueString = "\"" + parameter.Constant + "\"";
                }
                else
                {
                    DefaultValueString = parameter.Constant.ToString();
                }
            }
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            if (Modifiers.HasFlag(ParameterModifiers.Out))
            {
                sb.Append("out ");
            }
            else if (Modifiers.HasFlag(ParameterModifiers.Ref))
            {
                sb.Append("ref ");
            }

            if (Modifiers.HasFlag(ParameterModifiers.Params))
            {
                sb.Append("params ");
            }

            sb.Append(TypeDisplayName);
            sb.Append(" ");
            sb.Append(Name);

            if (DefaultValueString != null)
            {
                sb.Append(" = ");
                sb.Append(DefaultValueString);
            }

            return sb.ToString();
        }
    }
}