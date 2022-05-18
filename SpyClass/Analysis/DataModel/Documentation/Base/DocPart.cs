using System.IO;

namespace SpyClass.Analysis.DataModel.Documentation.Base
{
    public abstract class DocPart : DocComponent
    {
        public AccessModifier Access { get; protected set; }

        public string AccessModifierString => BuildAccessLevelString(Access);

        public static string BuildAccessLevelString(AccessModifier accessModifier)
        {
            return accessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Protected => "protected",
                AccessModifier.Internal => "internal",
                AccessModifier.ProtectedInternal => "protected internal",
                AccessModifier.Private => "private",
                AccessModifier.PrivateProtected => "private protected",
                _ => throw new InvalidDataException($"Unexpected access modifier {accessModifier}")
            };
        }

        protected virtual string BuildStringRepresentation(int indent)
        {
            return string.Empty;
        }
    }
}