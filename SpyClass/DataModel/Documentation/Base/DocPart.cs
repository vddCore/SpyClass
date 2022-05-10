using System.IO;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation.Base
{
    public abstract class DocPart : DocComponent
    {        
        public AccessModifier Access { get; protected set; }

        public string AccessModifierString => Access switch
        {
            AccessModifier.Public => "public",
            AccessModifier.Protected => "protected",
            AccessModifier.Internal => "internal",
            AccessModifier.ProtectedInternal => "protected internal",
            AccessModifier.Private => "private",
            AccessModifier.PrivateProtected => "private protected",
            _ => throw new InvalidDataException($"Unexpected access modifier {Access}")
        };
        
        protected DocPart(ModuleDefinition module) 
            : base(module)
        {
        }

        protected virtual string BuildStringRepresentation(int indent)
        {
            return string.Empty;
        }
    }
}