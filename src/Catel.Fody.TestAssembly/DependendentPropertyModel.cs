namespace Catel.Fody.TestAssembly
{
    using Catel.Data;

    public class DependendentPropertyModel : ModelBase
    {
        #region Properties
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", this.FirstName, this.LastName).Trim();
            }
        }
        #endregion
    }

    public class DetailedDependendentPropertyModel : DependendentPropertyModel
    {
        #region Properties
        public string Profile
        {
            get
            {
                return string.Format("Name:{0}, Age:{1}", this.FullName, this.Age).Trim();
            }
        }
        #endregion
    }
}