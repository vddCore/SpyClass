using System;
using System.Collections.Generic;
using System.Linq;
using SpyClass.Analysis.DataModel;

namespace SpyClass.Hierarchization
{
    public abstract class ContainerNode : DocTreeNode
    {
        public List<DocTreeNode> Children { get; } = new();

        public void Sort()
        {
            Children.Sort((a, b) =>
            {
                if (a is NamespaceNode)
                    return -1;

                if (a is TypeNode ta && b is TypeNode tb)
                {
                    return string.Compare(ta.TypeDoc.Name, tb.TypeDoc.Name, StringComparison.InvariantCulture);
                }
                
                return 1;
            });
        }
        
        public ContainerNode CreateByNamespace(string namePath)
        {
            var path = new Stack<string>(namePath.Split('.').Reverse());

            var ns = this;
            while (path.Any())
            {
                var childName = path.Pop();
                var node =
                    ns.Children.FirstOrDefault(x => x is NamespaceNode nsn && nsn.Name == childName) as NamespaceNode;

                if (node == null)
                {
                    node = new NamespaceNode(childName);
                    ns.Children.Add(node);
                }

                ns = node;
            }

            return ns;
        }

        public bool FindChildContainer(string namePath, out ContainerNode outNode)
        {
            var path = new Stack<string>(namePath.Split('.').Reverse());
            var currentNode = this;

            _restart:
            foreach (var treeNode in currentNode.Children)
            {
                if (treeNode is not NamespaceNode ns)
                    continue;

                if (ns.Name == path.Peek())
                {
                    currentNode = ns;
                    path.Pop();

                    if (path.Any())
                    {
                        goto _restart;
                    }
                    else
                    {
                        outNode = currentNode;
                        return true;
                    }
                }
            }

            outNode = null;
            return false;
        }
    }
}