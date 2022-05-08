using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class GenericParameterList
    {
        public TypeDoc Owner { get; }
        public List<GenericParameter> Parameters { get; } = new();

        public GenericParameterList(
            ModuleDefinition module,
            TypeDoc owner,
            Mono.Collections.Generic.Collection<Mono.Cecil.GenericParameter> cecilParameters)
        {
            Owner = owner;
            AnalyzeParameters(module, cecilParameters);
        }

        private void AnalyzeParameters(
            ModuleDefinition module,
            Mono.Collections.Generic.Collection<Mono.Cecil.GenericParameter> cecilParameters)
        {
            foreach (var parameter in cecilParameters)
            {
                Parameters.Add(new GenericParameter(module, Owner, parameter));
            }
        }

        public string BuildGenericParameterListString()
        {
            var sb = new StringBuilder();

            sb.Append("<");

            for (var i = 0; i < Parameters.Count; i++)
            {
                var param = Parameters[i];

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
                        constraintList.Add(constraint.DisplayName);
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