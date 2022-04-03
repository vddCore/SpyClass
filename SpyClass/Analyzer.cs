using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SpyClass.DataModel;
using SpyClass.DataModel.Documentation;

namespace SpyClass
{
    public static class Analyzer
    {
        public static List<TypeDoc> Analyze(Assembly assembly)
        {
            var ret = new List<TypeDoc>();

            var types = assembly.GetTypes();
            
            foreach (var type in types)
            {
                ret.Add(TypeDoc.FromType(type));
            }
            
            return ret;
        }
    }
}