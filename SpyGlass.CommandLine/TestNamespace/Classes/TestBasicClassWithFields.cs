using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace SpyGlass.CommandLine.TestNamespace.Classes
{
    public interface ITest1
    {
        
    }

    public interface ITest2
    {
        
    }

    public interface ITest3
    {
        
    }

    [Table("fuck", Schema = "nananana")]
    public class BlahBase
    {
    }
    
    public struct Blah<T> : ITest1, ITest2, ITest3 where T: class
    {
        public Blah(T blah, int specific)
        {
            
        }
        
        public Blah(T blah)
            : this(blah, 10)
        {
            
        }
    }
}