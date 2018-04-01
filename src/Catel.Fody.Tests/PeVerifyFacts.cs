// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeVerifyTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2017 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


#pragma warning disable 618
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