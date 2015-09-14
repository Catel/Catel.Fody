// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Test
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Windows.Media;
    using NUnit.Framework;

    public class ArgumentExpressionFacts
    {
        [TestFixture]
        public class SupportedExpressions
        {
            [TestCase]
            public void CorrectlyCompilesGenericArgumentChecks()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullForGenericArgument");
                var genericMethod = method.MakeGenericMethod(typeof (ProcessStartInfo));

                CallMethodAndExpectException<ArgumentNullException>(() => genericMethod.Invoke(instance, new object[] { null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentNullExceptionForNullTypes()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNull");

                CallMethodAndExpectException<ArgumentNullException>(() => method.Invoke(instance, new object[] { null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentNullExceptionForMultipleNullTypes()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullWithMultipleParameters");

                CallMethodAndExpectException<ArgumentNullException>(() => method.Invoke(instance, new object[] { null, null, null }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentNullExceptionForNotNullTypes()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNull");
                method.Invoke(instance, new object[] { "some value" });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNullString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmpty");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { (string)null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForEmptyString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmpty");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { string.Empty }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNoNullOrEmptyString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmpty");

                method.Invoke(instance, new object[] { "some value" });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNullString2()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrWhitespace");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { (string)null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForWhitespaceString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrWhitespace");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { "   " }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNoNullOrWhitespaceString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrWhitespace");

                method.Invoke(instance, new object[] { "some value" });
            }
        }

        [TestFixture]
        public class UnsupportedExpressions
        {
            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNullArray()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmptyArray");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { (object[])null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForEmptyArray()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmptyArray");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { new object[] { } }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNoNullOrEmptyArray()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmptyArray");

                method.Invoke(instance, new object[] { new object[] { 1, "some value" } });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotMatchString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMatch");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { "abcd" }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForMatchString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMatch");

                method.Invoke(instance, new object[] { "12345" });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForMatchString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNotMatch");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { "12345" }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNotMatchString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNotMatch");

                method.Invoke(instance, new object[] { "abcd" });
            }

            /*
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalInt()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalInt");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0 }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalInt()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalInt");

                method.Invoke(instance, new object[] { 3 });
            }     
        
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalDouble()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalDouble");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0.0d }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalDouble()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalDouble");

                method.Invoke(instance, new object[] { 3.0d });
            }      
        
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalFloat()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalFloat");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0.0f }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalFloat()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalFloat");

                method.Invoke(instance, new object[] { 3.0f });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalInt()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumInt");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6 }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalInt()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumInt");

                method.Invoke(instance, new object[] { 0 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalDouble()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumDouble");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6.0d }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalDouble()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumDouble");

                method.Invoke(instance, new object[] { 0.0d });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalFloat()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumFloat");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6.0f }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalFloat()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumFloat");

                method.Invoke(instance, new object[] { 0.0f });
            }    
        
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeInt()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeInt");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5 }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeInt()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeInt");

                method.Invoke(instance, new object[] { 3 });
            }    
        
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeDouble()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeDouble");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5.0d }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeDouble()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeDouble");

                method.Invoke(instance, new object[] { 3.0d });
            }        
        
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeFloat()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeFloat");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5.0f }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeFloat()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeFloat");

                method.Invoke(instance, new object[] { 3.0f });
            }        
        
            /*
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeString");

                CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { "z" }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeString()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeString");

                method.Invoke(instance, new object[] { "d" });
            }
            */

            //[TestCase]
            //public void CorrectlyThrowsArgumentExceptionForNotInheritsFrom()
            //{
            //    var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom");

            //    CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { new Exception() }));
            //}

            //[TestCase]
            //public void CorrectlyThrowsNoArgumentExceptionForInheritsFrom()
            //{
            //    var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom");

            //    method.Invoke(instance, new object[] { new ArgumentNullException() });
            //}

            //[TestCase]
            //public void CorrectlyThrowsArgumentExceptionForNotInheritsFrom2()
            //{
            //    var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom2");

            //    CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(Exception) }));
            //}

            //[TestCase]
            //public void CorrectlyThrowsNoArgumentExceptionForInheritsFrom2()
            //{
            //    var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom2");

            //    method.Invoke(instance, new object[] { typeof(ArgumentNullException) });
            //}

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotTypeOf()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { new object() }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForTypeOf()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType");

                method.Invoke(instance, new object[] { 2 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotTypeOf2()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType2");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(object) }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForTypeOf2()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType2");

                method.Invoke(instance, new object[] { typeof(int) });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotInterfaceImplemented()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new[] { new object() }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNotInterfaceImplemented()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface");

                method.Invoke(instance, new object[] { 2 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotInterfaceImplemented2()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface2");

                CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(object) }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNotInterfaceImplemented2()
            {
                var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface2");

                method.Invoke(instance, new object[] { typeof(int) });
            }
        }

        #region Methods

        private static void CallMethodAndExpectException<TException>(Action action)
        {
            try
            {
                action();

                Assert.Fail("Expected exception '{0}'", typeof(TException).Name);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
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