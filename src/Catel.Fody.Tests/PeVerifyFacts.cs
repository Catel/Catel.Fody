// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeVerifyTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2017 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using NUnit.Framework;

    [TestFixture]
    public class PeVerifyFacts
    {
        [Test]
        public void PeVerify()
        {
            var weaver = AssemblyWeaver.Instance;

            Verifier.Verify(weaver.BeforeAssemblyPath, weaver.AfterAssemblyPath);
        }
    }
}