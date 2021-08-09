namespace Catel.Fody.TestAssembly.Bugs.GH0361
{
    using Catel.MVVM;

    public class MyDerivedViewModel : ViewModelBase
    {
        [Model]
        public TestModel Model { get; set; }
        [ViewModelToModel]
        public object Property { get; set; }

        public void SetValue(string s)
        {

        }
    }

    public class TestModel
    {
        public object Property { get; set; }
    }
}
