namespace Catel.Fody.TestAssembly
{
    using Catel.Data;

    public class BackupTestModel : ModelBase
    {
        [ExcludeFromBackup]
        public string A { get; set; }

        public string B { get; set; }

        public string C { get; set; }
    }
}
