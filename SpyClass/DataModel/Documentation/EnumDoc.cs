using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using SpyClass.Extensions;

namespace SpyClass.DataModel.Documentation
{
    public sealed class EnumDoc : TypeDoc
    {
        private List<EnumField> _fields = new();

        public IReadOnlyList<EnumField> Fields => _fields;

        public string UnderlyingTypeFullName { get; private set; }
        public string UnderlyingTypeAlias { get; private set; }

        internal EnumDoc(ModuleDefinition module, TypeDefinition documentedType)
            : base(module, documentedType, TypeKind.Enum)
        {
            AnalyzeBaseType();
            AnalyzeFields();
        }

        private void AnalyzeBaseType()
        {
            var underlyingType = DocumentedType.GetEnumUnderlyingType();

            UnderlyingTypeFullName = underlyingType.FullName;
            UnderlyingTypeAlias = TryAliasTypeName(UnderlyingTypeFullName);
        }

        private void AnalyzeFields()
        {
            var names = DocumentedType.GetEnumNames();
            var values = DocumentedType.GetEnumValues();
            
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var value = values[i];
                
                // todo field attributes
                
                _fields.Add(
                    new(this, name, value)
                );
            }
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            sb.Append(base.BuildStringRepresentation(indent));
            
            if (UnderlyingTypeAlias != "int")
            {
                sb.Append($" : {UnderlyingTypeAlias}");
            }
            sb.AppendLine();
            sb.AppendLine(indentation + "{");

            for (var i = 0; i < _fields.Count; i++)
            {
                var fld = _fields[i];
                sb.Append(indentation + $"    {fld.Name} = {fld.Value}");

                if (i + 1 < _fields.Count)
                    sb.Append(",");

                sb.AppendLine();
            }

            sb.Append(indentation + "}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}