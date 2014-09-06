// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentChecksClass.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System;

    public class ArgumentChecksAsExpressionsClass
    {
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

        public void CheckForNullWithMultipleParameters(object myObject1, object myObject2, object myObject3)
        {
            Argument.IsNotNull(() => myObject1);
            Argument.IsNotNull(() => myObject2);
            Argument.IsNotNull(() => myObject3);
        }
    }
}