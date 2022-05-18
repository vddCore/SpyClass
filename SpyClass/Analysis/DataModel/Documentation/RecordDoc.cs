using System.Collections.Generic;
using Mono.Cecil;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public class RecordDoc : TypeDoc
    {
        protected override HashSet<string> IgnoredValidMethodNames => new()
        {
            "<Clone>$",
            "op_Inequality",
            "op_Equality",
        };

        public RecordDoc(TypeDefinition documentedType) 
            : base(documentedType, TypeKind.Record)
        {
        }
    }
}