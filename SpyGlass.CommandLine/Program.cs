using System.IO;
using System.Reflection;
using SpyClass;
using SpyClass.DataModel.Documentation;

namespace SpyGlass.CommandLine
{
    static class Program
    {
        static void Main(string[] args)
        {
            var doc = Analyzer.Analyze("/codespace/code/chroma/Chroma/Chroma/bin/Release/net6.0/Chroma.dll");
        }
    }
}