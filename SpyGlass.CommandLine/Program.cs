using System;
using SpyClass;

namespace SpyGlass.CommandLine
{
    static class Program
    {
        static void Main(string[] args)
        {
            var docs = Analyzer.Analyze("/codespace/code/chroma/Chroma/Chroma/bin/Release/net6.0/Chroma.dll");

            foreach (var doc in docs)
            {
                Console.WriteLine(doc.ToString());
            }
        }
    }
}