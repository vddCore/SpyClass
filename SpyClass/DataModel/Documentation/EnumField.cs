namespace SpyClass.DataModel.Documentation
{
    public sealed class EnumField
    {
        public EnumDoc Owner { get; }
        
        public string Name { get; }
        public string Value { get; }

        public EnumField(EnumDoc owner, string name, string value)
        {
            Owner = owner;
            
            Name = name;
            Value = value;
        }
    }
}