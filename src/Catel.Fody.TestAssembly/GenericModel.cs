namespace Catel.Fody.TestAssembly
{
    using Data;

    public class GenericModel<T> : ModelBase
    {
        public T GenericProperty { get; set; }
    }

    public class GenericModel : GenericModel<SimpleModel>
    {

    }
}
