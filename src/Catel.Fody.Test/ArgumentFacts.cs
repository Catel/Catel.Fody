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

        /*
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalInt()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMinimalInt");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0 }));
        }


        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalInt()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMinimalInt");

            method.Invoke(instance, new object[] { 3 });
        }     
        
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalDouble()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMinimalDouble");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0.0d }));
        }


        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalDouble()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMinimalDouble");

            method.Invoke(instance, new object[] { 3.0d });
        }      
        
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMinimalFloat()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMinimalFloat");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 0.0f }));
        }


        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMinimalFloat()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMinimalFloat");

            method.Invoke(instance, new object[] { 3.0f });
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalInt()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMaximumInt");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6 }));
        }

        
        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalInt()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMaximumInt");

            method.Invoke(instance, new object[] { 0 });
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalDouble()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMaximumDouble");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6.0d }));
        }


        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalDouble()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMaximumDouble");

            method.Invoke(instance, new object[] { 0.0d });
        }

        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForMaximalFloat()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMaximumFloat");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 6.0f }));
        }


        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForMaximalFloat()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForMaximumFloat");

            method.Invoke(instance, new object[] { 0.0f });
        }    
        
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeInt()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeInt");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5 }));
        }


        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeInt()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeInt");

            method.Invoke(instance, new object[] { 3 });
        }    
        
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeDouble()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeDouble");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5.0d }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeDouble()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeDouble");

            method.Invoke(instance, new object[] { 3.0d });
        }        
        
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeFloat()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeFloat");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { 5.0f }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeFloat()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeFloat");

            method.Invoke(instance, new object[] { 3.0f });
        }        
        
        /*
        [TestMethod]
        public void CorrectlyThrowsArgumentOutOfRangeExceptionExceptionForOutOfRangeString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeString");

            CallMethodAndExpectException<ArgumentOutOfRangeException>(() => method.Invoke(instance, new object[] { "z" }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentOutOfRangeExceptionExceptionForOutOfRangeString()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOutOfRangeString");

            method.Invoke(instance, new object[] { "d" });
        }
        */

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotInheritsFrom()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForInheritsFrom");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { new Exception() }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForInheritsFrom()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForInheritsFrom");

            method.Invoke(instance, new object[] { new ArgumentNullException() });
        }        
        
        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotInheritsFrom2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForInheritsFrom2");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(Exception) }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForInheritsFrom2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForInheritsFrom2");

            method.Invoke(instance, new object[] { typeof(ArgumentNullException) });
        }   


        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotTypeOf()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfType");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { new object() }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForTypeOf()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfType");

            method.Invoke(instance, new object[] { 2 });
        }        
        
        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotTypeOf2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfType2");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(object) }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForTypeOf2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfType2");

            method.Invoke(instance, new object[] { typeof(int) });
        }      

        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotInterfaceImplemented()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfImplementsInterface");

            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new[] { new object() }));
        }

        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForNotInterfaceImplemented()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfImplementsInterface");

            method.Invoke(instance, new object[] { 2 });
        }        
        
        [TestMethod]
        public void CorrectlyThrowsArgumentExceptionForNotInterfaceImplemented2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfImplementsInterface2");
       
            CallMethodAndExpectException<ArgumentException>(() => method.Invoke(instance, new object[] { typeof(object) }));
        }
      
        [TestMethod]
        public void CorrectlyThrowsNoArgumentExceptionForNotInterfaceImplemented2()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ArgumentChecksClass");

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("CheckForOfImplementsInterface2");

            method.Invoke(instance, new object[] { typeof(int) });
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