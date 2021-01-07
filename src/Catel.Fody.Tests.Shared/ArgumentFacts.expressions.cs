// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NUnit.Framework;

    public partial class ArgumentFacts
    {
        [TestFixture]
        public class SupportedExpressions
        {
            [TestCase]
            public void CorrectlyCompilesGenericArgumentChecks()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullForGenericArgument");
                var genericMethod = method.MakeGenericMethod(typeof (ProcessStartInfo));

                AssertException<ArgumentNullException>(() => genericMethod.Invoke(instance, new object[] { null }));
            }

            [TestCase]
            public void CorrectlyCompilesGenericArgumentWithPredicateChecks()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullForGenericArgumentWithPredicate");
                var genericMethod = method.MakeGenericMethod(typeof(MyEventArgs));

                AssertException<ArgumentNullException>(() => genericMethod.Invoke(instance, new object[] { null }));
            }

            [TestCase]
            public void CheckForNullForGenericArgumentWithPredicateAndInstantiation()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullForGenericArgumentWithPredicateAndInstantiation");
                var genericMethod = method.MakeGenericMethod(typeof(MyEventArgs));

                AssertException<ArgumentNullException>(() => genericMethod.Invoke(instance, new object[] { null, "mystring" }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentNullExceptionForNullTypes()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNull");

                AssertException<ArgumentNullException>(() => method.Invoke(instance, new object[] { null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentNullExceptionForMultipleNullTypes()
            {
                // Note: do NOT instantiate the type, then you will get the "unweaved" types. You need to use this helper during unit tests
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                // Instantiate to have properties registered
                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullWithMultipleParameters");

                AssertException<ArgumentNullException>(() => method.Invoke(instance, new object[] { null, null, null }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentNullExceptionForNotNullTypes()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNull");
                method.Invoke(instance, new object[] { "some value" });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNullString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmpty");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { (string)null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForEmptyString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmpty");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { string.Empty }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNoNullOrEmptyString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmpty");

                method.Invoke(instance, new object[] { "some value" });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNullString2()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrWhitespace");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { (string)null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForWhitespaceString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrWhitespace");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { "   " }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNoNullOrWhitespaceString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

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
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmptyArray");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { (object[])null }));
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForEmptyArray()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmptyArray");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { new object[] { } }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNoNullOrEmptyArray()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullOrEmptyArray");

                method.Invoke(instance, new object[] { new object[] { 1, "some value" } });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotMatchString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMatch");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { "abcd" }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForMatchString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMatch");

                method.Invoke(instance, new object[] { "12345" });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForMatchString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNotMatch");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { "12345" }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNotMatchString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNotMatch");

                method.Invoke(instance, new object[] { "abcd" });
            }

            /*
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalInt()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalInt");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0 }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalInt()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalInt");

                method.Invoke(instance, new object[] { 3 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalDouble()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalDouble");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0.0d }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalDouble()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalDouble");

                method.Invoke(instance, new object[] { 3.0d });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalFloat()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalFloat");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0.0f }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalFloat()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMinimalFloat");

                method.Invoke(instance, new object[] { 3.0f });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalInt()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumInt");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6 }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalInt()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumInt");

                method.Invoke(instance, new object[] { 0 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalDouble()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumDouble");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6.0d }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalDouble()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumDouble");

                method.Invoke(instance, new object[] { 0.0d });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalFloat()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumFloat");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6.0f }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalFloat()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForMaximumFloat");

                method.Invoke(instance, new object[] { 0.0f });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeInt()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeInt");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5 }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeInt()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeInt");

                method.Invoke(instance, new object[] { 3 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeDouble()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeDouble");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5.0d }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeDouble()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeDouble");

                method.Invoke(instance, new object[] { 3.0d });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeFloat()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeFloat");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5.0f }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeFloat()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeFloat");

                method.Invoke(instance, new object[] { 3.0f });
            }

            /*
            [TestCase]
            public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeString");

                AssertException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { "z" }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeString()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOutOfRangeString");

                method.Invoke(instance, new object[] { "d" });
            }
            */

            //[TestCase]
            //public void CorrectlyThrowsArgumentExceptionForNotInheritsFrom()
            //{
            //    var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom");

            //    AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { new Exception() }));
            //}

            //[TestCase]
            //public void CorrectlyThrowsNoArgumentExceptionForInheritsFrom()
            //{
            //    var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom");

            //    method.Invoke(instance, new object[] { new ArgumentNullException() });
            //}

            //[TestCase]
            //public void CorrectlyThrowsArgumentExceptionForNotInheritsFrom2()
            //{
            //    var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom2");

            //    AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(Exception) }));
            //}

            //[TestCase]
            //public void CorrectlyThrowsNoArgumentExceptionForInheritsFrom2()
            //{
            //    var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

            //    var instance = Activator.CreateInstance(type);

            //    var method = type.GetMethod("CheckForInheritsFrom2");

            //    method.Invoke(instance, new object[] { typeof(ArgumentNullException) });
            //}

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotTypeOf()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType");

                AssertException<ArgumentException>(() => method.Invoke(instance, new[] { new object() }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForTypeOf()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType");

                method.Invoke(instance, new object[] { 2 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotTypeOf2()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType2");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(object) }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForTypeOf2()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfType2");

                method.Invoke(instance, new object[] { typeof(int) });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotInterfaceImplemented()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface");

                AssertException<ArgumentException>(() => method.Invoke(instance, new[] { new object() }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNotInterfaceImplemented()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface");

                method.Invoke(instance, new object[] { 2 });
            }

            [TestCase]
            public void CorrectlyThrowsArgumentExceptionForNotInterfaceImplemented2()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface2");

                AssertException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(object) }));
            }

            [TestCase]
            public void CorrectlyThrowsNoArgumentExceptionForNotInterfaceImplemented2()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForOfImplementsInterface2");

                method.Invoke(instance, new object[] { typeof(int) });
            }

            [TestCase]
            public void CheckForNullWithMultipleParametersWithoutContent()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullWithMultipleParametersWithoutContent");

                AssertException<ArgumentNullException>(() => method.Invoke(instance, new object[] { null, null, null }));
            }

            [TestCase]
            public void CheckForNullWithMultipleParametersWithoutContent_EmptyRawCollection()
            {
                var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass");

                var instance = Activator.CreateInstance(type);

                var method = type.GetMethod("CheckForNullWithMultipleParametersWithoutContent");

                var customClassType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CustomClassType");
                var customClassInstance = Activator.CreateInstance(customClassType);

                AssertException<ArgumentNullException>(() => method.Invoke(instance, new[] { customClassInstance, null, (IList)new List<string>() }));
            }
        }
    }
}
