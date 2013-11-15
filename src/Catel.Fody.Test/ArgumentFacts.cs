// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using Catel.Test;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ArgumentFacts
    {
        #region Methods
        [TestMethod]
        public void CorrectlyThrowsArgumentNullExceptionForNullTypes()
        {
            // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            // Instantiate to have properties registered
            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNull");

            CallMethodAndExpectException<ArgumentNullException>(() => method.Invoke(instance, new object[] { null }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentNullExceptionForNotNullTypes()
        {
            // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            // Instantiate to have properties registered
            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNull");
            method.Invoke(instance, new object[] { "some value" });
        }
        #endregion

        private static void CallMethodAndExpectException<TException>(Action action)
        {
            try
            {
                action();

                Assert.Fail("Expected exception '{0}'", typeof(TException).Name);
            }
            catch (Exception ex)
            {
                Type exceptionType = ex.GetType();

                if (exceptionType == typeof(TException))
                {
                    return;
                }

                if (ex.InnerException != null)
                {
                    exceptionType = ex.InnerException.GetType();
                }

                if (exceptionType == typeof(TException))
                {
                    return;
                }

                Assert.Fail("Expected exception '{0}' but got '{1}'", typeof(TException).Name, ex.GetType().Name);
            }
        }
    }
}