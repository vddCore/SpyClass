using System;
using SpyClass;

namespace SpyGlass.CommandLine
{
    static class Program
    {
        static void Main(string[] args)
        {
            var docs = Analyzer.Analyze("SpyGlass.CommandLine.dll");

            foreach (var doc in docs)
            {
                if (!doc.Namespace.StartsWith("SpyGlass.CommandLine.TestNamespace"))
                    continue;

                Console.WriteLine(doc.ToString());
            }
        }
    }
}