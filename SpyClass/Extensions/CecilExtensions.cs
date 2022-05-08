using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SpyClass.Extensions
{
    internal static class CecilExtensions
    {
        public static CustomAttribute GetCustomAttribute(this TypeDefinition typeDef, string name)
        {
            foreach (var attr in typeDef.CustomAttributes)
            {
                if (attr.AttributeType.FullName == name)
                {
                    return attr;
                }
            }
            
            return null;
        }

        public static List<string> GetEnumNames(this TypeDefinition typeDef)
        {
            if (!typeDef.IsEnum)
                throw new InvalidOperationException("Not an enum.");
            
            var ret = new List<string>();
            foreach (var field in typeDef.Fields)
            {
                if (field.Name == "value__")
                    continue;

                ret.Add(field.Name);
            }

            return ret;
        }
        
        public static List<string> GetEnumValues(this TypeDefinition typeDef)
        {
            if (!typeDef.IsEnum)
                throw new InvalidOperationException("Not an enum.");
            
            var ret = new List<string>();
            foreach (var field in typeDef.Fields)
            {
                if (field.Name == "value__")
                    continue;

                ret.Add(field.Constant.ToString());
            }

            return ret;
        }
    }
}