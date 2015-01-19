// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentChecksClass.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections;
    using System.Windows.Media;
    using Collections;
    using Reflection;
    using System.Threading.Tasks;

    public class ArgumentChecksAsExpressionsClass
    {
        public class CustomClassType
        {
            
        }

        public struct CustomStructType
        {
            public static CustomStructType FromArgb(byte a, byte r, byte g, byte b)
            {
                return new CustomStructType
                {
                    A = a,
                    R = r,
                    G = g,
                    B = b
                };
            }

            public byte A { get; private set; }
            public byte R { get; private set; }
            public byte G { get; private set; }
            public byte B { get; private set; }
        }

        public void CheckForMinimalInt(int myValue)
        {
            Argument.IsMinimal(() => myValue, 1);
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

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> is <c>null</c> or empty.</exception>
        public void CheckForNullOrEmpty(string myString)
        {
            Argument.IsNotNullOrEmpty(() => myString);
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> is <c>null</c> or whitespace.</exception>
        public void CheckForNullOrWhitespace(string myString)
        {
            Argument.IsNotNullOrWhitespace(() => myString);
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myArray"/> is <c>null</c> or an empty array.</exception>
        public void CheckForNullOrEmptyArray(object[] myArray)
        {
            Argument.IsNotNullOrEmptyArray(() => myArray);
        }

        /// <exception cref="System.ArgumentNullException">The <paramref name="myObject"/> is <c>null</c>.</exception>
        public void CheckForNull(object myObject)
        {
            Argument.IsNotNull(() => myObject);
        }

        public async Task CheckForNullAsync(object myObject)
        {
            Argument.IsNotNull(() => myObject);
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myObject"/> is not of type <see cref="IComparable"/>.</exception>
        public void CheckForOfType(object myObject)
        {
            Argument.IsOfType(() => myObject, typeof(IComparable));
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myType"/> is not of type <see cref="IComparable"/>.</exception>
        public void CheckForOfType2(Type myType)
        {
            Argument.IsOfType(() => myType, typeof(IComparable));
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myObject"/> does not implement the <see cref="IComparable"/> interface.</exception>
        public void CheckForOfImplementsInterface(object myObject)
        {
            Argument.ImplementsInterface(() => myObject, typeof(IComparable));
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myType"/> does not implement the <see cref="IComparable"/> interface.</exception>
        public void CheckForOfImplementsInterface2(Type myType)
        {
            Argument.ImplementsInterface(() => myType, typeof(IComparable));
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> doesn't match with pattern <c><![CDATA[\\d+]]></c>.</exception>
        public void CheckForMatch(string myString)
        {
            Argument.IsMatch(() => myString, "\\d+");
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> does match with pattern <c><![CDATA[\\d+]]></c>.</exception>
        public void CheckForNotMatch(string myString)
        {
            Argument.IsNotMatch(() => myString, "\\d+");
        }

        public void CheckForNullWithMultipleParameters(CustomClassType customClassType, Color colorStruct, IEnumerable rawCollection, IList filteredCollection)
        {
            Argument.IsNotNull(() => customClassType);
            Argument.IsNotNull(() => colorStruct);
            Argument.IsNotNull(() => rawCollection);
            Argument.IsNotNull(() => filteredCollection);

            var finalStruct = CustomStructType.FromArgb(colorStruct.A, colorStruct.R, colorStruct.G, colorStruct.B);

            IDisposable suspendToken = null;
            var filteredCollectionType = filteredCollection.GetType();
            if (filteredCollectionType.IsGenericTypeEx() && filteredCollectionType.GetGenericTypeDefinitionEx() == typeof(FastObservableCollection<>))
            {
                suspendToken = (IDisposable)filteredCollectionType.GetMethodEx("SuspendChangeNotifications").Invoke(filteredCollection, null);
            }

            filteredCollection.Clear();

            // Removed external code for simplicity of test

            if (suspendToken != null)
            {
                suspendToken.Dispose();
            }
        }

        public void CheckForNullWithMultipleParametersWithoutContent(CustomClassType customClassType, Color colorStruct, IEnumerable rawCollection, IList filteredCollection)
        {
            Argument.IsNotNull(() => customClassType);
            Argument.IsNotNull(() => colorStruct);
            Argument.IsNotNull(() => rawCollection);
            Argument.IsNotNull(() => filteredCollection);
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

            if (suspendToken != null)
            {
                suspendToken.Dispose();
            }
        }
    }
}