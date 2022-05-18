using System.Text;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation.Base;

namespace SpyClass.Analysis.DataModel.Documentation.Components
{
    public class TypeInfo : DocComponent
    {
        public string FullName { get; private set; }
        public string DisplayName { get; private set; }

        public GenericArgumentList GenericArguments { get; private set; }

        public TypeInfo(TypeReference typeReference) 
        {
            AnalyzeTypeReference(typeReference);
        }

        private void AnalyzeTypeReference(TypeReference typeReference)
        {
            FullName = typeReference.FullName;
            DisplayName = NameTools.MakeDocFriendlyName(FullName, true);

            if (typeReference.IsGenericInstance)
            {
                GenericArguments = new GenericArgumentList(typeReference as GenericInstanceType);
            }
        }

        public string BuildStringRepresentation()
        {
            var sb = new StringBuilder();

            sb.Append(DisplayName);

            if (GenericArguments != null)
            {
                sb.Append(GenericArguments.BuildStringRepresentation(false));
            }
            
            return sb.ToString();
        }
    }
}