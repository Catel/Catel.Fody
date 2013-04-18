// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataObjectBaseTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections.ObjectModel;
    using Data;

    /// <summary>
    /// DataObjectBaseTest class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    public class ModelBaseTest : ModelBase<ModelBaseTest>
    {
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

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        /// <summary>
        /// Register the FullName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), string.Empty);

        /// <summary>
        /// Gets or sets the full name with change callback.
        /// </summary>
        public string FullNameWithChangeCallback { get; set; }

        /// <summary>
        /// Another.
        /// </summary>
        public string AnotherPropertyWithChangeCallback { get; set; }

        public ObservableCollection<int> CollectionProperty { get; set; }

        public bool OnLastNameChangedCalled { get; set; }

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
    }
}