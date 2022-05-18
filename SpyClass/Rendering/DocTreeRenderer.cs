using System;
using SpyClass.Analysis.DataModel.Documentation;
using SpyClass.Hierarchization;

namespace SpyClass.Rendering
{
    public abstract class DocTreeRenderer
    {
        private void Visit(DocTreeNode node)
        {
            if (node is NamespaceNode nsn)
                Visit(nsn);
            else if (node is TypeNode tpn)
                Visit(tpn);
            else throw new NotSupportedException($"'{node.GetType()}' is not supported.");
        }

        public void Render(RootNode root)
        {
            OnRender(root);
        }

        protected virtual void OnRender(RootNode root)
        {
            root.Sort();
            
            foreach (var child in root.Children)
            {
                if (child is ContainerNode cn)
                    cn.Sort();
                
                Visit(child);
            }
        }

        protected virtual void Visit(NamespaceNode node)
        {
            foreach (var child in node.Children)
            {
                if (child is ContainerNode cn)
                    cn.Sort();
                
                Visit(child);
            }
        }
        
        protected virtual void Visit(TypeNode typeNode)
        {
            if (typeNode.TypeDoc is ClassDoc cd)
                Visit(cd);
            else if (typeNode.TypeDoc is DelegateDoc dd)
                Visit(dd);
            else if (typeNode.TypeDoc is EnumDoc ed)
                Visit(ed);
            else if (typeNode.TypeDoc is InterfaceDoc id)
                Visit(id);
            else if (typeNode.TypeDoc is RecordDoc rd)
                Visit(rd);
            else if (typeNode.TypeDoc is StructDoc sd)
                Visit(sd);
            else
            {
                Visit(typeNode.TypeDoc);
            }
        }

        protected abstract void Visit(TypeDoc typeDoc);
        protected abstract void Visit(ClassDoc classDoc);
        protected abstract void Visit(DelegateDoc delegateDoc);
        protected abstract void Visit(EnumDoc enumDoc);
        protected abstract void Visit(InterfaceDoc interfaceDoc);
        protected abstract void Visit(RecordDoc recordDoc);
        protected abstract void Visit(StructDoc structDoc);
        protected abstract void Visit(FieldDoc fieldDoc);
        protected abstract void Visit(EventDoc eventDoc);
        protected abstract void Visit(IndexerDoc indexerDoc);
        protected abstract void Visit(PropertyDoc propertyDoc);
        protected abstract void Visit(ConstructorDoc constructorDoc);
        protected abstract void Visit(MethodDoc methodDoc);
    }
}