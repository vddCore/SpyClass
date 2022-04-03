using System;

namespace SpyClass.DataModel.Documentation
{
    public class ClassDoc : TypeDoc
    {
        public ClassDoc(Type documentedType) 
            : base(documentedType, TypeKind.Class)
        {
        }
    }
}