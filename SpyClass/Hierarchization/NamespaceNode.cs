using System.Collections.Generic;
using System.Linq;

namespace SpyClass.Hierarchization
{
    public class NamespaceNode : ContainerNode
    {
        public string Name { get; }

        public NamespaceNode(string name)
        {
            Name = name;
        }
    }
}