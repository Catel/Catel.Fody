namespace Catel.Fody.TestAssembly
{
    using System.Collections.Generic;
    using System.IO;
    using Collections;
    using Data;

    public class CTL768_BaseModel : ModelBase
    {
        public CTL768_BaseModel(string location)
        {
            
        }
    }

    public class CTL768_Model : CTL768_BaseModel
    {
        private string _location;

        public CTL768_Model(string location)
            : base(Path.Combine(location))
        {
            _location = location;
            FilteredItems = new FastObservableCollection<string>();
        }

        public FastObservableCollection<string> FilteredItems { get; private set; }
    }

    [NoWeaving]
    public class CTL768_Model_ExpectedCode : CTL768_BaseModel
    {
        private string _location;

        public CTL768_Model_ExpectedCode(string location)
            : base(Path.Combine(location))
        {
            _location = location;
            FilteredItems = new FastObservableCollection<string>();
        }

        public FastObservableCollection<string> FilteredItems { get; private set; }
    }
}