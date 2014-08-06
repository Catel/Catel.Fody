// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependentPropertyModel.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Data;

    public class DependentPropertyModel : ModelBase
    {
        #region Properties
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName).Trim(); }
        }
        #endregion
    }

    public class DetailedDependentPropertyModel : DependentPropertyModel
    {
        #region Properties
        public string Profile
        {
            get { return string.Format("Name:{0}, Age:{1}", FullName, Age).Trim(); }
        }
        #endregion
    }
}