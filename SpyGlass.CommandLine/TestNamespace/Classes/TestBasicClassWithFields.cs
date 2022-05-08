using System.Collections.Generic;

namespace SpyGlass.CommandLine.TestNamespace.Classes
{
    public class TestBasicClassWithFields
    {
        public int Int32Field;
        public bool BooleanField;
        public string StringField;

        public List<TestBasicClassWithFields> GenericArgumentListField;
        public Dictionary<string, Dictionary<string, int>> GenericArgumentDictionaryField;

        public TestBasicClassWithFields ReferenceToSelf;

        public TestBasicClassWithGenericParameters<TestBasicClassWithFields, int, nuint> GenericInstanceField;
    }
}