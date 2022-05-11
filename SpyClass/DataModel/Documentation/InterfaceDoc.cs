using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class InterfaceDoc : TypeDoc
    {
        public InterfaceDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Interface)
        {
        }
    }
}