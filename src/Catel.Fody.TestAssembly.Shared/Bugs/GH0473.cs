namespace Catel.Fody.TestAssembly.Bugs.GH0473
{
    using Catel.MVVM;

    public class GH0473ViewModel : ViewModelBase
    {
        public GH0473ViewModel()
        {
            Model = new TestModel();
        }

        [Model]
        [Expose(nameof(TestModel.Property))]
        public TestModel Model { get; }
    }

    public class TestModel
    {
        public object Property { get; set; }
    }
}
