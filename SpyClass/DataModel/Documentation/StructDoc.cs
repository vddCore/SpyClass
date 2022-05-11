using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class StructDoc : TypeDoc
    {
        public StructDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Struct)
        {
        }
    }
}