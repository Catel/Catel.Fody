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
}
