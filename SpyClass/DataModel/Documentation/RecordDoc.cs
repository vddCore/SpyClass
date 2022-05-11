using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class RecordDoc : TypeDoc
    {
        public RecordDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Record)
        {
        }
    }
}