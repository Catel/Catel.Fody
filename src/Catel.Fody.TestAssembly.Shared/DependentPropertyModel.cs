namespace Catel.Fody.TestAssembly;

using System;
using System.Collections.Generic;
using Catel.Fody.TestAssembly.Bugs.GH0511;
using Data;
using MVVM;

public class DependentPropertyModel : ModelBase
{
    public string FirstName { get; set; }

    // Using full property to check if these are supported as well
    public string MiddleName
    {
        get { return GetValue<string>(MiddleNameProperty); }
        set { SetValue(MiddleNameProperty, value); }
    }

#if CATEL_5
    public static readonly PropertyData MiddleNameProperty = RegisterProperty("MiddleName", typeof(string));
#elif CATEL_6_OR_HIGHER
    public static readonly IPropertyData MiddleNameProperty = RegisterProperty<string>("MiddleName");
#endif

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
}

public class Person : ModelBase
{
    public string FirstName { get; set; }

    public string Surnames { get; set; }
}

#if CATEL_7_OR_HIGHER
public class DependentPersonViewModel : FeaturedViewModelBase
#else
public class DependentPersonViewModel : ViewModelBase
#endif
{
#if CATEL_7_OR_HIGHER
    public DependentPersonViewModel(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {

    }
#endif

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
    public string Profile
    {
        get { return $"Name:{FullName}, Age:{Age}".Trim(); }
    }
}

public class DetailedDependentPropertyModel : DependentPropertyModel
{
    public string Profile
    {
        get { return $"Name:{FullName}, Age:{Age}".Trim(); }
    }
}
