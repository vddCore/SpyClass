using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using SpyClass.Analysis.DataModel.Documentation.Components;

namespace SpyClass.Analysis.DataModel.Documentation
{
    public sealed class EnumDoc : TypeDoc
    {
        public new List<EnumField> Fields { get; } = new();

        public string UnderlyingTypeFullName { get; private set; }
        public string UnderlyingTypeAlias { get; private set; }

        internal EnumDoc(TypeDefinition documentedType)
            : base(documentedType, TypeKind.Enum)
        {
            AnalyzeBaseType();
            AnalyzeFields();
        }

        private void AnalyzeBaseType()
        {
            var underlyingType = DocumentedType.GetEnumUnderlyingType();

            UnderlyingTypeFullName = underlyingType.FullName;
            UnderlyingTypeAlias = NameTools.MakeDocFriendlyName(UnderlyingTypeFullName, false);
        }

        private void AnalyzeFields()
        {
            foreach(var field in DocumentedType.Fields)
            {
                if (field.Name == "value__")
                    continue;
                
                Fields.Add(new EnumField(this, field));
            }
        }

        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();
            var indentation = "".PadLeft(indent, ' ');

            sb.Append(indentation);
            sb.Append(BuildBasicIdentityString(this));
            sb.Append(BuildDisplayNameString());
            
            if (UnderlyingTypeAlias != "int")
            {
                sb.Append($" : {UnderlyingTypeAlias}");
            }
            
            sb.AppendLine();
            sb.AppendLine(indentation + "{");

            for (var i = 0; i < Fields.Count; i++)
            {
                var fld = Fields[i];

                sb.Append(fld.BuildStringRepresentation(indent));

                if (i + 1 < Fields.Count)
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