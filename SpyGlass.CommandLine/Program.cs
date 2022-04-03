using System;
using SpyClass.DataModel;
using SpyClass.DataModel.Documentation;

namespace SpyGlass.CommandLine
{
    static class Program
    {
        private enum TestEnum : sbyte
        {
            A = -1,
            B = 2,
            X = -2,
            Y = 1
        }
        
        static void Main(string[] args)
        {
            var enumDoc = TypeDoc.FromType(typeof(TestEnum));
            Console.WriteLine(enumDoc);
        }
    }
}