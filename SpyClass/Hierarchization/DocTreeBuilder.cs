using System.Collections.Generic;
using SpyClass.Analysis.DataModel.Documentation;

namespace SpyClass.Hierarchization
{
    public class DocTreeBuilder
    {
        private readonly List<TypeDoc> _docs;
        private readonly RootNode _root;

        public DocTreeBuilder(List<TypeDoc> docs)
        {
            _docs = docs;
            _root = new RootNode();
        }

        public RootNode Build()
        {
            foreach (var doc in _docs)
            {
                if (!_root.FindChildContainer(doc.Namespace, out var ns))
                {
                    ns = _root.CreateByNamespace(doc.Namespace);
                }

                ns.Children.Add(new TypeNode(doc));
            }
            
            return _root;
        }
    }
}