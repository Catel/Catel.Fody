// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentChecksClass.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    public class ArgumentChecksClass
    {
        public void CheckForNull([NotNull] object myObject)
        {
            Argument.IsNotNull("myObject", myObject);
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