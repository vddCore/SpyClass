using System.IO;
using Mono.Cecil;

namespace SpyClass.DataModel.Documentation
{
    public abstract class DocPart
    {
        protected ModuleDefinition Module { get; }
        
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

        public DocPart(ModuleDefinition module)
        {
            Module = module;
        }

        protected virtual string BuildStringRepresentation(int indent)
        {
            return string.Empty;
        }
    }
}