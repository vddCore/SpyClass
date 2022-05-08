using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public sealed class ClassDoc : TypeDoc
    {
        private List<TypeField> _fields = new();

        public IReadOnlyList<TypeField> Fields => _fields;

        public ClassDoc(ModuleDefinition module, TypeDefinition documentedType) 
            : base(module, documentedType, TypeKind.Class)
        {
            AnalyzeFields();            
        }

        private void AnalyzeFields()
        {
            var fields = DocumentedType.Fields;

            foreach (var info in fields)
            {
                _fields.Add(new TypeField(Module, info, this));
            }
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');
            
            sb.Append(base.BuildStringRepresentation(indent));
            sb.AppendLine();
            sb.AppendLine("{");

            foreach (var field in _fields)
            {
                sb.AppendLine(indentation + "    " + field);
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}