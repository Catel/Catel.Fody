namespace Catel.Fody.TestAssembly.Bugs.GH0473
{
    using System;
    using Catel.Data;
    using Catel.MVVM;

    public class GH0473ViewModel : ViewModelBase
    {
        public GH0473ViewModel(TestModel testModel)
        {
            ArgumentNullException.ThrowIfNull(testModel);

            Model = testModel;
        }

        [Model(SupportIEditableObject = false, SupportValidation = false)]
        [Expose(nameof(TestModel.Property))]
        public TestModel Model { get; }
    }

    public class GH0473ViewModel_Expected : ViewModelBase
    {
        public GH0473ViewModel_Expected(TestModel testModel)
        {
            ArgumentNullException.ThrowIfNull(testModel);

            Model = testModel;
        }

        [Model(SupportIEditableObject = false, SupportValidation = false)]
        public TestModel Model
        {
            get { return GetValue<TestModel>(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData ModelProperty = RegisterProperty(nameof(Model), typeof(TestModel), null);
#else
        public static readonly IPropertyData ModelProperty = RegisterProperty(nameof(Model), typeof(TestModel), null);
#endif

        [ViewModelToModel("Model", "Property")]
        public object Property
        {
            get { return GetValue<object>(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData PropertyProperty = RegisterProperty(nameof(Property), typeof(object), null);
#else
        public static readonly IPropertyData PropertyProperty = RegisterProperty(nameof(Property), typeof(object), null);
#endif
    }

    public class TestModel
    {
        public object Property { get; set; }
    }
}
