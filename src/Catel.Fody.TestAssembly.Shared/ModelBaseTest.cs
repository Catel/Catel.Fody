namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using Data;

    public class GenericModelBaseTestBase<TComponent> : ModelBase
    {
        public ObservableCollection<TComponent> CatelOperations
        {
            get { return GetValue<ObservableCollection<TComponent>>(CatelOperationsProperty); }
            set { SetValue(CatelOperationsProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData CatelOperationsProperty = RegisterProperty("CatelOperations", typeof(ObservableCollection<TComponent>), null,
            (sender, e) => ((GenericModelBaseTestBase<TComponent>)sender).OnCatelOperationsChanged());
#elif CATEL_6_OR_GREATER
        public static readonly IPropertyData CatelOperationsProperty = RegisterProperty<ObservableCollection<TComponent>>("CatelOperations", () => null,
            (sender, e) => ((GenericModelBaseTestBase<TComponent>)sender).OnCatelOperationsChanged());
#endif

        private void OnCatelOperationsChanged()
        {
        }

        public ObservableCollection<TComponent> Operations { get; set; }

        private void OnOperationsChanged()
        {
            HasChangedNotificationBeenCalled = true;
        }

        public bool HasChangedNotificationBeenCalled { get; private set; }
    }

    public class GenericModelBaseTest : GenericModelBaseTestBase<int>
    {

    }

    /// <summary>
    /// DataObjectBaseTest class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    public class ModelBaseTest : ModelBase
    {
//        static ModelBaseTest()
//        {
//#if CATEL_6_OR_GREATER
//            FullNameProperty.IsDecoratedWithValidationAttributes = true;
//#endif
//        }

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public ModelBaseTest()
        {
            CollectionProperty = new ObservableCollection<int>();
        }

        public bool BoolValue { get; set; }

        public int IntValue { get; set; }

        public Guid GuidValue { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string PropertyWithoutBackingField
        {
            get { return "JustAValue"; }
        }

        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), string.Empty);
#elif CATEL_6_OR_GREATER
        public static readonly IPropertyData FullNameProperty = RegisterProperty<string>("FullName", string.Empty);
#endif

        public string FullNameWithChangeCallback { get; set; }

        public string AnotherPropertyWithChangeCallback { get; set; }

        public ObservableCollection<int> CollectionProperty { get; set; }

        public bool OnLastNameChangedCalled { get; set; }

        private void OnLastNameChanged(int someParameter)
        {
            // This method must only raise warning, but not be used
        }

        private void OnLastNameChanged()
        {
            OnLastNameChangedCalled = true;
        }

        public bool OnFullNameWithChangeCallbackChangedCalled { get; set; }

        private void OnFullNameWithChangeCallbackChanged()
        {
            OnFullNameWithChangeCallbackChangedCalled = true;
        }

        public bool OnAnotherPropertyWithChangeCallbackChangedCalled { get; set; }

        private void OnAnotherPropertyWithChangeCallbackChanged()
        {
            OnAnotherPropertyWithChangeCallbackChangedCalled = true;
        }

        [Required]
        public string PropertyWithValidationAttribute { get; set; }
    }
}
