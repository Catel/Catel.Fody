namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class ExcludeFromBackupFacts
    {
        [Test]
        public void CorrectlyExcludesValuesFromBackup()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.BackupTestModel");
            var testModel = (dynamic)Activator.CreateInstance(type);

            testModel.A = "1";
            testModel.B = "2";
            testModel.C = "3";

            var editableObject = (IEditableObject)testModel;
            editableObject.BeginEdit();

            testModel.A = "A";
            testModel.B = "B";
            testModel.C = "C";

            editableObject.CancelEdit();

            Assert.That("A", Is.EqualTo(testModel.A));
            Assert.That("2", Is.EqualTo(testModel.B));
            Assert.That("3", Is.EqualTo(testModel.C));
        }
    }
}
