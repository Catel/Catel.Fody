// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentChecksClass.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System;

    public class ArgumentChecksClass
    {
        public void CheckForMinimalInt([Minimal(1)]int myValue)
        {
            // Argument.IsMinimal<int>("myValue", myValue, 1);
        }

        public void CheckForMinimalDouble([Minimal(1.0d)]double myValue)
        {
            // Argument.IsMinimal<double>("myValue", myValue, 1.0d);
        }

        public void CheckForMinimalFloat([Minimal(1.0f)]float myValue)
        {
            // Argument.IsMinimal<int>("myValue", myValue, 1.0f);
        }

        public void CheckForMaximumInt([Maximum(5)]int myValue)
        {
            // Argument.IsMaximum<int>("myValue", myValue, 1);
        }

        public void CheckForMaximumDouble([Maximum(5.0d)]double myValue)
        {
            // Argument.IsMaximum<double>("myValue", myValue, 1.0d);
        }

        public void CheckForMaximumFloat([Maximum(5.0f)]float myValue)
        {
            // Argument.IsMaximum<int>("myValue", myValue, 1.0f);
        }

        public void CheckForOutOfRangeInt([NotOutOfRange(1, 4)]int myValue)
        {
            // Argument.IsNotOutOfRange<int>("myValue", myValue, 1, 4);
        }

        public void CheckForOutOfRangeString([NotOutOfRange("a", "d")]string myValue)
        {
            // Argument.IsNotOutOfRange<string>("myValue", myValue, "a", "d");
        }

        public void CheckForOutOfRangeDouble([NotOutOfRange(1.0d, 4.0d)]double myValue)
        {
            // Argument.IsNotOutOfRange<double>("myValue", myValue, 1.0d, 4.0d);
        }

        public void CheckForOutOfRangeFloat([NotOutOfRange(1.0f, 4.0f)]float myValue)
        {
            // Argument.IsNotOutOfRange<float>("myValue", myValue, 1.0f, 4.0f);
        }

        public void CheckForInheritsFrom([InheritsFrom(typeof(ArgumentException))]object myObject)
        {
            // Argument.InheritsFrom("myObject", myObject, typeof(ArgumentException));
        }

        public void CheckForInheritsFrom2([InheritsFrom(typeof(ArgumentException))]Type myType)
        {
            Argument.InheritsFrom("myType", myType, typeof(ArgumentException));
        }

        public void CheckForNullOrEmpty([NotNullOrEmpty] string myString)
        {
            // Argument.IsNotNullOrEmpty(() => myString);
        }

        public void CheckForNullOrWhitespace([NotNullOrWhitespace] string myString)
        {
            // Argument.IsNotNullOrWhitespace(() => myString);
        }

        public void CheckForNullOrEmptyArray([NotNullOrEmptyArray] object[] myArray)
        {
            // Argument.IsNotNullOrEmptyArray(() => myArray);
        }

        public void CheckForNull([NotNull] object myObject)
        {
            // Argument.IsNotNull(() => myObject);
        }

        public void CheckForOfType([OfType(typeof(IComparable))]object myObject)
        {
            // Argument.IsOfType(() => myObject, typeof(IComparable));
        }

        public void CheckForOfType2([OfType(typeof(IComparable))]Type myType)
        {
            // Argument.IsOfType(() => myType, typeof(IComparable));
        }

        public void CheckForOfImplementsInterface([OfType(typeof(IComparable))]object myObject)
        {
            // Argument.ImplementsInterface(() => myObject, typeof(IComparable));
        }

        public void CheckForOfImplementsInterface2([OfType(typeof(IComparable))]Type myType)
        {
            // Argument.ImplementsInterface(() => myType, typeof(IComparable));
        }

        public void CheckForMatch([Match("\\d+")] string myString)
        {
            // Argument.IsMatch(() => myString, "\\d+");
        }

        public void CheckForNotMatch([NotMatch("\\d+")] string myString)
        {
            // Argument.IsNotMatch(() => myString, "\\d+");
        }

        public void CheckForNullWithMultipleParameters([NotNull] object myObject1, [NotNull] object myObject2, [NotNull] object myObject3,
            [NotNull] object myObject4, [NotNull] object myObject5, [NotNull] object myObject6, [NotNull] object myObject7)
        {
            //Argument.IsNotNull("myObject1", myObject1);
            //Argument.IsNotNull("myObject2", myObject2);
            //Argument.IsNotNull("myObject3", myObject3);
            //Argument.IsNotNull("myObject4", myObject4);
            //Argument.IsNotNull("myObject5", myObject5);
            //Argument.IsNotNull("myObject6", myObject6);
            //Argument.IsNotNull("myObject7", myObject7);
        }

        [NoWeaving]
        public void NoWeavingCheckForNull([NotNull] object myObject)
        {
        }
    }
}
