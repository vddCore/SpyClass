namespace SpyClass.DataModel.Documentation
{
    public class EnumField
    {
        public string Name { get; }
        public string Value { get; }

        public EnumField(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}