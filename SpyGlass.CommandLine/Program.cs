using System;
using System.Text;
using SpyClass.Analysis;
using SpyClass.Hierarchization;
using SpyClass.Rendering;
using SpyClass.Rendering.HtmlRendering;

namespace SpyGlass.CommandLine
{
    static class Program
    {
        static void Main(string[] args)
        {
            var docs = Analyzer.Analyze("/codespace/code/chroma/Chroma/Chroma/bin/Release/net6.0/Chroma.dll");

            var dtb = new DocTreeBuilder(docs);
            var node = dtb.Build();
            var renderer = new HtmlRenderer("docs");
            
            renderer.Render(node);
        }
    }
}