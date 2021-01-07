namespace Catel.Fody.TestAssembly.Bugs.GH0274
{
    using System;
    using Catel.Data;

    public class MyPair : KeyValuePair<int, int>
    {
    }

    public class MyModel : ModelBase
    {
        //public MyAssemblyPair AssemblyPair { get; set; } // this one works fine
        public MyPair Pair { get; set; } // this one fails
    }
}
