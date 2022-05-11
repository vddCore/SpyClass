using System;
using System.Runtime.InteropServices;

namespace SpyGlass.CommandLine.TestNamespace.Classes
{
    public class Blah
    {
        public delegate void SomeEventDelegate(int arg1, string arg2);

        public class InnerBlah
        {

        }
        
        public static event SomeEventDelegate TestEvent;
    }

    public struct Blah2
    {
    }
}