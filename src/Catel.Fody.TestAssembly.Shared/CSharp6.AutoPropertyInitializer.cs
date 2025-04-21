namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections.ObjectModel;
    using Data;

    public class CSharp6_AutoPropertyInitializer : ModelBase
    {
        public ObservableCollection<SimpleModel> SimpleModels { get; set; } = new ObservableCollection<SimpleModel>();
    }

    public class CSharp6_AutoPropertyInitializerWithMultipleConstructors : ModelBase
    {
        public CSharp6_AutoPropertyInitializerWithMultipleConstructors()
        {

        }

        public CSharp6_AutoPropertyInitializerWithMultipleConstructors(int someValue)
        {

        }

        public bool ShowErrorsWithoutChangeNotification { get; set; } = true;
        public bool ShowErrorsWithChangeNotification { get; set; } = true;

        private void OnShowErrorsWithChangeNotificationChanged()
        {
            // Do something
        }
    }

    public class CSharp6_AutoPropertyInitializer_Generic<T> : ModelBase
    {
        public CSharp6_AutoPropertyInitializer_Generic()
        {

        }

        public CSharp6_AutoPropertyInitializer_Generic(Guid guid)
        {

        }

        public ObservableCollection<T> SimpleModels { get; set; } = new ObservableCollection<T>();

        public T SelectedItem { get; set; } = default(T);
    }

    public class CSharp6_AutoPropertyInitializer_DependencyInjection : ModelBase
    {
        private readonly object _injected1;
        private readonly object _injected2;

        public CSharp6_AutoPropertyInitializer_DependencyInjection(object injected1, object injected2)
        {
            _injected1 = injected1;
            _injected2 = injected2;
        }

        public bool ShowErrors { get; set; } = true;
    }

    public class CSharp6_AutoPropertyInitializer_Generic : CSharp6_AutoPropertyInitializer_Generic<SimpleModel>
    {
        public CSharp6_AutoPropertyInitializer_Generic()
            : base(Guid.NewGuid())
        {

        }

        public string AdditionalProperty { get; set; }
    }

    [NoWeaving]
    public class CSharp6_AutoPropertyInitializer_ExpectedCode
    {
        public CSharp6_AutoPropertyInitializer_ExpectedCode()
        {
            SimpleModels_Field = new ObservableCollection<SimpleModel>();
            SimpleModels_Property = new ObservableCollection<SimpleModel>();
        }

        public ObservableCollection<SimpleModel> SimpleModels_Field;

        public ObservableCollection<SimpleModel> SimpleModels_Property { get; set; }

    }

    [NoWeaving]
    public class CSharp6_AutoPropertyInitializer_Generic_ExpectedCode<T> : ModelBase
    {
        public CSharp6_AutoPropertyInitializer_Generic_ExpectedCode()
        {
            SimpleModels_Field = new ObservableCollection<T>();
            SimpleModels_Property = new ObservableCollection<T>();
            SelectedItem = default(T);
        }

        public CSharp6_AutoPropertyInitializer_Generic_ExpectedCode(Guid guid)
        {
            SimpleModels_Field = new ObservableCollection<T>();
            SimpleModels_Property = new ObservableCollection<T>();
            SelectedItem = default(T);
        }

        public ObservableCollection<T> SimpleModels_Field;

        public ObservableCollection<T> SimpleModels_Property { get; set; }

        public T SelectedItem { get; set; }
    }

    [NoWeaving]
    public class CSharp6_AutoPropertyInitializer_Generic_ExpectedCode : CSharp6_AutoPropertyInitializer_Generic_ExpectedCode<SimpleModel>
    {
        public CSharp6_AutoPropertyInitializer_Generic_ExpectedCode()
            : base(Guid.NewGuid())
        {

        }

        public string AdditionalProperty { get; set; }
    }
}
