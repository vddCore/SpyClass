using System.Collections.Generic;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public class RecordDoc : TypeDoc
    {
        protected override HashSet<string> IgnoredValidMethodNames => new()
        {
            "<Clone>$",
            "op_Inequality",
            "op_Equality",
        };

        public RecordDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Record)
        {
        }
    }
}