using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public class StructDoc : TypeDoc
    {
        public bool IsReadOnly { get; private set; }

        public StructDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Struct)
        {
            AnalyzeStruct();
        }

        private void AnalyzeStruct()
        {
            if (DocumentedType.HasCustomAttributes)
            {
                if (DocumentedType.CustomAttributes.Any(x =>x.AttributeType.FullName == typeof(IsReadOnlyAttribute).FullName))
                    IsReadOnly = true;
            }
        }
    }
}