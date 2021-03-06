using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Collections.Generic;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class GenericParameterList : DocComponent
    {
        private readonly List<GenericParameter> _parameters = new();

        public static readonly GenericParameterList Empty = new();
        
        public bool Any => Parameters.Count > 0;

        public IReadOnlyList<GenericParameter> Parameters => _parameters;
        
        private GenericParameterList()
        {
        }
        
        public GenericParameterList(Collection<Mono.Cecil.GenericParameter> cecilParameters)
        {
            AnalyzeParameters(cecilParameters);
        }

        private void AnalyzeParameters(Collection<Mono.Cecil.GenericParameter> cecilParameters)
        {
            foreach (var parameter in cecilParameters)
            {
                _parameters.Add(new GenericParameter(parameter));
            }
        }

        public string BuildGenericParameterListString()
        {
            var sb = new StringBuilder();

            if (Any)
            {
                sb.Append("<");

                for (var i = 0; i < Parameters.Count; i++)
                {
                    var param = Parameters[i];

                    if (param.Attributes != null)
                    {
                        sb.Append(param.Attributes.BuildStringRepresentation(true));
                        sb.Append(" ");
                    }
                    
                    if (param.IsIn)
                    {
                        sb.Append("in ");
                    }
                    else if (param.IsOut)
                    {
                        sb.Append("out ");
                    }

                    sb.Append(param.Name);
                    if (i < Parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(">");
            }
            
            return sb.ToString();
        }

        public string BuildGenericParameterConstraintString()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Parameters.Count; i++)
            {
                var param = Parameters[i];
                var constraintList = new List<string>();
                
                if (param.Constraints != null)
                {
                    foreach (var constraint in param.Constraints)
                    {
                        constraintList.Add(constraint.BuildStringRepresentation());
                    }
                }
                
                if (param.Modifiers.Any())
                {
                    foreach (var mod in param.Modifiers)
                    {
                        constraintList.Add(mod switch
                        {
                            GenericParameterModifier.Class => "class",
                            GenericParameterModifier.Default => "default",
                            GenericParameterModifier.New => "new()",
                            GenericParameterModifier.Struct => "struct",
                            GenericParameterModifier.Unmanaged => "unmanaged",
                            GenericParameterModifier.NotNull => "notnull",
                            _ => throw new InvalidOperationException($"Unknown generic parameter modifier {mod}.")
                        });
                    }
                }

                if (constraintList.Any())
                {
                    sb.Append($" where {param.Name}: {string.Join(", ", constraintList)}");
                }
            }
            
            return sb.ToString();
        }
    }
}