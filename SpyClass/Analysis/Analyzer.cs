using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using SpyClass.Analysis.DataModel.Documentation;
using SpyClass.Analysis.Extensions;

namespace SpyClass.Analysis
{
    public static class Analyzer
    {
        // todo thread-unsafe
        public static AnalysisOptions Options { get; private set; }
        
        public static List<TypeDoc> Analyze(string fileName, AnalysisOptions options = null)
        {
            options ??= new AnalysisOptions();
            Options = options;
            
            var ret = new List<TypeDoc>();

            var assembly = AssemblyDefinition.ReadAssembly(fileName);
            var module = assembly.MainModule;

            var types = module.Types;
            
            foreach (var type in types)
            {
                if (type.FullName == "<Module>")
                {
                    continue;
                }
                
                var compilerGeneratedAttribute = type.GetCustomAttribute(typeof(CompilerGeneratedAttribute).FullName);
                
                if (compilerGeneratedAttribute != null && options.IgnoreCompilerGeneratedTypes)
                    continue;

                if (!type.IsPublic && !options.IncludeNonPublicTypes)
                    continue;

                if (type.IsNested)
                    continue;

                ret.Add(TypeDoc.FromType(type));
            }
            
            return ret;
        }
    }
}