using System.Collections.Generic;
using SpyClass.Analysis.DataModel.Documentation;

namespace SpyClass.Hierarchization
{
    public class TypeNode : ContainerNode
    {
        public TypeDoc TypeDoc { get; }

        public TypeNode(TypeDoc typeDoc)
        {
            TypeDoc = typeDoc;

            foreach (var nestedType in typeDoc.NestedTypes)
            {
                Children.Add(new TypeNode(nestedType));
            }
        }
    }
}