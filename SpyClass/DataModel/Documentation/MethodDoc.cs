using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public class MethodDoc : DocPart
    {
        public TypeDoc Owner { get; }
        public MethodModifiers Modifiers { get; }

        public string Name { get; private set; }
        
        public TypeInfo ReturnTypeInfo { get; private set; }

        public AttributeList Attributes { get; private set; }
        public GenericParameterList GenericParameters { get; private set; }
        public MethodParameterList MethodParameters { get; private set; }

        public bool IsOverride { get; private set; }

        public MethodDoc(TypeDoc owner, MethodDefinition method)
        {
            Owner = owner;

            Modifiers = DetermineModifiers(method);
            Access = DetermineAccess(method);

            AnalyzeMethod(method);
        }

        private void AnalyzeMethod(MethodDefinition method)
        {
            Name = method.Name;
            ReturnTypeInfo = new TypeInfo(method.ReturnType);

            if (method.HasCustomAttributes)
            {
                Attributes = new AttributeList(method.CustomAttributes);
            }

            GenericParameters = new GenericParameterList(method.GenericParameters);
            MethodParameters = new MethodParameterList(method.Parameters, true);

            if (method.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ExtensionAttribute).FullName))
            {
                MethodParameters.Parameters[0].MarkAsThisParameter();
            }

            IsOverride = method.IsVirtual && !method.IsNewSlot;
        }

        public static MethodModifiers DetermineModifiers(MethodDefinition method)
        {
            MethodModifiers ret = 0;

            if (method.IsStatic)
            {
                ret |= MethodModifiers.Static;
            }

            if (method.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(DllImportAttribute).FullName)
                && !method.HasBody)
            {
                ret |= MethodModifiers.Extern;
            }
            else if (!method.DeclaringType.IsInterface)
            {
                if (method.IsAbstract)
                {
                    ret |= MethodModifiers.Abstract;
                }
                else if (method.IsVirtual)
                {
                    ret |= MethodModifiers.Virtual;
                }
            }

            return ret;
        }

        public static AccessModifier DetermineAccess(MethodDefinition method)
        {
            if (method.IsPublic)
            {
                return AccessModifier.Public;
            }
            else if (method.IsFamily)
            {
                return AccessModifier.Protected;
            }
            else if (method.IsAssembly)
            {
                return AccessModifier.Internal;
            }
            else if (method.IsFamilyOrAssembly)
            {
                return AccessModifier.ProtectedInternal;
            }
            else if (method.IsPrivate)
            {
                return AccessModifier.Private;
            }
            else if (method.IsFamilyAndAssembly)
            {
                return AccessModifier.PrivateProtected;
            }

            throw new NotSupportedException($"Cannot determine type access: {method.Name}");
        }

        private string BuildMethodModifierString()
        {
            var sb = new StringBuilder();

            if (Modifiers.HasFlag(MethodModifiers.Abstract))
            {
                sb.Append(" abstract");
            }

            if (Modifiers.HasFlag(MethodModifiers.Static))
            {
                sb.Append(" static");
            }

            if (IsOverride)
            {
                sb.Append(" override");
            }
            else if (Modifiers.HasFlag(MethodModifiers.Virtual))
            {
                sb.Append(" virtual");
            }

            return sb.ToString();
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            
            var indentation = "".PadLeft(indent, ' ');

            if (Attributes != null)
            {
                foreach (var attrib in Attributes.Attributes)
                {
                    sb.AppendLine(indentation + "[" + attrib.BuildStringRepresentation() + "]");
                }
            }
            
            sb.Append(indentation);
            sb.Append(AccessModifierString);
            sb.Append(BuildMethodModifierString());
            sb.Append(" ");
            sb.Append(ReturnTypeInfo.BuildStringRepresentation());
            sb.Append(" ");
            sb.Append(Name);

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterListString());
            }

            sb.Append(MethodParameters.BuildStringRepresentation(false));

            if (GenericParameters != null)
            {
                sb.Append(GenericParameters.BuildGenericParameterConstraintString());
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}