﻿namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections;
    using System.Linq;
    using Collections;
    using Reflection;
    using System.Threading.Tasks;

    public class CustomClassType
    {
    }

    public class ArgumentChecksAsExpressionsClass
    {
        public void CheckForMinimalInt(int myValue)
        {
            Argument.IsMinimal(() => myValue, 1);
        }

        public void CheckForMinimalInt_Expected(int myValue)
        {
            Argument.IsMinimal("myValue", myValue, 1);
        }

        public void CheckForMinimalDouble(double myValue)
        {
            Argument.IsMinimal(() => myValue, 1.0d);
        }

        public void CheckForMinimalFloat(float myValue)
        {
            Argument.IsMinimal(() => myValue, 1.0f);
        }

        public void CheckForMaximumInt(int myValue)
        {
            Argument.IsMaximum(() => myValue, 1);
        }

        public void CheckForMaximumDouble(double myValue)
        {
            Argument.IsMaximum(() => myValue, 1.0d);
        }

        public void CheckForMaximumFloat(float myValue)
        {
            Argument.IsMaximum(() => myValue, 1.0f);
        }

        public void CheckForOutOfRangeInt(int myValue)
        {
            Argument.IsNotOutOfRange(() => myValue, 1, 4);
        }

        public void CheckForOutOfRangeInt_Expected(int myValue)
        {
            Argument.IsNotOutOfRange("myValue", myValue, 1, 4);
        }

        public void CheckForOutOfRangeString(string myValue)
        {
            Argument.IsNotOutOfRange(() => myValue, "a", "d");
        }

        public void CheckForOutOfRangeDouble(double myValue)
        {
            Argument.IsNotOutOfRange(() => myValue, 1.0d, 4.0d);
        }

        public void CheckForOutOfRangeFloat(float myValue)
        {
            Argument.IsNotOutOfRange(() => myValue, 1.0f, 4.0f);
        }

        public void CheckForNullOrEmpty(string myString)
        {
            Argument.IsNotNullOrEmpty(() => myString);
        }

        public void CheckForNullOrWhitespace(string myString)
        {
            Argument.IsNotNullOrWhitespace(() => myString);
        }

        public void CheckForNullOrEmptyArray(object[] myArray)
        {
            Argument.IsNotNullOrEmptyArray(() => myArray);
        }

        public void CheckForNull(object myObject)
        {
            Argument.IsNotNull(() => myObject);
        }

#pragma warning disable 1998
        public async Task CheckForNullAsync(object myObject)
#pragma warning restore 1998
        {
            Argument.IsNotNull(() => myObject);

            //Console.WriteLine(myObject);
        }

#pragma warning disable 1998
        public async Task CheckForNullAsync_ExpectedAsync(object myObject)
#pragma warning restore 1998
        {
            Argument.IsNotNull("myObject", myObject);

            //Console.WriteLine(myObject);
        }

#pragma warning disable 1998
        public async Task CheckForNullAsync_MultipleParametersAsync(object myObject1, object myObject2, object myObject3)
#pragma warning restore 1998
        {
            Argument.IsNotNull(() => myObject1);
            Argument.IsNotNull(() => myObject2);
            Argument.IsNotNull(() => myObject3);

            //Console.WriteLine(myObject);
        }

#pragma warning disable 1998
        public async Task CheckForNullAsync_MultipleParameters_ExpectedAsync(object myObject1, object myObject2, object myObject3)
#pragma warning restore 1998
        {
            Argument.IsNotNull("myObject1", myObject1);
            Argument.IsNotNull("myObject2", myObject2);
            Argument.IsNotNull("myObject3", myObject3);

            //Console.WriteLine(myObject);
        }

#pragma warning disable 1998
        public async Task CheckForNullAsync_MultipleParameters_UsagesAsync(object myObject1, object myObject2, object myObject3)
#pragma warning restore 1998
        {
            Argument.IsNotNull(() => myObject1);
            Argument.IsNotNull(() => myObject2);
            Argument.IsNotNull(() => myObject3);

            Console.WriteLine(myObject2);
        }

#pragma warning disable 1998
        public async Task CheckForNullAsync_MultipleParameters_Usages_ExpectedAsync(object myObject1, object myObject2, object myObject3)
#pragma warning restore 1998
        {
            Argument.IsNotNull("myObject1", myObject1);
            Argument.IsNotNull("myObject2", myObject2);
            Argument.IsNotNull("myObject3", myObject3);

            Console.WriteLine(myObject2);
        }

        public void CheckForOfType(object myObject)
        {
            Argument.IsOfType(() => myObject, typeof(IComparable));
        }

        public void CheckForOfType2(Type myType)
        {
            Argument.IsOfType(() => myType, typeof(IComparable));
        }

        public void CheckForOfImplementsInterface(object myObject)
        {
            Argument.ImplementsInterface(() => myObject, typeof(IComparable));
        }

        public void CheckForOfImplementsInterface2(Type myType)
        {
            Argument.ImplementsInterface(() => myType, typeof(IComparable));
        }

        public void CheckForMatch(string myString)
        {
            Argument.IsMatch(() => myString, "\\d+");
        }

        public void CheckForNotMatch(string myString)
        {
            Argument.IsNotMatch(() => myString, "\\d+");
        }

        private static readonly object[] Objects = new object[] { null, null, null };

        public void CheckForNullWithInnerExpression(object obj)
        {
            Argument.IsNotNull(() => obj);

            var filteredObjects = Objects.Select(x => x == obj);
        }

        public void CheckForNullWithMultipleParameters(CustomClassType customClassType, IEnumerable rawCollection, IList filteredCollection)
        {
            Argument.IsNotNull(() => customClassType);
            Argument.IsNotNull(() => rawCollection);
            Argument.IsNotNull(() => filteredCollection);

            IDisposable suspendToken = null;
            var filteredCollectionType = filteredCollection.GetType();
            if (filteredCollectionType.IsGenericTypeEx() && filteredCollectionType.GetGenericTypeDefinitionEx() == typeof(FastObservableCollection<>))
            {
                suspendToken = (IDisposable)filteredCollectionType.GetMethodEx("SuspendChangeNotifications").Invoke(filteredCollection, null);
            }

            filteredCollection.Clear();

            // Removed external code for simplicity of test

            suspendToken?.Dispose();
        }

        public void CheckForNullWithMultipleParametersWithoutContent(CustomClassType customClassType, IEnumerable rawCollection, IList filteredCollection)
        {
            Argument.IsNotNull(() => customClassType);
            Argument.IsNotNull(() => rawCollection);
            Argument.IsNotNull(() => filteredCollection);
        }

        public void CheckForNullWithMultipleParametersWithoutContent_Expected(CustomClassType customClassType, IEnumerable rawCollection, IList filteredCollection)
        {
            Argument.IsNotNull("customClassType", customClassType);
            Argument.IsNotNull("rawCollection", rawCollection);
            Argument.IsNotNull("filteredCollection", filteredCollection);
        }

        public void CheckForNullWithMultipleParametersContentOnly(CustomClassType customClassType, IEnumerable rawCollection, IList filteredCollection)
        {
            IDisposable suspendToken = null;
            var filteredCollectionType = filteredCollection.GetType();
            if (filteredCollectionType.IsGenericTypeEx() && filteredCollectionType.GetGenericTypeDefinitionEx() == typeof(FastObservableCollection<>))
            {
                suspendToken = (IDisposable)filteredCollectionType.GetMethodEx("SuspendChangeNotifications").Invoke(filteredCollection, null);
            }

            filteredCollection.Clear();

            // Removed external code for simplicity of test

            suspendToken?.Dispose();
        }

        public void CheckForNullForGenericArgument<T>(T value)
            where T : class
        {
            Argument.IsNotNull(() => value);

        }

        public void CheckForNullForGenericArgumentWithPredicate<TEventArgs>(TEventArgs value)
            where TEventArgs : EventArgs
        {
            Argument.IsNotNull(() => value);

        }

        public void CheckForNullForGenericArgumentWithPredicate_Expected<TEventArgs>(TEventArgs value)
            where TEventArgs : EventArgs
        {
            Argument.IsNotNull("value", value);

        }

        public void CheckForNullForGenericArgumentWithPredicateAndInstantiation<TEventArgs>(object obj, string someString)
            where TEventArgs : EventArgs, new()
        {
            Argument.IsNotNull(() => obj);

            var newObj = new TEventArgs();
        }

        public void CheckForNullForGenericArgumentWithPredicateAndInstantiation_Expected<TEventArgs>(MyTestClass obj, string someString)
            where TEventArgs : EventArgs, new()
        {
            Argument.IsNotNull("obj", obj);

            var newObj = new TEventArgs();

            obj.SomeMethod();
        }
    }

    public class MyTestClass
    {
        public void SomeMethod()
        {

        }
    }
}
