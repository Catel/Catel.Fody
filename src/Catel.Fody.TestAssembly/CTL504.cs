// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL504.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Data;

    public class CTL504_Model : ModelBase
    {
        public string IsoCode
        {
            get { return this.GetValue<string>(IsoCodeProperty); }
            set { this.SetValue(IsoCodeProperty, value); }
        }

        public static readonly PropertyData IsoCodeProperty = RegisterProperty<CTL504_Model, string>(o => o.IsoCode);

        public string Description
        {
            get { return this.GetValue<string>(DescriptionProperty); }
            set { this.SetValue(DescriptionProperty, value); }
        }

        public static readonly PropertyData DescriptionProperty = RegisterProperty<CTL504_Model, string>(o => o.Description);
    }
}