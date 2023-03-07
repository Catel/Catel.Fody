namespace Catel.Fody.TestAssembly
{
    using global::Fody;
    using NUnit.Framework;

    [TestFixture]
    public class PeVerifyFacts
    {
        [Test]
        public void PeVerify()
        {
            var weaver = AssemblyWeaver.Instance;

            PeVerifier.ThrowIfDifferent(weaver.BeforeAssemblyPath, weaver.AfterAssemblyPath);
        }
    }
}
