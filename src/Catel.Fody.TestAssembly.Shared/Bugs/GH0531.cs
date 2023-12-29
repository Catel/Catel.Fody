namespace Catel.Fody.TestAssembly.Bugs.GH0531
{
    using Catel.Data;
    using Catel.MVVM;
    using System.Diagnostics;

    public class TestModel : ObservableObject
    {
        private int _counter;

        public object MyProperty { get; set; }

        public int Counter { get => _counter; }

        private void OnMyPropertyChanged()
        {
            _counter++;

            Debug.WriteLine($"Property changed: {MyProperty} {_counter}");
        }
    }

    public class MyDerivedViewModel : ViewModelBase
    {
        public MyDerivedViewModel(TestModel testModel)
        {
            Model = testModel;
        }

        [Model]
        public TestModel Model { get; set; }

        [ViewModelToModel]
        public object MyProperty { get; set; }
    }
}
