using System.Collections.Generic;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class GenericParameter
    {
        public TypeDoc Owner { get; }
        
        public string Name { get; private set; }
        
        public bool IsIn { get; private set; }
        public bool IsOut { get; private set; }

        public List<GenericParameterModifier> Modifiers { get; private set; } = new();
        public List<TypeDoc> Constraints { get; private set; }

        public GenericParameter(ModuleDefinition module, TypeDoc owner, Mono.Cecil.GenericParameter parameter)
        {
            Owner = owner;

            AnalyzeGenericParameter(module, parameter);
        }

        private void AnalyzeGenericParameter(ModuleDefinition module, Mono.Cecil.GenericParameter parameter)
        {
            Name = parameter.Name;

            if (parameter.HasConstraints)
            {
                Constraints = new List<TypeDoc>();

                foreach (var constraint in parameter.Constraints)
                {
                    Constraints.Add(
                        TypeDoc.FromType(module, constraint.ConstraintType.Resolve())
                    );
                }
            }
            
            if (parameter.Attributes.HasFlag(GenericParameterAttributes.Contravariant))
            {
                IsIn = true;
            }

            if (parameter.Attributes.HasFlag(GenericParameterAttributes.Covariant))
            {
                IsOut = true;
            }
            
            var hasNullableAttribute = false;
            var hasUnmanagedAttribute = false;
                
            if (parameter.HasCustomAttributes)
            {
                foreach (var customAttribute in parameter.CustomAttributes)
                {
                    if (customAttribute.AttributeType.FullName ==
                        "System.Runtime.CompilerServices.NullableAttribute")
                    {
                        hasNullableAttribute = true;
                    }
                    else if (customAttribute.AttributeType.FullName ==
                             "System.Runtime.CompilerServices.IsUnmanagedAttribute")
                    {
                        hasUnmanagedAttribute = true;
                    }
                }
            }

            if (hasNullableAttribute)
            {
                Modifiers.Add(GenericParameterModifier.NotNull);
            }

            if (parameter.Attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                Constraints.RemoveAll(x => x.FullName == "System.ValueType");

                if (hasUnmanagedAttribute)
                {
                    Modifiers.Add(GenericParameterModifier.Unmanaged);
                }
                else
                {
                    Modifiers.Add(GenericParameterModifier.Struct);
                }
            }

            if (parameter.Attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                Modifiers.Add(GenericParameterModifier.Class);
            }
            
            if (parameter.Attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                if (!Modifiers.Contains(GenericParameterModifier.Struct) &&
                    !Modifiers.Contains(GenericParameterModifier.Unmanaged))
                {
                    Modifiers.Add(GenericParameterModifier.New);
                }
            }
        }
    }
}