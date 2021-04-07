// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL569.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System;
    using MVVM;

    public class CTL569_ViewModel : ViewModelBase
    {
        public string ReleaseGroup { get; set; }
        public object SelectedShow { get; set; }
        public object SelectedFeed { get; set; }
        public string SearchTerms { get; set; }

        public bool SearchIsEnabled
        {
            get { return !string.IsNullOrWhiteSpace(SearchTerms) && SelectedFeed is not null; }
        }

#if CATEL_5
        protected override void OnPropertyChanged(Catel.Data.AdvancedPropertyChangedEventArgs e)
#elif CATEL_6
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
#endif
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case "ReleaseGroup":
                case "SelectedShow":
                    Console.WriteLine(e.PropertyName);
                    break;
            }
        }
    }
}
