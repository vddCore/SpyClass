using Mono.Cecil;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public sealed class ClassDoc : TypeDoc
    {
        public ClassDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Class)
        {
        }
    }
}