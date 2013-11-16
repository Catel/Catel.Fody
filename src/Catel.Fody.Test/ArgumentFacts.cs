// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Test
{
    using System;

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
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNull");
            method.Invoke(instance, new object[] { "some value" });
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNullString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrEmpty");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { (string)null }));
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForEmptyString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrEmpty");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { string.Empty }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForNoNullOrEmptyString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrEmpty");

            method.Invoke(instance, new object[] { "some value" });
        }         
        
        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNullArray()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrEmptyArray");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { (object[])null }));
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForEmptyArray()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrEmptyArray");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { new object[] { } }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForNoNullOrEmptyArray()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrEmptyArray");

            method.Invoke(instance, new object[] { new object[] { 1, "some value" } });
        } 
        
        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNullString2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrWhitespace");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { (string)null }));
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForWhitespaceString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrWhitespace");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { "   " }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForNoNullOrWhitespaceString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNullOrWhitespace");

            method.Invoke(instance, new object[] { "some value" });
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotMatchString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMatch");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { "abcd" }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForMatchString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMatch");

            method.Invoke(instance, new object[] { "12345" });
        }       
        
        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForMatchString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNotMatch");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { "12345" }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForNotMatchString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForNotMatch");

            method.Invoke(instance, new object[] { "abcd" });
        }

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
        #endregion
    }
}