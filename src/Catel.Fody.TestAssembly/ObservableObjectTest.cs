namespace Catel.Fody.TestAssembly
{
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

        public string ExistingProperty
        {
            get => _existingProperty;
            set
            {
                _existingProperty = value;
                RaisePropertyChanged(nameof(ExistingProperty));
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
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
    }
}
