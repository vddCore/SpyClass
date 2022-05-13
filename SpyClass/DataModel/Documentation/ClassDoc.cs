using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public sealed class ClassDoc : TypeDoc
    {
        public ClassDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Class)
        {
        }
    }
}