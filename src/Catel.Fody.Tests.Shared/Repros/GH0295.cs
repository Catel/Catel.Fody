#if CATEL_5

namespace Catel.Fody.Tests.Repros
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0295TestFixture
    {
        [TestCase]
        public void NetStandardWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0295.ModelReferencingNetStandard");
            var instance = Activator.CreateInstance(type) as dynamic;

            Assert.That(instance, Is.Not.Null);

            var netStandardType = AssemblyWeaver.Instance_NetStandard.Assembly.GetType("Catel.Fody.TestAssembly.NetStandardModel");
            var netStandardInstance = Activator.CreateInstance(netStandardType) as dynamic;

            Assert.That(netStandardInstance, Is.Not.Null);
        }
    }
}

#endif
