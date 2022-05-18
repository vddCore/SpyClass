using Mono.Cecil;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public class InterfaceDoc : TypeDoc
    {
        public InterfaceDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Interface)
        {
        }
    }
}