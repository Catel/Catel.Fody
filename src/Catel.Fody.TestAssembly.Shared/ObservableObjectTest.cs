namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections.Generic;
    using Catel.Collections;
    using Catel.Data;

    public class ObservableObjectTest : ObservableObject
    {
        private string _existingProperty;

        [NoWeaving]
        public bool OnFirstNameChangedCallbackCalled { get; set; }

        [NoWeaving]
        public bool OnLastNameChangedCallbackCalled { get; set; }

        [NoWeaving]
        public bool OnLastNameWithWrongCallbackChangedCallbackCalled { get; set; }

        public string ExistingProperty
        {
            get => _existingProperty;
            set
            {
                _existingProperty = value;
                RaisePropertyChanged(nameof(ExistingProperty));
            }
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string LastNameWithWrongCallback { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public bool? NullableBoolProperty { get; set; }

        public bool BoolProperty { get; set; }

        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public int NullableIntProperty { get; set; }

        public int IntProperty { get; set; }

        public string NullableStringProperty { get; set; }

        public string StringProperty { get; set; }

        public List<int> ListProperty { get; set; }

        private void OnFirstNameChanged()
        {
            OnFirstNameChangedCallbackCalled = true;
        }

        private void OnLastNameChanged()
        {
            OnLastNameChangedCallbackCalled = true;
        }

        private void OnLastNameWithWrongCallbackChanged(object sender)
        {
            OnLastNameWithWrongCallbackChangedCallbackCalled = true;
        }

        public void ManuallyRaiseChangeNotificationForManualChangeNotificationProperty()
        {
            RaisePropertyChanged(propertyExpression: () => ManualChangeNotificationProperty);
        }

        [NoWeaving]
        public string ManualChangeNotificationProperty { get; set; }

        private bool _isExpanded;
        public object RowGroupDefinition { get; set; }
        public FastObservableCollection<object> ParentCollection { get; set; }

        private void UpdateExpandState()
        {
            // dummy call
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (Equals(_isExpanded, value))
                {
                    return;
                }

                _isExpanded = value;
                if (RowGroupDefinition is null)
                {
                    RaisePropertyChanged(propertyExpression: () => IsExpanded);

                    return;
                }

                var parentCollection = ParentCollection;
                using (parentCollection?.SuspendChangeNotifications())
                {
                    Console.WriteLine("excecute some logic");
                }

                RaisePropertyChanged(propertyExpression: () => IsExpanded);
            }
        }
    }

    [NoWeaving]
    public class ObservableObjectTest_Expected : ObservableObject
    {
        private bool _onFirstNameChangedCallbackCalled;
        private bool _onLastNameChangedCallbackCalled;
        private bool _onLastNameWithWrongCallbackChangedCallbackCalled;
        private string _existingProperty;
        private string _firstName;
        private string _lastName;
        private string _lastNameWithWrongCallback;
        private bool _isExpanded;
        private int? _nullableIntProperty;
        private int _intProperty;
        private string? _nullableStringProperty;
        private string _stringProperty;
        private List<int> _listProperty;
        private bool? _nullableBoolProperty;
        private bool _boolProperty;
        private DateTimeOffset? _nullableDateTimeOffsetProperty;
        private DateTimeOffset _dateTimeOffsetProperty;

        public bool OnFirstNameChangedCallbackCalled
        {
            get => _onFirstNameChangedCallbackCalled;
            set => _onFirstNameChangedCallbackCalled = value;
        }

        public bool OnLastNameChangedCallbackCalled
        {
            get => _onLastNameChangedCallbackCalled;
            set => _onLastNameChangedCallbackCalled = value;
        }

        public bool OnLastNameWithWrongCallbackChangedCallbackCalled
        {
            get => _onLastNameWithWrongCallbackChangedCallbackCalled;
            set => _onLastNameWithWrongCallbackChangedCallbackCalled = value;
        }

        public bool? NullableBoolProperty
        {
            get => _nullableBoolProperty;
            set
            {
                if (value == _nullableBoolProperty)
                {
                    return;
                }

                _nullableBoolProperty = value;
                RaisePropertyChanged(nameof(NullableBoolProperty));
            }
        }

        public bool BoolProperty
        {
            get => _boolProperty;
            set
            {
                if (value == _boolProperty)
                {
                    return;
                }

                _boolProperty = value;
                RaisePropertyChanged(nameof(BoolProperty));
            }
        }

        public DateTimeOffset? NullableDateTimeOffsetProperty
        {
            get => _nullableDateTimeOffsetProperty;
            set
            {
                if (value == _nullableDateTimeOffsetProperty)
                {
                    return;
                }

                _nullableDateTimeOffsetProperty = value;
                RaisePropertyChanged(nameof(NullableDateTimeOffsetProperty));
            }
        }

        public DateTimeOffset DateTimeOffsetProperty
        {
            get => _dateTimeOffsetProperty;
            set
            {
                if (value == _dateTimeOffsetProperty)
                {
                    return;
                }

                _dateTimeOffsetProperty = value;
                RaisePropertyChanged(nameof(DateTimeOffsetProperty));
            }
        }

        public int? NullableIntProperty
        {
            get => _nullableIntProperty;
            set
            {
                if (value == _nullableIntProperty)
                {
                    return;
                }

                _nullableIntProperty = value;
                RaisePropertyChanged(nameof(NullableIntProperty));
            }
        }

        public int IntProperty
        {
            get => _intProperty;
            set
            {
                if (value == _intProperty)
                {
                    return;
                }

                _intProperty = value;
                RaisePropertyChanged(nameof(IntProperty));
            }
        }

        public string? NullableStringProperty
        {
            get => _nullableStringProperty;
            set
            {
                if (value == _nullableStringProperty)
                {
                    return;
                }

                _nullableStringProperty = value;
                RaisePropertyChanged(nameof(NullableStringProperty));
            }
        }

        public string StringProperty
        {
            get => _stringProperty;
            set
            {
                if (value == _stringProperty)
                {
                    return;
                }

                _stringProperty = value;
                RaisePropertyChanged(nameof(StringProperty));
            }
        }

        public List<int> ListProperty
        {
            get => _listProperty;
            set
            {
                if (ReferenceEquals(value, _listProperty))
                {
                    return;
                }

                _listProperty = value;
                RaisePropertyChanged(nameof(StringProperty));
            }
        }

        public string ExistingProperty
        {
            get => _existingProperty;
            set
            {
                if (value == _lastNameWithWrongCallback)
                {
                    return;
                }

                _existingProperty = value;
                RaisePropertyChanged(nameof(ExistingProperty));
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (value == _lastNameWithWrongCallback)
                {
                    return;
                }

                _firstName = value;
                OnFirstNameChanged();
                RaisePropertyChanged(nameof(FirstName));
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (value == _lastNameWithWrongCallback)
                {
                    return;
                }

                _lastName = value;
                OnLastNameChanged();
                RaisePropertyChanged(nameof(LastName));
            }
        }

        public string LastNameWithWrongCallback
        {
            get => _lastNameWithWrongCallback;
            set
            {
                if (value == _lastNameWithWrongCallback)
                {
                    return;
                }

                _lastNameWithWrongCallback = value;
                RaisePropertyChanged(nameof(LastNameWithWrongCallback));
            }
        }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        private void OnFirstNameChanged()
        {
            OnFirstNameChangedCallbackCalled = true;
        }

        private void OnLastNameChanged()
        {
            OnLastNameChangedCallbackCalled = true;
        }

        private void OnLastNameWithWrongCallbackChanged(object sender)
        {
            OnLastNameWithWrongCallbackChangedCallbackCalled = true;
        }

        public void ManuallyRaiseChangeNotificationForManualChangeNotificationProperty()
        {
            RaisePropertyChanged(nameof(ManualChangeNotificationProperty));
        }

        public string ManualChangeNotificationProperty { get; set; }

        public object RowGroupDefinition { get; set; }
        public FastObservableCollection<object> ParentCollection { get; set; }

        private void UpdateExpandState()
        {
            // dummy call
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (Equals(_isExpanded, value))
                {
                    return;
                }

                _isExpanded = value;
                if (RowGroupDefinition is null)
                {
                    RaisePropertyChanged("IsExpanded");

                    return;
                }

                var parentCollection = ParentCollection;
                using (parentCollection?.SuspendChangeNotifications())
                {
                    Console.WriteLine("excecute some logic");
                }

                RaisePropertyChanged("IsExpanded");
            }
        }
    }
}
