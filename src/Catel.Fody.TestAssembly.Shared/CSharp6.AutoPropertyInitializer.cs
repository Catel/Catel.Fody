namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections.ObjectModel;
    using Data;

    public class CSharp6_AutoPropertyInitializer : ModelBase
    {
        #region Properties
        public ObservableCollection<SimpleModel> SimpleModels { get; set; } = new ObservableCollection<SimpleModel>();
        #endregion
    }

    public class CSharp6_AutoPropertyInitializerWithMultipleConstructors : ModelBase
    {
        public CSharp6_AutoPropertyInitializerWithMultipleConstructors()
        {

        }

        public CSharp6_AutoPropertyInitializerWithMultipleConstructors(int someValue)
        {

        }

        #region Properties
        public bool ShowErrors { get; set; } = true;
        #endregion
    }

    public class CSharp6_AutoPropertyInitializer_Generic<T> : ModelBase
    {
        public CSharp6_AutoPropertyInitializer_Generic()
        {

        }

        public CSharp6_AutoPropertyInitializer_Generic(Guid guid)
        {

        }

        #region Properties
        public ObservableCollection<T> SimpleModels { get; set; } = new ObservableCollection<T>();

        public T SelectedItem { get; set; } = default(T);
        #endregion
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

        public ObservableCollection<T> SimpleModels_Field;

        public ObservableCollection<T> SimpleModels_Property { get; set; }

        public T SelectedItem { get; set; }
    }

    [NoWeaving]
    public class CSharp6_AutoPropertyInitializer_Generic_ExpectedCode : CSharp6_AutoPropertyInitializer_Generic_ExpectedCode<SimpleModel>
    {
    }
}
