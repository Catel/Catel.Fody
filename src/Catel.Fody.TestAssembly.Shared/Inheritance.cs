namespace Catel.Fody.TestAssembly
{
    using Data;

    public class BaseClass : ModelBase
    {
        public string PropertyOnBase { get; set; }
    }

    public class InheritedClass : BaseClass
    {
        public string PropertyOnInherited { get; set; }
    }
}