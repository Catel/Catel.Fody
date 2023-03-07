namespace Catel.Fody.TestAssembly
{
    using Catel.Data;

    [NoWeaving]
    public class NoWeavingModelTest : ModelBase
    {
        public string FirstName { get; set; }
    }

    public class NoPropertyWeavingModelTest : ModelBase
    {
        #region Properties
        [NoWeaving]
        public string FirstName { get; set; }

        public string LastName { get; set; }
        #endregion
    }
}