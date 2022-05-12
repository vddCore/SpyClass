using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public sealed class ClassDoc : TypeDoc
    {
        public ClassDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Class)
        {
        }
    }
}