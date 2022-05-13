using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class InterfaceDoc : TypeDoc
    {
        public InterfaceDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Interface)
        {
        }
    }
}