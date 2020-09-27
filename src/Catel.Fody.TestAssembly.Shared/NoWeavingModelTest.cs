// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoWeavingModelTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Catel.Data;

    [NoWeaving]
    public class NoWeavingModelTest : ModelBase
    {
        public string FirstName { get; set; }
    }

    public class NoPropertyWeavingModelTest : ModelBase
    {
        #region Properties
        [NoWeaving]
        public string FirstName { get; set; }

        public string LastName { get; set; }
        #endregion
    }
}