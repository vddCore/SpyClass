using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public class TypeInfo : DocComponent
    {
        public string FullName { get; private set; }
        public string DisplayName { get; private set; }

        public GenericArgumentList GenericArguments { get; private set; }

        public TypeInfo(ModuleDefinition module, TypeReference typeReference) 
            : base(module)
        {
            AnalyzeTypeReference(typeReference);
        }

        private void AnalyzeTypeReference(TypeReference typeReference)
        {
            FullName = typeReference.FullName;
            DisplayName = NameTools.MakeDocFriendlyName(FullName, true);

            if (typeReference.IsGenericInstance)
            {
                GenericArguments = new GenericArgumentList(Module, typeReference as GenericInstanceType);
            }
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            sb.Append(DisplayName);

            if (GenericArguments != null)
            {
                sb.Append(GenericArguments.BuildStringRepresentation());
            }
            
            return sb.ToString();
        }
    }
}