using System.Linq;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public class DelegateDoc : TypeDoc
    {
        public string ReturnTypeFullName { get; private set; }
        public string ReturnTypeDisplayName { get; private set; }
        
        public MethodParameterList MethodParameters { get; private set; }

        public DelegateDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Delegate)
        {
            AnalyzeDelegate();
        }

        private void AnalyzeDelegate()
        {
            var invokeMethod = DocumentedType.Methods.First(x => x.Name == "Invoke");

            ReturnTypeFullName = invokeMethod.ReturnType.FullName;
            ReturnTypeDisplayName = NameTools.MakeDocFriendlyName(ReturnTypeFullName, false);
            MethodParameters = new MethodParameterList(invokeMethod.Parameters, false);
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            sb.Append(indentation);
            sb.Append(BuildBasicIdentityString());
            sb.Append(" ");
            sb.Append(ReturnTypeDisplayName);
            sb.Append(" ");
            sb.Append(DisplayName);

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
    }
}