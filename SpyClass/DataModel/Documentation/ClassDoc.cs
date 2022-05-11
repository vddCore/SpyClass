using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public sealed class ClassDoc : TypeDoc
    {
        public List<FieldDoc> Fields { get; } = new();

        public ClassDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Class)
        {
            AnalyzeFields();
        }

        private void AnalyzeFields()
        {
            foreach (var field in DocumentedType.Fields)
            {
                if (!field.IsPublic && !field.IsFamily && !Analyzer.Options.IncludeNonUserMembers)
                    continue;

                if (field.IsCompilerControlled && Analyzer.Options.IgnoreCompilerGeneratedTypes)
                    continue;
                
                Fields.Add(new FieldDoc(Module, field, this));
            }
        }

        protected override string BuildInnerContent(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            foreach (var field in Fields)
            {
                sb.AppendLine(indentation + "    " + field);
            }
            
            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}