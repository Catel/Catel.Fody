namespace Catel.Fody.Tests.Repros
{
    using System;
    using System.ComponentModel;
    using Catel.MVVM;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class GH0531TestFixture
    {
        [TestCase]
        public void Raises_PropertyChanged_Count_Correctly()
        {
            var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0531.TestModel");
            var model = Activator.CreateInstance(modelType);

            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0531.MyDerivedViewModel");
            var viewModel = Activator.CreateInstance(viewModelType, model) as dynamic;

            Assert.That(viewModel.Model.Counter, Is.EqualTo(0));

            PropertyHelper.SetPropertyValue(model, "MyProperty", "test");

            Assert.That(viewModel.Model.Counter, Is.EqualTo(1));
        }
    }
}
