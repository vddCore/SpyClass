using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;

namespace SpyClass.DataModel.Documentation.Components
{
    public sealed class EnumField : DocComponent
    {
        public EnumDoc Owner { get; }
        
        public string Name { get; }
        public string Value { get; }

        public EnumField(ModuleDefinition module, EnumDoc owner, string name, string value)
            : base(module)
        {
            Owner = owner;
            
            Name = name;
            Value = value;
        }
    }
}