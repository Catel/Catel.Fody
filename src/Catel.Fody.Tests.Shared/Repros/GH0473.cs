namespace Catel.Fody.Tests.Repros
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0473TestFixture
    {
        [TestCase]
        public void Weaving_Init_Only_Model_Properties()
        {
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0473.GH0473ViewModel");
            var viewModel = Activator.CreateInstance(viewModelType) as dynamic;

            viewModel.Property = new object();
        }
    }
}
