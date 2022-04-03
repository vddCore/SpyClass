using System;
using System.Collections.Generic;
using System.Text;

namespace SpyClass.DataModel.Documentation
{
    public sealed class EnumDoc : TypeDoc
    {
        private List<EnumField> _fields = new();

        public IReadOnlyList<EnumField> Fields => _fields;

        public string UnderlyingTypeFullName { get; private set; }
        public string UnderlyingTypeAlias { get; private set; }

        internal EnumDoc(Type documentedType)
            : base(documentedType, TypeKind.Enum)
        {
            AnalyzeBaseType();
            AnalyzeFields();
        }

        private void AnalyzeBaseType()
        {
            var underlyingType = Enum.GetUnderlyingType(DocumentedType);

            UnderlyingTypeFullName = underlyingType.FullName;
            UnderlyingTypeAlias = TryAliasTypeName(UnderlyingTypeFullName);
        }

        private void AnalyzeFields()
        {
            var names = DocumentedType.GetEnumNames();
            var values = DocumentedType.GetEnumValues();
            var underlyingType = DocumentedType.GetEnumUnderlyingType();
            
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                
                // todo attributes
                var member = DocumentedType.GetMember(name)[0];
                
                _fields.Add(new(
                    name,
                    Convert.ChangeType(
                        values.GetValue(i)!,
                        underlyingType
                    )!.ToString()
                ));
            }
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();

            var indentation = "".PadLeft(indent, ' ');

            sb.Append(indentation + AccessModifierString);
            sb.Append(" enum ");
            sb.Append(DisplayName);

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