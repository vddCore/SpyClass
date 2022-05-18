using System;
using System.IO;
using HtmlAgilityPack;
using SpyClass.Analysis.DataModel.Documentation;
using SpyClass.Hierarchization;
using SpyClass.Rendering.HtmlRendering.Utils;

namespace SpyClass.Rendering.HtmlRendering
{
    public class HtmlRenderer : DocTreeRenderer
    {
        private HtmlDocument _indexDocument;
        private HtmlDocument _currentContentDocument;

        private HtmlNode _navtreeRootNode;
        private HtmlNode _currentNavtreeNode;

        private string _outDirectory;

        public HtmlRenderer(string outDirectory)
        {
            _outDirectory = outDirectory;

            CreateOutDirectory();
            LoadTemplate();
        }

        private void CreateOutDirectory()
        {
            if (Directory.Exists(_outDirectory))
                Directory.Delete(_outDirectory, true);

            Directory.CreateDirectory(_outDirectory);
            FileSystem.CopyDirectory(Path.Combine(AppContext.BaseDirectory, "SpyClass.WebTemplate"), _outDirectory);
        }

        private void LoadTemplate()
        {
            _indexDocument = new HtmlDocument();
            _indexDocument.Load(Path.Combine(_outDirectory, "index.html"));

            _navtreeRootNode = _indexDocument.DocumentNode.SelectSingleNode("//[@role='tree']");
        }

        protected override void Visit(NamespaceNode node)
        {
            base.Visit(node);
        }

        protected override void Visit(TypeDoc typeDoc)
        {
        }

        protected override void Visit(ClassDoc classDoc)
        {
        }

        protected override void Visit(DelegateDoc delegateDoc)
        {
        }

        protected override void Visit(EnumDoc enumDoc)
        {
        }

        protected override void Visit(InterfaceDoc interfaceDoc)
        {
        }

        protected override void Visit(RecordDoc recordDoc)
        {
        }

        protected override void Visit(StructDoc structDoc)
        {
        }

        protected override void Visit(FieldDoc fieldDoc)
        {
        }

        protected override void Visit(EventDoc eventDoc)
        {
        }

        protected override void Visit(IndexerDoc indexerDoc)
        {
        }

        protected override void Visit(PropertyDoc propertyDoc)
        {
        }

        protected override void Visit(ConstructorDoc constructorDoc)
        {
        }

        protected override void Visit(MethodDoc methodDoc)
        {
        }
    }
}