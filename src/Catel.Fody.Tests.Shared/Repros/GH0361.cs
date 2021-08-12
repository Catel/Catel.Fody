namespace Catel.Fody.Tests.Repros
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0361TestFixture
    {
        [TestCase]
        public void WeavingWithSetValueMethodInViewModel()
        {
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0361.MyDerivedViewModel");
            var viewModel = Activator.CreateInstance(viewModelType) as dynamic;

            viewModel.Property = new object();
        }
    }
}
