using System.Collections.Generic;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class GenericParameter : DocComponent
    {
        public string Name { get; private set; }
        
        public bool IsIn { get; private set; }
        public bool IsOut { get; private set; }

        public AttributeList Attributes { get; private set; } = AttributeList.Empty;
        
        public List<GenericParameterModifier> Modifiers { get; private set; } = new();
        public List<TypeInfo> Constraints { get; private set; }

        public GenericParameter(Mono.Cecil.GenericParameter parameter)
        {
            AnalyzeGenericParameter(parameter);
        }

        private void AnalyzeGenericParameter(Mono.Cecil.GenericParameter parameter)
        {
            Name = parameter.Name;

            if (parameter.HasCustomAttributes)
            {
                Attributes = new AttributeList(parameter.CustomAttributes);
            }

            if (parameter.HasConstraints)
            {
                Constraints = new List<TypeInfo>();

                foreach (var constraint in parameter.Constraints)
                {
                    Constraints.Add(new TypeInfo(constraint.ConstraintType));
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