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
        public void CheckForInheritsFrom([InheritsFrom(typeof(ArgumentException))]object myObject)
        {
            // Argument.InheritsFrom("myObject", myObject, typeof(ArgumentException));
        }

        public void CheckForInheritsFrom2([InheritsFrom(typeof(ArgumentException))]Type myType)
        {
            Argument.InheritsFrom("myType", myType, typeof(ArgumentException));
        }
        
        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> is <c>null</c> or empty.</exception>
        public void CheckForNullOrEmpty([NotNullOrEmpty] string myString)
        {
            // Argument.IsNotNullOrEmpty(() => myString);
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> is <c>null</c> or whitespace.</exception>
        public void CheckForNullOrWhitespace([NotNullOrWhitespace] string myString)
        {
            // Argument.IsNotNullOrWhitespace(() => myString);
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myArray"/> is <c>null</c> or an empty array.</exception>
        public void CheckForNullOrEmptyArray([NotNullOrEmptyArray] object[] myArray)
        {
            // Argument.IsNotNullOrEmptyArray(() => myArray);
        }

        /// <exception cref="System.ArgumentNullException">The <paramref name="myObject"/> is <c>null</c>.</exception>
        public void CheckForNull([NotNull] object myObject)
        {
            // Argument.IsNotNull(() => myObject);
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myObject"/> is not of type <see cref="IComparable"/>.</exception>
        public void CheckForOfType([OfType(typeof(IComparable))]object myObject)
        {
            // Argument.IsOfType(() => myObject, typeof(IComparable));
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myType"/> is not of type <see cref="IComparable"/>.</exception>
        public void CheckForOfType2([OfType(typeof(IComparable))]Type myType)
        {
            // Argument.IsOfType(() => myType, typeof(IComparable));
        }    
        
        /// <exception cref="System.ArgumentException">The <paramref name="myObject"/> does not implement the <see cref="IComparable"/> interface.</exception>
        public void CheckForOfImplementsInterface([OfType(typeof(IComparable))]object myObject)
        {
            // Argument.ImplementsInterface(() => myObject, typeof(IComparable));
        }       
        
        /// <exception cref="System.ArgumentException">The <paramref name="myType"/> does not implement the <see cref="IComparable"/> interface.</exception>
        public void CheckForOfImplementsInterface2([OfType(typeof(IComparable))]Type myType)
        {
            // Argument.ImplementsInterface(() => myType, typeof(IComparable));
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> doesn't match with pattern <c><![CDATA[\\d+]]></c>.</exception>
        public void CheckForMatch([Match("\\d+")] string myString)
        {
            // Argument.IsMatch(() => myString, "\\d+");
        }

        /// <exception cref="System.ArgumentException">The <paramref name="myString"/> does match with pattern <c><![CDATA[\\d+]]></c>.</exception>
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
    }
}