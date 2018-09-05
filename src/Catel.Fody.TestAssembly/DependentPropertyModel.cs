// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependentPropertyModel.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System;
    using System.Collections.Generic;
    using Data;
    using MVVM;

    public class DependentPropertyModel : ModelBase
    {
        #region Properties
        public string FirstName { get; set; }

        // Using full property to check if these are supported as well
        public string MiddleName
        {
            get { return GetValue<string>(MiddleNameProperty); }
            set { SetValue(MiddleNameProperty, value); }
        }

        public static readonly PropertyData MiddleNameProperty = RegisterProperty("MiddleName", typeof(string));

        public string LastName { get; set; }

        public int Age { get; set; }

        public string FullName
        {
            get
            {
                var items = new List<string>();

                if (!string.IsNullOrWhiteSpace(FirstName))
                {
                    items.Add(FirstName);
                }

                if (!string.IsNullOrWhiteSpace(MiddleName))
                {
                    items.Add(MiddleName);
                }

                if (!string.IsNullOrWhiteSpace(LastName))
                {
                    items.Add(LastName);
                }

                return string.Join(" ", items);
            }
        }
        #endregion
    }

    public class Person : ModelBase
    {
        public string FirstName { get; set; }

        public string Surnames { get; set; }
    }

    public class DependentPersonViewModel : ViewModelBase
    {
        [Model]
        public Person Person { get; private set; }

        [ViewModelToModel]
        public string FirstName { get; set; }

        [ViewModelToModel]
        public string Surnames { get; set; }

        public string FullName
        {
            get { return $"{FirstName} {Surnames}"; }
        }
    }

    public class DependentPropertyModelWithExistingOnPropertyChanged : DependentPropertyModel
    {
        #region Properties
        public string Profile
        {
            get { return $"Name:{FullName}, Age:{Age}".Trim(); }
        }

        #endregion
    }

    public class DetailedDependentPropertyModel : DependentPropertyModel
    {
        #region Properties
        public string Profile
        {
            get { return $"Name:{FullName}, Age:{Age}".Trim(); }
        }
        #endregion
    }
}
